using BE.Domain.Interfaces;
using BE.Infrastructure.Data.Repositories;
using BE.Infrastructure.Interfaces;
using BE.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BE.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAuthenticationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Add Google Auth
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            // Add JWT Service
            services.AddSingleton<IJwtService, JwtService>();

            // Add Auth Service
            services.AddScoped<IAuthService, AuthService>();

            // Configure JWT Authentication
            var jwtSecret = configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT Secret not configured");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var authService = context.HttpContext.RequestServices
                            .GetRequiredService<IAuthService>();
                        var userId = context.Principal!.GetUserId();

                        // Update last login (fire and forget)
                        _ = Task.Run(() => authService.UpdateLastLoginAsync(userId));

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        // Additional extension methods
        public static IServiceCollection AddBusinessServices(
            this IServiceCollection services)
        {
            // Add other business services here
            // services.AddScoped<IExpenseService, ExpenseService>();
            // services.AddScoped<IGroupService, GroupService>();

            return services;
        }
    }
}
