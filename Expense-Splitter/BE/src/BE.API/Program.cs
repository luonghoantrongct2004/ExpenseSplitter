using BE.API.Extensions;
using BE.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Đăng ký tất cả services cần thiết cho application
/// Click vào icon này link: ServiceExtensions.cs#AddApplicationServices
/// </summary>

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     /// <summary>
//     /// Cấu hình Swagger UI cho development
//     /// link: SwaggerExtensions.cs#UseSwaggerDocumentation
//     /// </summary>
//     app.UseSwaggerDocumentation();
// }
if (app.Environment.IsProduction())
{
    app.UseMiddleware<SwaggerAuthMiddleware>();
}
app.UseHttpsRedirection();
/// <summary>
/// CORS configuration
/// link: CorsExtensions.cs#AddCorsConfiguration
/// Policy: AllowSpecificOrigins
/// </summary>
app.UseCors("AllowSpecificOrigins");
/// <summary>
/// Custom middleware pipeline
/// link: MiddlewareExtensions.cs#UseCustomMiddleware
/// Includes: GlobalErrorHandlingMiddleware
/// </summary>
app.UseCustomMiddleware();
/// <summary>
/// Authentication & Authorization middleware
/// link: ServiceExtensions.cs#AddAuthenticationConfiguration
/// </summary>
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Chia Tiền Nhóm API đang chạy! 🚀");
app.MapGet("/health", () => Results.Ok(new
{
    status = "Khỏe re! 💪",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
}));

app.Run();
