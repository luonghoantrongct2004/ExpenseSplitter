using static BE.Common.CommonData;

namespace BE.Domain.Entities
{
    public class ActivityLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid? GroupId { get; set; }
        public ActivityAction Action { get; set; }
        public string EntityType { get; set; } // "Expense", "Group", "Settlement", etc.
        public Guid EntityId { get; set; }
        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        // Navigation properties
        public virtual User User { get; set; }

        public virtual Group? Group { get; set; }
    }
}