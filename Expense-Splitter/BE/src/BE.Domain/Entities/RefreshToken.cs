namespace BE.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }

    public virtual User User { get; set; } = null!;

    public bool IsActive => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiresAt;
}