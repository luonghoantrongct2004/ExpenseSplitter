using BE.Domain.Entities;

namespace BE.Domain.Specifications;

public class ActiveRefreshTokenSpec : BaseSpecification<RefreshToken>
{
    public ActiveRefreshTokenSpec(string token)
        : base(rt => rt.Token == token && !rt.IsUsed && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
    {
        AddInclude(rt => rt.User);
    }
}

public class RefreshTokenByTokenSpec : BaseSpecification<RefreshToken>
{
    public RefreshTokenByTokenSpec(string token)
        : base(rt => rt.Token == token)
    {
    }
}

public class UserActiveRefreshTokensSpec : BaseSpecification<RefreshToken>
{
    public UserActiveRefreshTokensSpec(Guid userId)
        : base(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsUsed)
    {
    }
}