using BE.API.Extensions;
using BE.Application.DTOs;
using BE.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHostEnvironment _hostEnvironment;

        public AuthController(IAuthService authService, IHostEnvironment hostEnvironment)
        {
            _authService = authService;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// [DEV ONLY] Lấy token test để dev không cần Google OAuth
        /// </summary>
        [HttpPost("dev-login")]
        [ApiExplorerSettings(IgnoreApi = false)] 
        public async Task<IActionResult> DevLogin([FromBody] DevLoginDto dto)
        {
            // Chỉ cho phép trong môi trường Development
            if (!_hostEnvironment.IsDevelopment())
                return NotFound(new { message = "Endpoint này chỉ dùng trong development thôi nhé! 🚫" });

            try
            {
                var user = await _authService.DevLoginAsync(dto.Email);

                return Ok(new
                {
                    user = user.User,
                    accessToken = user.AccessToken,
                    expiresAt = user.AccessTokenExpiresAt,
                    message = "Dev login thành công! 🎉 (NHỚ XÓA KHI DEPLOY NHÉ!)"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Login với Google như dân chơi
        /// </summary>
        [HttpPost("google")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            try
            {
                // Lấy thông tin thiết bị với IP
                dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                dto.DeviceInfo = Request.Headers["User-Agent"].ToString();

                var result = await _authService.GoogleLoginAsync(dto);

                // Nhét refresh token vào cookie cho bảo mật
                Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.RefreshTokenExpiresAt
                });

                // Không trả refresh token trong body nhé, hack lắm
                return Ok(new
                {
                    user = result.User,
                    accessToken = result.AccessToken,
                    expiresAt = result.AccessTokenExpiresAt,
                    message = "Login thành công! Welcome to the club 🎉"
                });
            }
            catch (UnauthorizedException)
            {
                return Unauthorized(new
                {
                    message = "Ê ê ê! Google token không hợp lệ rồi bro 🤔",
                    hint = "Thử login lại xem, có khi Google nó hết hạn"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Ối dồi ôi! Server tụi mình đang hơi mệt 😅",
                    hint = "Thử lại sau 1 tí nha, hoặc gọi dev dậy fix bug"
                });
            }
        }

        /// <summary>
        /// Làm mới token khi hết hạn
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken([FromBody] RefreshTokenDto? dto)
        {
            try
            {
                // Lấy refresh token từ cookie hoặc body
                var refreshToken = Request.Cookies["refreshToken"] ?? dto?.RefreshToken;

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new
                    {
                        message = "Ủa, refresh token đâu rồi ta? 🤷‍♂️",
                        hint = "Có vẻ bạn chưa login hoặc token đã bay mất"
                    });
                }

                var result = await _authService.RefreshTokenAsync(new RefreshTokenDto
                {
                    RefreshToken = refreshToken
                });

                // Update cookie mới toanh
                Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.RefreshTokenExpiresAt
                });

                return Ok(new
                {
                    accessToken = result.AccessToken,
                    expiresAt = result.AccessTokenExpiresAt,
                    message = "Token mới cóng lanh đây! 🎁"
                });
            }
            catch (UnauthorizedException)
            {
                return Unauthorized(new
                {
                    message = "Token này hết hạn lâu rồi bạn ơi! 😵",
                    hint = "Login lại đi nào"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Refresh token bị kẹt rồi 🔧",
                    hint = "Có lẽ nên logout rồi login lại cho lành"
                });
            }
        }

        /// <summary>
        /// Xem thông tin bản thân (ai đó đang tò mò)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userId = User.GetUserId();
                var user = await _authService.GetCurrentUserAsync(userId);

                if (user == null)
                {
                    return NotFound(new
                    {
                        message = "Ơ kìa, bạn là ai vậy? 🕵️",
                        hint = "Có vẻ account bạn đã bị thanos búng mất"
                    });
                }

                return Ok(new
                {
                    data = user,
                    message = "Đây là bạn nè! 👋"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Không tìm thấy bạn luôn 😱",
                    hint = "Thử logout rồi login lại xem sao"
                });
            }
        }

        /// <summary>
        /// Logout khỏi thiết bị hiện tại
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _authService.RevokeRefreshTokenAsync(refreshToken);
                }

                // Xóa cookie sạch sẽ
                Response.Cookies.Delete("refreshToken");

                return Ok(new
                {
                    message = "Tạm biệt! Hẹn gặp lại bạn sau 👋",
                    emoji = "😢"
                });
            }
            catch (Exception)
            {
                // Kệ lỗi, cứ logout
                Response.Cookies.Delete("refreshToken");
                return Ok(new
                {
                    message = "Logout hơi lỗi nhưng mà... bye bye! 🤷‍♂️",
                    note = "Token đã bị xóa rồi, yên tâm"
                });
            }
        }

        /// <summary>
        /// Logout khỏi tất cả thiết bị (khi bị hack hoặc chia tay)
        /// </summary>
        [HttpPost("logout-all")]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<IActionResult> LogoutAllDevices()
        {
            try
            {
                var userId = User.GetUserId();
                await _authService.RevokeAllUserTokensAsync(userId);

                Response.Cookies.Delete("refreshToken");

                return Ok(new
                {
                    message = "Đã đuổi hết tất cả thiết bị! 💪",
                    note = "Giờ chỉ có bạn mới login lại được thôi"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "Logout all bị fail rồi 😅",
                    hint = "Nhưng mà cookie này đã xóa, thử login lại các thiết bị khác xem"
                });
            }
        }

        /// <summary>
        /// Easter egg endpoint
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            var responses = new[]
            {
                "Pong! 🏓",
                "Còn sống nè bạn! 💓",
                "Server đang chạy phà phà 🚀",
                "Alo alo, mic test 1 2 3 🎤",
                "Đang online, ko phải đang ngủ 😴"
            };

            var random = new Random();
            return Ok(new
            {
                message = responses[random.Next(responses.Length)],
                timestamp = DateTime.UtcNow
            });
        }
    }
}