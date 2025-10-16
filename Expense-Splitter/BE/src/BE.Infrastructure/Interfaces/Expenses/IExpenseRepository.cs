using BE.Domain.Entities;
using BE.Domain.Interfaces;

namespace BE.Infrastructure.Interfaces.Expenses;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<Expense?> GetExpenseWithDetailsAsync(Guid expenseId);
    Task<IEnumerable<Expense>> GetGroupExpensesAsync(Guid groupId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetUserTotalPaidAsync(Guid groupId, Guid userId);
    Task<decimal> GetUserTotalOwedAsync(Guid groupId, Guid userId);
    Task<bool> HasUserPaidExpensesAsync(Guid groupId, Guid userId);
    Task<bool> IsUserInvolvedInExpenseAsync(Guid expenseId, Guid userId);
}
