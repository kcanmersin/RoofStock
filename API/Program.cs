using Core.Data;
using Core.Data.Entity.User;
using Core.Extensions;
using Core.Service.JWT;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Hangfire;
using Core.Service.OrderBackgroundService;
using API.Hubs;
using API.Notification.StockPriceAlert;
using Serilog;
using API.Middlewares.ExceptionHandling;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
        options.AddPolicy("fixed", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10, 
                Window = TimeSpan.FromSeconds(10), 
                QueueLimit = 2, 
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true 
            })
    );
    options.AddFixedWindowLimiter(policyName: "default", options =>
    {
        options.PermitLimit = 300; // Number of requests allowed per window
        options.Window = TimeSpan.FromMinutes(1); // Time window duration
        options.QueueLimit = 3; // Max requests that can be queued after limit is reached
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // Queue processing order
        options.AutoReplenishment = true; // Automatically replenish permits after the window expires
        options.QueueProcessingOrder = QueueProcessingOrder.NewestFirst; // Can also use OldestFirst based on preference
    })
    .RejectionStatusCode = 429; // Status code for rejected requests


    options.AddTokenBucketLimiter(policyName: "apiKeyUsage", options =>
    {
        options.TokenLimit = 50; // Number of tokens available
        options.TokensPerPeriod = 10; // Tokens replenished per period
        options.ReplenishmentPeriod = TimeSpan.FromMinutes(1); // Replenishment period
        options.QueueLimit = 5; // Queue limit
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// Logging configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .WriteTo.Console()
    .WriteTo.File("Logs/logfile.log", rollingInterval: RollingInterval.Day)
    .ReadFrom.Configuration(context.Configuration));

builder.Services.LoadCoreLayerExtension(builder.Configuration);

builder.Services.AddIdentity<AppUser, AppRole>(opt =>
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

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<StockPriceMonitorService>();
builder.Services.AddScoped<StockPriceAlertService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseMetricServer();


// Log requests
app.UseSerilogRequestLogging();

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Routing middleware
app.UseRouting();
// Apply rate limiting middleware before routing
app.UseRateLimiter();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers().RequireRateLimiting("default");
});

// Health check endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/h", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecksUI(); // This will add a health checks UI endpoint
});

// Rate limiting applied globally, but can be overridden by specific controllers or actions

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.UseStaticFiles();

app.UseHangfireDashboard();
app.UseResponseCaching();

var options = new BackgroundJobServerOptions
{
    Queues = new[] { "high-priority", "low-priority" },
    WorkerCount = Environment.ProcessorCount * 5
};

app.UseHangfireServer(options);

RecurringJob.AddOrUpdate<OrderBackgroundService>(
    "CheckAndProcessOrders",
    x => x.CheckAndProcessOrders(),
    Cron.Minutely,
    queue: "high-priority"
);

RecurringJob.AddOrUpdate<StockPriceAlertService>(
    "CheckAndTriggerStockPriceAlerts",
    x => x.CheckAndTriggerAlertsAsync(),
    Cron.Minutely,
    queue: "low-priority"
);

app.MapControllers();
app.MapHub<StockPriceHub>("/stockPriceHub");

app.Run();
