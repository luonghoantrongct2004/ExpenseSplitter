namespace BE.Domain.Entities
{
    public class Group : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Currency { get; set; } = Common.CommonData.Currency.VND;
        public string InviteCode { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid CreatedById { get; set; }

        // Navigation properties
        public virtual User CreatedBy { get; set; }

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
    }
}