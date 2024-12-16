using API.Middlewares.ExceptionHandling;
using Core.Extensions;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Prometheus;
using Serilog;
using Serilog.Filters;
using System.Reflection;
using System.Threading.RateLimiting;

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
                PermitLimit = 1500,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            })
    );
    options.AddFixedWindowLimiter(policyName: "default", options =>
    {
        options.PermitLimit = 1500;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueLimit = 15;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.AutoReplenishment = true;
    }).RejectionStatusCode = 429;
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



app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.UseStaticFiles();


app.MapControllers();

app.Run();
