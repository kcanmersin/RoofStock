using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Core.Data;
using Core.Service.JWT;
using MediatR;
using System.Reflection;
using FluentValidation;
using Core.Service.StockApi;
using Hangfire;
using Quartz;
using Hangfire.PostgreSql;
using Core.Service.Email;
using Core.Middlewares.ExceptionHandling;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.Hangfire;
using Core.Health;
using Core.Features;
using Core.Data.Entity.User;
using Microsoft.AspNetCore.Identity;
using Core.Notification.StockPriceAlert;
using Core.Service.OrderBackgroundService;
using Microsoft.AspNetCore.Builder;
using Core.Service.RabbitMQEmailService;
using RabbitMQ.Client;

namespace Core.Extensions
{
    public static class CoreLayerExtensions
    {
        public static IServiceCollection LoadCoreLayerExtension(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<AppUser, AppRole>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 6;
            })
            .AddRoleManager<RoleManager<AppRole>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Register RabbitMQ components as singletons
            services.AddSingleton(sp =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
                    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
                };
                return factory.CreateConnection();
            });

            services.AddSingleton(sp =>
            {
                var connection = sp.GetRequiredService<IConnection>();
                return connection.CreateModel();
            });

            services.AddSingleton<EmailConsumerService>(); // EmailConsumerService should be a singleton

            // Register scoped services
            services.AddScoped<StockPriceAlertService>();
            services.AddScoped<IEmailService, EmailService>();

            // Connection string
            var defaultConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? configuration["ConnectionStrings:DefaultConnection"];

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(defaultConnectionString));

            // Stock API health check
            services.AddHttpClient<StockApiHealthCheck>();

            // Health checks configuration
            services.AddHealthChecks()
                .AddNpgSql(
                    connectionString: defaultConnectionString,
                    name: "PostgreSQL Health Check",
                    failureStatus: HealthStatus.Unhealthy)
                .AddCheck<StockApiHealthCheck>("Stock API Health Check")
                .AddHangfire(hangfireOptions =>
                {
                    hangfireOptions.MaximumJobsFailed = 5;
                    hangfireOptions.MinimumAvailableServers = 1;
                }).AddCheck<QuartzHealthCheck>("Quartz Health Check")
                .AddSmtpHealthCheck(opt =>
                {
                    var portString = Environment.GetEnvironmentVariable("EMAIL_PORT") ?? configuration["Email:Smtp:Port"];

                    if (!int.TryParse(portString, out int port))
                    {
                        throw new InvalidOperationException($"Invalid SMTP port number: {portString}");
                    }

                    opt.Host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? configuration["Email:Smtp:Host"];
                    opt.Port = port;
                }, name: "SMTP Health Check");

            // Memory cache
            services.AddMemoryCache();

            // JWT settings configuration
            var jwtSettings = new JwtSettings
            {
                Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? configuration["JwtSettings:Secret"],
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? configuration["JwtSettings:Issuer"],
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? configuration["JwtSettings:Audience"],
                ExpiryMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRYMINUTES"), out var expiryMinutes)
                                ? expiryMinutes
                                : int.Parse(configuration["JwtSettings:ExpiryMinutes"])
            };
            services.AddSingleton(jwtSettings);

            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IBuyService, BuyService>();
            services.AddScoped<ISellService, SellService>();

            // MediatR and EmailService
            services.AddMediatR(Assembly.GetExecutingAssembly());

            // Currency Conversion Service
            services.AddHttpClient<CurrencyConversionService>();

            // FluentValidation setup
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // StockApiService setup
            services.AddScoped<IStockApiService, StockApiService>();
            services.AddHttpClient<IStockApiService, StockApiService>(client =>
            {
                var stockApiBaseUrl = Environment.GetEnvironmentVariable("STOCKAPI_BASEURL") ?? configuration["StockApiSettings:BaseUrl"];
                client.BaseAddress = new Uri(stockApiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("X-Finnhub-Token", Environment.GetEnvironmentVariable("STOCKAPI_APIKEY") ?? configuration["StockApiSettings:ApiKey"]);
            });

            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(defaultConnectionString));
            services.AddHangfireServer();

            services.AddQuartzExtension(configuration);
            return services;
        }

        public static IApplicationBuilder UseCoreLayerRecurringJobs(this IApplicationBuilder app)
        {
            var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

            recurringJobManager.AddOrUpdate<OrderBackgroundService>(
                "CheckAndProcessOrders",
                x => x.CheckAndProcessOrders(),
                Cron.Minutely,
                TimeZoneInfo.Local,
                "high-priority"
            );

            recurringJobManager.AddOrUpdate<StockPriceAlertService>(
                "CheckAndTriggerStockPriceAlerts",
                x => x.CheckAndTriggerAlertsAsync(),
                Cron.Minutely,
                TimeZoneInfo.Local,
                "high-priority"
            );

            // Start the EmailConsumerService to process messages from RabbitMQ
            var emailConsumerService = app.ApplicationServices.GetRequiredService<EmailConsumerService>();
            Task.Run(() => emailConsumerService.Start());

            return app;
        }
    }
}
