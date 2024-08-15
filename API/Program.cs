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

var builder = WebApplication.CreateBuilder(args);
//log
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
//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
var app = builder.Build();
//log
app.UseSerilogRequestLogging(); 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
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
