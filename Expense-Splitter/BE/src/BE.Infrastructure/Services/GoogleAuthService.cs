using BE.Application.DTOs;
using BE.Infrastructure.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BE.Infrastructure.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly string _clientId;

    public GoogleAuthService(
        IConfiguration configuration,
        ILogger<GoogleAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _clientId = _configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google ClientId not configured");
    }

    public async Task<GoogleUserInfo?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            _logger.LogInformation($"Google token validated for user: {payload.Email}");

            return new GoogleUserInfo
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                Name = payload.Name ?? payload.Email,
                Picture = payload.Picture
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Google token");
            return null;
        }
    }
}