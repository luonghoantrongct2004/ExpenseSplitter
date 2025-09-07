using BE.Common;
using static BE.Common.CommonData;

namespace BE.Domain.Entities
{
    public class Expense : BaseEntity
    {
        public Guid GroupId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = CommonData.Currency.VND;
        public string Description { get; set; }
        public string? Note { get; set; }
        public ExpenseCategory? Category { get; set; }
        public Guid PaidById { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
        public Guid CreatedById { get; set; }
        public bool IsDeleted { get; set; } = false;

        public virtual Group Group { get; set; }

        public virtual User PaidBy { get; set; }
        public virtual User CreatedBy { get; set; }
        public virtual ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}