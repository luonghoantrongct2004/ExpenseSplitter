using BE.Domain.Entities;
using System.Security.Claims;

namespace BE.Infrastructure.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();

    ClaimsPrincipal? ValidateToken(string token);

    string? GetUserIdFromToken(string token);

    DateTime GetTokenExpiry(string token);
}