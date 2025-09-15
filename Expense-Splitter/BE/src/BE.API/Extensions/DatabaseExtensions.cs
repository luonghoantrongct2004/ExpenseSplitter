using BE.Domain;
using Microsoft.EntityFrameworkCore;

namespace BE.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                if (!string.IsNullOrEmpty(databaseUrl))
                {
                    connectionString = ConvertDatabaseUrl(databaseUrl);
                }
            }

            options.UseNpgsql(connectionString);

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return services;
    }

    private static string ConvertDatabaseUrl(string databaseUrl)
    {
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');

        return $"Host={databaseUri.Host};" +
               $"Port={databaseUri.Port};" +
               $"Database={databaseUri.LocalPath.TrimStart('/')};" +
               $"Username={userInfo[0]};" +
               $"Password={userInfo[1]};" +
               $"SSL Mode=Require;Trust Server Certificate=true";
    }
}
