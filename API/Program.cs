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
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Core.Service.OrderBackgroundService;
using API.Hubs;
using API.Notification.StockPriceAlert;

var builder = WebApplication.CreateBuilder(args);

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

// Statik dosya middleware'ını ekleyin
builder.Services.AddScoped<StockPriceMonitorService>();
builder.Services.AddScoped<StockPriceAlertService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// Static files middleware burada kullanılmalı
app.UseStaticFiles();

app.UseHangfireDashboard();

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
