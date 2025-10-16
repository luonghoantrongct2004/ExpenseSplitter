using BE.Application.Models;
using static BE.Common.CommonData;

namespace BE.Application.DTOs.Expense.Expense;

public class CreateExpenseDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string? Note { get; set; }
    public ExpenseCategory? Category { get; set; }
    public Guid PaidById { get; set; } // Who paid for the expense
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public List<CreateExpenseSplitDto> Splits { get; set; } = new();
}

public class CreateExpenseSplitDto
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
public class ExpenseDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public string? Note { get; set; }
    public ExpenseCategory? Category { get; set; }
    public Guid PaidById { get; set; }
    public string PaidByName { get; set; }
    public string? PaidByAvatar { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; }
    public List<ExpenseSplitDto> Splits { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
public class UpdateExpenseDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public string? Note { get; set; }
    public ExpenseCategory? Category { get; set; }
    public Guid PaidById { get; set; }
    public DateTime ExpenseDate { get; set; }
    public List<CreateExpenseSplitDto> Splits { get; set; } = new();
}

public class ExpenseSplitDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string? Avatar { get; set; }
    public decimal Amount { get; set; }
    public bool IsSettled { get; set; }
    public string? Description { get; set; }
}

public class ExpenseFilterDto: PaginationParams
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ExpenseCategory? Category { get; set; }
    public Guid? PaidById { get; set; }
    public Guid? ParticipantId { get; set; }
    public bool IncludeDeleted { get; set; } = false;
}

public class UserBalanceDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOwed { get; set; }
    public decimal Balance { get; set; }
    public List<UserDebtDto> Debts { get; set; } = new();
}

public class UserDebtDto
{
    public Guid ToUserId { get; set; }
    public string ToUserName { get; set; }
    public decimal Amount { get; set; }
}

public class BalanceSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public decimal Balance { get; set; }
    public List<BalanceDetailDto> Details { get; set; } = new();
}

public class BalanceDetailDto
{
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } // "owes" or "lent"
}
public class SplitMemberDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Percentage { get; set; }
}