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

namespace Core.Extensions
{
    public static class CoreLayerExtensions
    {
        public static IServiceCollection LoadCoreLayerExtension(this IServiceCollection services, IConfiguration configuration)
        {
            // Connection string'i environment variable'dan çek

            // Connection string'i oluştur
            var defaultConnectionString =  Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? configuration["ConnectionStrings:DefaultConnection"];

            // E-posta ayarlarını environment variable'lardan veya appsettings'ten çek
            var emailHost = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? configuration["Email:Smtp:Host"];
            var emailPortString = Environment.GetEnvironmentVariable("EMAIL_PORT") ?? configuration["Email:Smtp:Port"];
            var emailUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? configuration["Email:Smtp:Username"];
            var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? configuration["Email:Smtp:Password"];
            var emailFrom = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? configuration["Email:Smtp:From"];


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
            services.AddScoped<IEmailService, EmailService>();

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

            // Hangfire configuration with PostgreSQL storage
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(defaultConnectionString));
            services.AddHangfireServer();

            // Add Quartz extension
            services.AddQuartzExtension(configuration);

            return services;
        }
    }
}