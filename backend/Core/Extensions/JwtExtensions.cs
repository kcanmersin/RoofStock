using Core.Service.JWT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Core.Extensions
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure and bind JwtSettings from environment variables or configuration
            var jwtSettings = new JwtSettings
            {
                Secret = configuration["JwtSettings:Secret"],
                Issuer = configuration["JwtSettings:Issuer"],
                Audience = configuration["JwtSettings:Audience"],
                ExpiryMinutes = int.Parse(configuration["JwtSettings:ExpiryMinutes"])
            };

            // Register JwtSettings as a singleton service for dependency injection
            services.AddSingleton(jwtSettings);

            // Configure the JWT authentication scheme
            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Register IJwtService implementation as a singleton for dependency injection
            services.AddSingleton<IJwtService, JwtService>();

            return services;
        }
    }
}
