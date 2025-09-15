using Microsoft.AspNetCore.Mvc;

namespace BE.Common
{
    public static class ResultExtensions
    {
        public static ApiResponse<T> ToApiResponse<T>(this Result<T> result, string? successMessage = null)
        {
            return result.Succeeded
                ? ApiResponse<T>.Ok(result.Data!, successMessage ?? Messages.Successed)
                : ApiResponse<T>.Fail(result.Error ?? Messages.Exception);
        }

        public static IActionResult ToActionResult<T>(this Result<T> result, string? successMessage = null)
        {
            if (result.Succeeded)
                return new OkObjectResult(ApiResponse<T>.Ok(result.Data!, successMessage ?? Messages.Successed));

            return new BadRequestObjectResult(ApiResponse<T>.Fail(result.Error ?? Messages.Exception));
        }
    }
}