using BE.Application.DTOs;

namespace BE.Infrastructure.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken);
}