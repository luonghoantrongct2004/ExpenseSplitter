using static BE.Common.CommonData;

namespace BE.Domain.Entities
{
    public class GroupMember : BaseEntity
    {
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public GroupRole Role { get; set; } = GroupRole.Member;
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Group Group { get; set; }

        public virtual User User { get; set; }
    }
}