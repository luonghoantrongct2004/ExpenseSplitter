using static BE.Common.CommonData;

namespace BE.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? Data { get; set; } // JSON data for additional info
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
    }
}