using BE.Domain.Exceptions; // Import exceptions từ domain của mình

namespace BE.API.Middleware
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                CustomException =>
                    (401, "Ê! Chưa đăng nhập mà đòi vào à? 🚫"),

                NotFoundException =>
                    (404, "Tìm hoài không thấy luôn á! 🔍"),

                ForbiddenException =>
                    (403, "Stop! Bạn không có quyền vào đây 🛑"),

                BadRequestException =>
                    (400, "Sai rồi bạn ơi, check lại đi! ❌"),

                InvalidOperationException =>
                    (400, "Làm vậy không được đâu nha! 🤷‍♂️"),

                _ =>
                    (500, "Úi, có gì đó sai sai! Dev đang fix 🔨")
            };

            var response = new
            {
                statusCode,
                message,
                error = exception.Message,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
