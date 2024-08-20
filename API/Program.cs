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
using Serilog;
using API.Middlewares.ExceptionHandling;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Prometheus;
using Serilog.Sinks.Elasticsearch;

// Add your additional using statements
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog.Filters;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .Enrich.FromLogContext()
        .WriteTo.Console() 
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.WithProperty<string>("SourceContext", v => v.Contains("Middleware")))
            .WriteTo.File("Logs/middleware-log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", fileSizeLimitBytes: 10485760, retainedFileCountLimit: 7, shared: true))
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.WithProperty<string>("SourceContext", v => v.Contains("ActionFilter")))
            .WriteTo.File("Logs/action-log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}", fileSizeLimitBytes: 10485760, retainedFileCountLimit: 7, shared: true))
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.WithProperty<string>("SourceContext", v => v.Contains("Logstash")))
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch:9200"))
            {
                AutoRegisterTemplate = true,
                IndexFormat = "logstash-{0:yyyy.MM.dd}",
                NumberOfShards = 2,
                NumberOfReplicas = 1
            }))
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
});


builder.Configuration.AddEnvironmentVariables();

// Read from environment variable
var host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? builder.Configuration["Email:Smtp:Host"];

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("fixed", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: partition => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 300,
            Window = TimeSpan.FromSeconds(10),
            QueueLimit = 2,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        })
    );
    options.AddFixedWindowLimiter(policyName: "default", options =>
    {
        options.PermitLimit = 300; 
        options.Window = TimeSpan.FromMinutes(1); 
        options.QueueLimit = 3; 
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.AutoReplenishment = true;
    })
    .RejectionStatusCode = 429; 
});

// Load Core Layer
builder.Services.LoadCoreLayerExtension(builder.Configuration);



builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddSignalR();
builder.Services.AddControllers(options =>
{
    // Register the LoggingActionFilter globally
    options.Filters.Add<LoggingActionFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Use Prometheus metric server
app.UseMetricServer();

// Log requests
app.UseSerilogRequestLogging();

// Apply the RequestResponseLoggingMiddleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseResponseCaching();

// Enable Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowAllOrigins");

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
    endpoints.MapHealthChecksUI(); 
});

// Apply global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.UseStaticFiles();

// Use Hangfire dashboard and server
app.UseHangfireDashboard();
app.UseResponseCaching();

var options = new BackgroundJobServerOptions
{
    Queues = new[] { "high-priority", "low-priority" },
    WorkerCount = Environment.ProcessorCount * 5
};

app.UseHangfireServer(options);

// Configure Hangfire recurring jobs

// Map controllers and SignalR hubs
app.MapControllers();
app.MapHub<StockPriceHub>("/stockPriceHub");

app.Run();
