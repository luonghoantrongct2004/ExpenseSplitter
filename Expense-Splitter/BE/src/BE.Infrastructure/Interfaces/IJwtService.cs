using BE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BE.Infrastructure.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    string? GetUserIdFromToken(string token);
    DateTime GetTokenExpiry(string token);
}
