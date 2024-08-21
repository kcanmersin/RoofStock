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
            .Enrich.WithProperty("LogType", "Middleware")  
            .WriteTo.File("Logs/middleware.log", 
                          rollingInterval: RollingInterval.Day,
                          outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}",
                          fileSizeLimitBytes: 10485760, 
                          retainedFileCountLimit: 7, 
                          shared: true))

        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.WithProperty<string>("SourceContext", v => v.Contains("ActionFilter")))
            .Enrich.WithProperty("LogType", "ActionFilter") 
            .WriteTo.File("Logs/action.log", 
                          rollingInterval: RollingInterval.Day,
                          outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}",
                          fileSizeLimitBytes: 10485760, 
                          retainedFileCountLimit: 7, 
                          shared: true))

        .WriteTo.Logger(lc => lc
            .Enrich.WithProperty("LogType", "General") 
            .WriteTo.File("Logs/logfile.log", 
                          rollingInterval: RollingInterval.Day,
                          outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}",
                          fileSizeLimitBytes: 10485760, 
                          retainedFileCountLimit: 7, 
                          shared: true))

        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
});


builder.Configuration.AddEnvironmentVariables();

var host = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? builder.Configuration["Email:Smtp:Host"];

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

builder.Services.LoadCoreLayerExtension(builder.Configuration);



builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddSignalR();
builder.Services.AddControllers(options =>
{
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

app.UseMetricServer();

app.UseSerilogRequestLogging();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseResponseCaching();

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

app.UseCors("AllowAllOrigins");
app.UseCoreLayerRecurringJobs(); 

app.UseRouting();

app.UseRateLimiter();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers().RequireRateLimiting("default");
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/h", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapHealthChecksUI();
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.UseStaticFiles();

app.UseHangfireDashboard();

var options = new BackgroundJobServerOptions
{
    Queues = new[] { "high-priority", "low-priority" },
    WorkerCount = Environment.ProcessorCount * 5
};

app.UseHangfireServer(options);

app.MapControllers();
app.MapHub<StockPriceHub>("/stockPriceHub");

app.Run();
