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
using Hangfire.PostgreSql;
using Core.Service.Email;
using Core.Middlewares.ExceptionHandling;

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

            //add glo
            // public class GlobalExceptionHandlesr : IExceptionHandler
            //services.AddExceptionHandler<GlobalExceptionHandler>();
            //add imemorycache
            services.AddMemoryCache();
            // JWT ayarlarını environment variable'dan çek
            var jwtSettings = new JwtSettings
            {
                Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? configuration["JwtSettings:Secret"],
                Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? configuration["JwtSettings:Issuer"],
                Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? configuration["JwtSettings:Audience"],
                ExpiryMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRYMINUTES"), out var expiryMinutes) ? expiryMinutes : int.Parse(configuration["JwtSettings:ExpiryMinutes"])
            };
            services.AddSingleton(jwtSettings);

            services.AddScoped<IJwtService, JwtService>();

            // MediatR ve EmailService ekle
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddScoped<IEmailService, EmailService>();

            // Currency Conversion Service ekle
            services.AddHttpClient<CurrencyConversionService>();

            // Assembly içinden validatörleri ekle
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // StockApiService'i environment variable kullanarak ekle
            services.AddScoped<IStockApiService, StockApiService>();
            services.AddHttpClient<IStockApiService, StockApiService>(client =>
            {
                var stockApiBaseUrl = Environment.GetEnvironmentVariable("STOCKAPI_BASEURL") ?? configuration["StockApiSettings:BaseUrl"];
                client.BaseAddress = new Uri(stockApiBaseUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("X-Finnhub-Token", Environment.GetEnvironmentVariable("STOCKAPI_APIKEY") ?? configuration["StockApiSettings:ApiKey"]);
            });

            // Hangfire için Postgres konfigürasyonu
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(defaultConnectionString));
            services.AddHangfireServer();

            services.AddQuartzExtension(configuration);
            return services;
        }
    }
}