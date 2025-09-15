// BE.API/Extensions/ServiceExtensions.cs
using BE.Domain.Entities;
using BE.Domain.Interfaces;
using BE.Infrastructure.Data.Repositories;
using BE.Infrastructure.Interfaces;
using BE.Infrastructure.Interfaces.Groups;
using BE.Infrastructure.Mappings;
using BE.Infrastructure.Repositories.Groups;
using BE.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BE.API.Extensions;

public static class ServiceExtensions
{
    private static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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
                OnTokenValidated = async context =>
                {
                    var authService = context.HttpContext.RequestServices
                        .GetRequiredService<IAuthService>();
                    var userId = context.Principal!.GetUserId();

                    await authService.UpdateLastLoginAsync(userId);
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
    /// <summary>
    /// Đăng ký tất cả Repository vào DI Container
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Generic repository - dùng chung cho các entity đơn giản
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // === REPOSITORY CHO TỪNG FEATURE ===

        // Group Management
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
        // User Management
        services.AddScoped<IRepository<User>, Repository<User>>();
        // Expense Management
        services.AddScoped<IRepository<Expense>, Repository<Expense>>();
        // Notification System
        services.AddScoped<IRepository<Notification>, Repository<Notification>>();
        // Activity Logging
        services.AddScoped<IRepository<ActivityLog>, Repository<ActivityLog>>();

        return services;
    }

    /// <summary>
    /// Đăng ký tất cả Business Services vào DI Container
    /// Lưu ý: Thêm service theo thứ tự feature và dependency
    /// </summary>
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // === AUTHENTICATION & AUTHORIZATION SERVICES ===
        // Thứ tự quan trọng: JWT Service phải singleton vì không có state
        services.AddSingleton<IJwtService, JwtService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IAuthService, AuthService>();

        // === CORE BUSINESS SERVICES ===

        // Group Management - Đã implement
        services.AddScoped<IGroupService, GroupService>();

        // TODO: User Management
        // services.AddScoped<IUserService, UserService>();
        // services.AddScoped<IUserPreferenceService, UserPreferenceService>();

        // TODO: Expense Management - Team Expense đang làm
        // services.AddScoped<IExpenseService, ExpenseService>();
        // services.AddScoped<IExpenseSplitService, ExpenseSplitService>();
        // services.AddScoped<IExpenseCalculationService, ExpenseCalculationService>();

        return services;
    }

    /// <summary>
    /// Extension method chính để đăng ký tất cả services
    /// Gọi method này trong Program.cs
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Thứ tự quan trọng - đừng thay đổi nếu không cần thiết

        // 1. Database - phải đầu tiên
        services.AddDatabaseConfiguration(configuration);

        // 2. AutoMapper - cần cho mapping DTOs
        services.AddAutoMapper(typeof(GroupMappingProfile).Assembly);
        services.AddAutoMapper(typeof(UserMappingProfile).Assembly);

        // 3. Authentication & Authorization
        services.AddAuthenticationConfiguration(configuration);

        // 4. Repositories - data layer
        services.AddRepositories();

        // 5. Business Services - business logic layer
        services.AddBusinessServices();

        // 6. API Documentation
        services.AddSwaggerDocumentation();

        // 7. CORS - cross-origin requests
        services.AddCorsConfiguration(configuration);

        // 8. Controllers & API features
        services.AddControllers();

        // 9. Logging
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddDebug();
            // TODO: Thêm Serilog hoặc NLog cho production
            // logging.AddSerilog();
        });

        // TODO: Thêm các services khác
        // services.AddHealthChecks(); // Health check cho monitoring
        // services.AddMemoryCache(); // In-memory caching
        // services.AddHttpClient(); // HTTP client factory
        // services.AddSignalR(); // Real-time notifications

        return services;
    }

}
