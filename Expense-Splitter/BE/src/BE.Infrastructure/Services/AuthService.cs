using AutoMapper;
using BE.Application.DTOs;
using BE.Domain.Entities;
using BE.Domain.Interfaces;
using BE.Domain.Specifications;
using BE.Infrastructure.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static BE.Common.CommonData;
using static BE.Domain.Specifications.UserByGoogleIdSpec;

namespace BE.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserPreference> _userPreferenceRepository;
        private readonly IRepository<ActivityLog> _activityLogRepository;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IMapper _mapper;

        public AuthService(IRepository<User> userRepository, 
            IRepository<UserPreference> userPreferenceRepository, 
            IRepository<ActivityLog> activityLogRepository, 
            IRepository<RefreshToken> refreshTokenRepository, 
            IGoogleAuthService googleAuthService, 
            IJwtService jwtService, ILogger<AuthService> logger, 
            IHostEnvironment hostEnvironment, IMapper mapper)
        {
            _userRepository = userRepository;
            _userPreferenceRepository = userPreferenceRepository;
            _activityLogRepository = activityLogRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _googleAuthService = googleAuthService;
            _jwtService = jwtService;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> DevLoginAsync(string email)
        {
            // Chỉ cho phép trong Development
            if (!_hostEnvironment.IsDevelopment())
                throw new UnauthorizedException("Dev login not allowed in production");

            // Tìm hoặc tạo user test
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Name = "Dev User",
                    GoogleId = $"dev_{Guid.NewGuid()}",
                    AvatarUrl = "https://ui-avatars.com/api/?name=Dev+User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
            }

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            return new AuthResponseDto
            {
                User = _mapper.Map<UserDto>(user),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
                RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
            };
        }

        public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
        {
            try
            {
                // 1. Validate Google token
                var googleUser = await _googleAuthService.ValidateGoogleTokenAsync(dto.Token);
                if (googleUser == null)
                {
                    throw new UnauthorizedException("Token của Google sai rồi bro!");
                }

                // 2. Find or create user
                var spec = new UserByGoogleIdSpec(googleUser.GoogleId);
                var user = await _userRepository.FirstOrDefaultAsync(spec);

                if (user == null)
                {
                    user = new User
                    {
                        GoogleId = googleUser.GoogleId,
                        Email = googleUser.Email,
                        Name = googleUser.Name,
                        AvatarUrl = googleUser.Picture,
                        IsActive = true,
                        LastLoginAt = DateTime.UtcNow
                    };

                    user = await _userRepository.AddAsync(user);

                    var userPreference = new UserPreference
                    {
                        UserId = user.Id,
                        DefaultCurrency = "VND",
                        Language = "vi",
                        Timezone = "Asia/Ho_Chi_Minh"
                    };

                    await _userPreferenceRepository.AddAsync(userPreference);
                }
                else
                {
                    // Update existing user
                    user.Name = googleUser.Name;
                    user.AvatarUrl = googleUser.Picture;
                    user.LastLoginAt = DateTime.UtcNow;

                    await _userRepository.UpdateAsync(user);
                }

                // 3. Log activity
                var loginActivity = new ActivityLog
                {
                    UserId = user.Id,
                    Action = ActivityAction.Login,
                    EntityType = "User",
                    EntityId = user.Id,
                    IpAddress = dto.IpAddress,
                    UserAgent = dto.DeviceInfo
                };

                await _activityLogRepository.AddAsync(loginActivity);

                // 4. Generate tokens
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var accessTokenExpiry = DateTime.UtcNow.AddDays(1);
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);

                // 5. Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = refreshTokenExpiry,
                    DeviceInfo = dto.DeviceInfo,
                    IpAddress = dto.IpAddress
                };

                await _refreshTokenRepository.AddAsync(refreshTokenEntity);

                return new AuthResponseDto
                {
                    User = MapToUserDto(user),
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiresAt = accessTokenExpiry,
                    RefreshTokenExpiresAt = refreshTokenExpiry
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi văng ở login rồi bro");
                throw;
            }
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            // Find refresh token
            var spec = new ActiveRefreshTokenSpec(dto.RefreshToken);
            var refreshToken = await _refreshTokenRepository.FirstOrDefaultAsync(spec);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                throw new UnauthorizedException("Refresh token tầm bậy rồi!");
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedException("Người dùng này không tìm thấy hoặc đã bị khóa rồi!");
            }

            // Mark old token as used
            refreshToken.IsUsed = true;
            await _refreshTokenRepository.UpdateAsync(refreshToken);

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var accessTokenExpiry = DateTime.UtcNow.AddMinutes(15);
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);

            // Save new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = refreshTokenExpiry,
                DeviceInfo = refreshToken.DeviceInfo,
                IpAddress = refreshToken.IpAddress
            };

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);

            return new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = accessTokenExpiry,
                RefreshTokenExpiresAt = refreshTokenExpiry
            };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var spec = new RefreshTokenByTokenSpec(refreshToken);
            var token = await _refreshTokenRepository.FirstOrDefaultAsync(spec);

            if (token != null)
            {
                token.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(token);
                return true;
            }

            return false;
        }

        public async Task<bool> RevokeAllUserTokensAsync(Guid userId)
        {
            var spec = new UserActiveRefreshTokensSpec(userId);
            var tokens = await _refreshTokenRepository.ListAsync(spec);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
            }

            if (tokens.Any())
            {
                await _refreshTokenRepository.UpdateRangeAsync(tokens);
            }

            return true;
        }

        public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
        {
            var spec = new UserWithPreferencesSpec(userId);
            var user = await _userRepository.FirstOrDefaultAsync(spec);

            return user == null ? null : MapToUserDto(user);
        }

        public async Task UpdateLastLoginAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}