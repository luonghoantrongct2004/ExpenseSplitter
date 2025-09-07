namespace BE.Domain.Entities
{
    public class User : BaseEntity
    {
        public string GoogleId { get; set; } // Google's unique ID
        public string Email { get; set; }
        public string Name { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }

        public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

        public virtual ICollection<Expense> ExpensesPaid { get; set; } = new List<Expense>();
        public virtual ICollection<ExpenseSplit> ExpenseSplits { get; set; } = new List<ExpenseSplit>();
        public virtual ICollection<Settlement> SettlementsFrom { get; set; } = new List<Settlement>();
        public virtual ICollection<Settlement> SettlementsTo { get; set; } = new List<Settlement>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual UserPreference? UserPreference { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}