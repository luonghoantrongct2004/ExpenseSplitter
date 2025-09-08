using BE.Application.DTOs;

namespace BE.Infrastructure.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);

    Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto);

    Task<UserDto?> GetCurrentUserAsync(Guid userId);

    Task<bool> RevokeRefreshTokenAsync(string refreshToken);

    Task<bool> RevokeAllUserTokensAsync(Guid userId);

    Task UpdateLastLoginAsync(Guid userId);
}