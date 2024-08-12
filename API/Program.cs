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

// Servisleri kaydet
builder.Services.AddScoped<StockPriceMonitorService>();
builder.Services.AddScoped<StockPriceAlertService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// Use Hangfire Dashboard
app.UseHangfireDashboard();
app.UseHangfireServer();

// Hangfire job'u tanımlama
RecurringJob.AddOrUpdate<OrderBackgroundService>(
    "CheckAndProcessOrders", 
    x => x.CheckAndProcessOrders(), 
    Cron.Minutely 
);

RecurringJob.AddOrUpdate<StockPriceAlertService>(
    "CheckAndTriggerStockPriceAlerts",
    x => x.CheckAndTriggerAlertsAsync(),
    Cron.Minutely 
);

app.MapControllers();
app.MapHub<StockPriceHub>("/stockPriceHub");

app.Run();
