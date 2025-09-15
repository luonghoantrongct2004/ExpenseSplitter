namespace BE.API.Middleware;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalErrorHandlingMiddleware>();

        return app;
    }
}
