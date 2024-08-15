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


namespace Core.Extensions
{
    public static class CoreLayerExtensions
    {
        public static IServiceCollection LoadCoreLayerExtension(this IServiceCollection services, IConfiguration configuration)
        {
            // Connection string'i environment variable'dan çek
            var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(defaultConnectionString));
            //add stock api health check
            services.AddHttpClient<StockApiHealthCheck>();

            // Health checks configuration
            services.AddHealthChecks()
                .AddNpgSql(
                    connectionString: defaultConnectionString,
                    name: "PostgreSQL Health Check",
                    failureStatus: HealthStatus.Unhealthy)
                .AddCheck<StockApiHealthCheck>("Stock API Health Check").
                AddHangfire(hangfireOptions =>
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


            // var host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? _configuration["Email:Smtp:Host"];
            // var portString = Environment.GetEnvironmentVariable("EMAIL_PORT") ?? _configuration["Email:Smtp:Port"];
            // var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME") ?? _configuration["Email:Smtp:Username"];
            // var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? _configuration["Email:Smtp:Password"];
            // var from = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? _configuration["Email:Smtp:From"];
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
