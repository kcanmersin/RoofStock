using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Core.Data;
using Core.Service.JWT;
using MediatR;
using System.Reflection;
using FluentValidation;
using Core.Service.StockApi;
using Quartz;
using Core.Service.Email;
using Core.Middlewares.ExceptionHandling;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Core.Features;
using Core.Data.Entity.User;
using Microsoft.AspNetCore.Identity;
using Core.Notification.StockPriceAlert;
using Core.Service.OrderBackgroundService;
using Microsoft.AspNetCore.Builder;
using Core.Service.PredictionService;

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



            // Register scoped services
            services.AddScoped<StockPriceAlertService>();
            services.AddScoped<IEmailService, EmailService>();

            //prediction service
            services.AddHttpClient<IPredictService, PredictService>(client =>
            {
                client.BaseAddress = new Uri("http://127.0.0.1:5000");
            });



            // Connection string
            var defaultConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnectionROOF") ?? configuration["ConnectionStrings:DefaultConnection"];

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(defaultConnectionString));

            services.AddMemoryCache();


            //AddJwtAuthentication add this
            services.AddJwtAuthentication(configuration);

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


            services.AddQuartzExtension(configuration);


            return services;
        }

        public static IApplicationBuilder UseCoreLayerRecurringJobs(this IApplicationBuilder app)
        {


            return app;
        }
    }
}
