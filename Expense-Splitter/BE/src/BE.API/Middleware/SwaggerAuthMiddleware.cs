namespace BE.API.Middleware;

public class SwaggerAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _username;
    private readonly string _password;

    public SwaggerAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _username = configuration["Swagger:Username"] ?? "hoantrong";
        _password = configuration["Swagger:Password"] ?? "trong225";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            string authHeader = context.Request.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                var decodedUsernamePassword = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];

                if (username == _username && password == _password)
                {
                    await _next(context);
                    return;
                }
            }

            context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Swagger\"";
            context.Response.StatusCode = 401;
            return;
        }

        await _next(context);
    }
}