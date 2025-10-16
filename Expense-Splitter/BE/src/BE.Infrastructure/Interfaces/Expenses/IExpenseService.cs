using BE.Application.DTOs.Expense.Expense;
using BE.Application.Models;
using BE.Common;
using static BE.Common.CommonData;

namespace BE.Infrastructure.Interfaces.Expenses;

public interface IExpenseService
{
    Task<Result<ExpenseDto>> CreateExpenseAsync(Guid userId, Guid groupId, CreateExpenseDto dto);
    Task<Result<ExpenseDto>> GetExpenseByIdAsync(Guid expenseId, Guid userId);
    Task<Result<PagedList<ExpenseDto>>> GetGroupExpensesAsync(Guid groupId, Guid userId, ExpenseFilterDto filter);
    Task<Result<ExpenseDto>> UpdateExpenseAsync(Guid userId, Guid expenseId, UpdateExpenseDto dto);
    Task<Result> DeleteExpenseAsync(Guid userId, Guid expenseId);
    Task<Result<UserBalanceDto>> GetUserBalanceInGroupAsync(Guid groupId, Guid userId);
    Task<Result<List<BalanceSummaryDto>>> GetGroupBalancesAsync(Guid groupId, Guid userId);
}
