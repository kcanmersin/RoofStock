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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


RecurringJob.AddOrUpdate<OrderBackgroundService>(
    "CheckAndProcessOrders", // İşin adı
    x => x.CheckAndProcessOrders(), // Çalıştırılacak metod
    Cron.Minutely // Her dakika çalıştır
);

app.MapControllers();
app.Run();
