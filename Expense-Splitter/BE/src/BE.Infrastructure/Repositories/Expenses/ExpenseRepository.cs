using BE.Domain.Entities;
using BE.Infrastructure.Interfaces.Expenses;
using Microsoft.EntityFrameworkCore;

namespace BE.Infrastructure.Repositories.Expenses;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Expense?> GetExpenseWithDetailsAsync(Guid expenseId)
    {
        return await _dbSet
            .Include(e => e.PaidBy)
            .Include(e => e.CreatedBy)
            .Include(e => e.Group)
            .Include(e => e.Splits)
                .ThenInclude(s => s.User)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == expenseId && !e.IsDeleted);
    }

    public async Task<IEnumerable<Expense>> GetGroupExpensesAsync(Guid groupId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _dbSet
            .Include(e => e.PaidBy)
            .Include(e => e.Splits)
                .ThenInclude(s => s.User)
            .Where(e => e.GroupId == groupId && !e.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= endDate.Value);

        return await query.OrderByDescending(e => e.ExpenseDate).ToListAsync();
    }

    public async Task<decimal> GetUserTotalPaidAsync(Guid groupId, Guid userId)
    {
        return await _dbSet
            .Where(e => e.GroupId == groupId && e.PaidById == userId && !e.IsDeleted)
            .SumAsync(e => e.Amount);
    }

    public async Task<decimal> GetUserTotalOwedAsync(Guid groupId, Guid userId)
    {
        return await _context.ExpenseSplits
            .Include(s => s.Expense)
            .Where(s => s.UserId == userId &&
                       s.Expense.GroupId == groupId &&
                       !s.Expense.IsDeleted)
            .SumAsync(s => s.Amount);
    }

    public async Task<bool> HasUserPaidExpensesAsync(Guid groupId, Guid userId)
    {
        return await _dbSet
            .AnyAsync(e => e.GroupId == groupId && e.PaidById == userId && !e.IsDeleted);
    }

    public async Task<bool> IsUserInvolvedInExpenseAsync(Guid expenseId, Guid userId)
    {
        return await _dbSet
            .AnyAsync(e => e.Id == expenseId &&
                          (e.PaidById == userId || e.Splits.Any(s => s.UserId == userId)));
    }
}
