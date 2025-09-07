namespace BE.Domain.Entities
{
    public class ExpenseSplit : BaseEntity
    {
        public Guid ExpenseId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal? Percentage { get; set; }
        public bool IsSettled { get; set; } = false;

        // Navigation properties
        public virtual Expense Expense { get; set; }

        public virtual User User { get; set; }
    }
}