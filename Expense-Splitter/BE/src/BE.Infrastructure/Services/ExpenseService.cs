using AutoMapper;
using BE.Application.DTOs.Expense.Expense;
using BE.Application.Models;
using BE.Common;
using BE.Domain.Entities;
using BE.Domain.Interfaces;
using BE.Infrastructure.Interfaces.Expenses;
using BE.Infrastructure.Interfaces.Groups;
using Microsoft.Extensions.Logging;
using static BE.Common.CommonData;

namespace BE.Infrastructure.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IRepository<ExpenseSplit> _splitRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<ActivityLog> _activityLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        IGroupRepository groupRepository,
        IRepository<ExpenseSplit> splitRepository,
        IRepository<User> userRepository,
        IRepository<ActivityLog> activityLogRepository,
        IMapper mapper,
        ILogger<ExpenseService> logger)
    {
        _expenseRepository = expenseRepository;
        _groupRepository = groupRepository;
        _splitRepository = splitRepository;
        _userRepository = userRepository;
        _activityLogRepository = activityLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ExpenseDto>> CreateExpenseAsync(Guid userId, Guid groupId, CreateExpenseDto dto)
    {
        try
        {
            // Validate user is member of group
            var isMember = await _groupRepository.IsUserMemberAsync(groupId, userId);
            if (!isMember)
                return Result<ExpenseDto>.Failure(Messages.UserNotInGroup);

            // Validate payer is member of group
            if (dto.PaidById != userId)
            {
                var isPayerMember = await _groupRepository.IsUserMemberAsync(groupId, dto.PaidById);
                if (!isPayerMember)
                    return Result<ExpenseDto>.Failure("Người chi trả không phải thành viên nhóm");
            }

            // Validate at least one split
            if (dto.Splits == null || !dto.Splits.Any())
                return Result<ExpenseDto>.Failure("Phải có ít nhất một người tham gia chia tiền");

            // Validate splits total
            var splitTotal = dto.Splits.Sum(s => s.Amount);
            if (Math.Abs(splitTotal - dto.Amount) > 0.01m)
            {
                return Result<ExpenseDto>.Failure($"Tổng tiền chia ({splitTotal:N0}) không khớp với số tiền chi ({dto.Amount:N0})");
            }

            // Validate all split users are group members
            foreach (var split in dto.Splits)
            {
                if (split.Amount <= 0)
                    return Result<ExpenseDto>.Failure("Số tiền chia phải lớn hơn 0");

                var isSplitUserMember = await _groupRepository.IsUserMemberAsync(groupId, split.UserId);
                if (!isSplitUserMember)
                    return Result<ExpenseDto>.Failure("Một hoặc nhiều người trong danh sách chia không phải thành viên nhóm");
            }

            // Create expense
            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                Amount = dto.Amount,
                Description = dto.Description,
                Note = dto.Note,
                Category = dto.Category,
                PaidById = dto.PaidById,
                ExpenseDate = dto.ExpenseDate,
                CreatedById = userId,
                Currency = Currency.VND,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _expenseRepository.AddAsync(expense);

            // Create splits
            foreach (var splitDto in dto.Splits)
            {
                var split = new ExpenseSplit
                {
                    Id = Guid.NewGuid(),
                    ExpenseId = expense.Id,
                    UserId = splitDto.UserId,
                    Amount = splitDto.Amount,
                    Description = splitDto.Description,
                    Percentage = splitDto.Amount / dto.Amount * 100,
                    IsSettled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _splitRepository.AddAsync(split);
            }

            // Log activity
            await LogActivityAsync(userId, groupId, ActivityAction.Create, "Expense", expense.Id);

            // Get created expense with details
            var createdExpense = await _expenseRepository.GetExpenseWithDetailsAsync(expense.Id);
            var result = _mapper.Map<ExpenseDto>(createdExpense);
            result.CanEdit = userId == createdExpense.CreatedById || await _groupRepository.IsUserAdminAsync(groupId, userId);
            result.CanDelete = result.CanEdit;

            return Result<ExpenseDto>.Success(result, "Tạo chi tiêu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return Result<ExpenseDto>.Failure(Messages.Exception);
        }
    }

    public async Task<Result<ExpenseDto>> UpdateExpenseAsync(Guid userId, Guid expenseId, UpdateExpenseDto dto)
    {
        try
        {
            var expense = await _expenseRepository.GetExpenseWithDetailsAsync(expenseId);
            if (expense == null)
                return Result<ExpenseDto>.Failure("Không tìm thấy chi tiêu");

            // Check permissions
            var isAdmin = await _groupRepository.IsUserAdminAsync(expense.GroupId, userId);
            if (expense.CreatedById != userId && !isAdmin)
                return Result<ExpenseDto>.Failure("Bạn không có quyền chỉnh sửa chi tiêu này");

            // Validate payer
            if (dto.PaidById != expense.PaidById)
            {
                var isPayerMember = await _groupRepository.IsUserMemberAsync(expense.GroupId, dto.PaidById);
                if (!isPayerMember)
                    return Result<ExpenseDto>.Failure("Người chi trả không phải thành viên nhóm");
            }

            // Validate splits
            if (dto.Splits == null || !dto.Splits.Any())
                return Result<ExpenseDto>.Failure("Phải có ít nhất một người tham gia chia tiền");

            var splitTotal = dto.Splits.Sum(s => s.Amount);
            if (Math.Abs(splitTotal - dto.Amount) > 0.01m)
            {
                return Result<ExpenseDto>.Failure($"Tổng tiền chia ({splitTotal:N0}) không khớp với số tiền chi ({dto.Amount:N0})");
            }

            // Update expense properties
            expense.Amount = dto.Amount;
            expense.Description = dto.Description;
            expense.Note = dto.Note;
            expense.Category = dto.Category;
            expense.PaidById = dto.PaidById;
            expense.ExpenseDate = dto.ExpenseDate;
            expense.UpdatedAt = DateTime.UtcNow;

            await _expenseRepository.UpdateAsync(expense);

            // Delete existing splits
            var existingSplits = expense.Splits.ToList();
            await _splitRepository.DeleteRangeAsync(existingSplits);

            // Create new splits
            foreach (var splitDto in dto.Splits)
            {
                if (splitDto.Amount <= 0)
                    return Result<ExpenseDto>.Failure("Số tiền chia phải lớn hơn 0");

                var split = new ExpenseSplit
                {
                    Id = Guid.NewGuid(),
                    ExpenseId = expense.Id,
                    UserId = splitDto.UserId,
                    Amount = splitDto.Amount,
                    Description = splitDto.Description,
                    Percentage = splitDto.Amount / dto.Amount * 100,
                    IsSettled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _splitRepository.AddAsync(split);
            }

            // Log activity
            await LogActivityAsync(userId, expense.GroupId, ActivityAction.Update, "Expense", expense.Id);

            // Get updated expense
            var updatedExpense = await _expenseRepository.GetExpenseWithDetailsAsync(expense.Id);
            var result = _mapper.Map<ExpenseDto>(updatedExpense);
            result.CanEdit = true;
            result.CanDelete = true;

            return Result<ExpenseDto>.Success(result, "Cập nhật chi tiêu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense {ExpenseId}", expenseId);
            return Result<ExpenseDto>.Failure(Messages.Exception);
        }
    }
    public async Task<Result<ExpenseDto>> GetExpenseByIdAsync(Guid expenseId, Guid userId)
    {
        try
        {
            var expense = await _expenseRepository.GetExpenseWithDetailsAsync(expenseId);
            if (expense == null)
                return Result<ExpenseDto>.Failure("Không tìm thấy chi tiêu");

            // Check if user is member of the group
            var isMember = await _groupRepository.IsUserMemberAsync(expense.GroupId, userId);
            if (!isMember)
                return Result<ExpenseDto>.Failure(Messages.UserNotInGroup);

            var dto = _mapper.Map<ExpenseDto>(expense);
            var isAdmin = await _groupRepository.IsUserAdminAsync(expense.GroupId, userId);
            dto.CanEdit = expense.CreatedById == userId || isAdmin;
            dto.CanDelete = dto.CanEdit;

            return Result<ExpenseDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense {ExpenseId}", expenseId);
            return Result<ExpenseDto>.Failure(Messages.Exception);
        }
    }

    public async Task<Result<PagedList<ExpenseDto>>> GetGroupExpensesAsync(Guid groupId, Guid userId, ExpenseFilterDto filter)
    {
        try
        {
            var isMember = await _groupRepository.IsUserMemberAsync(groupId, userId);
            if (!isMember)
                return Result<PagedList<ExpenseDto>>.Failure(Messages.UserNotInGroup);

            var expenses = await _expenseRepository.GetGroupExpensesAsync(groupId, filter.StartDate, filter.EndDate);

            // Apply additional filters
            if (filter.Category.HasValue)
                expenses = expenses.Where(e => e.Category == filter.Category);

            if (filter.PaidById.HasValue)
                expenses = expenses.Where(e => e.PaidById == filter.PaidById);

            if (filter.ParticipantId.HasValue)
                expenses = expenses.Where(e => e.Splits.Any(s => s.UserId == filter.ParticipantId));

            var totalCount = expenses.Count();
            var items = expenses
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var expenseDtos = _mapper.Map<List<ExpenseDto>>(items);

            // Set permissions
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, userId);
            foreach (var dto in expenseDtos)
            {
                dto.CanEdit = dto.CreatedById == userId || isAdmin;
                dto.CanDelete = dto.CanEdit;
            }

            var pagedList = new PagedList<ExpenseDto>(expenseDtos, totalCount, filter.PageNumber, filter.PageSize);
            return Result<PagedList<ExpenseDto>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group expenses");
            return Result<PagedList<ExpenseDto>>.Failure(Messages.Exception);
        }
    }

    public async Task<Result> DeleteExpenseAsync(Guid userId, Guid expenseId)
    {
        try
        {
            var expense = await _expenseRepository.GetExpenseWithDetailsAsync(expenseId);
            if (expense == null)
                return Result.Failure("Không tìm thấy chi tiêu");

            // Check permissions
            var isAdmin = await _groupRepository.IsUserAdminAsync(expense.GroupId, userId);
            if (expense.CreatedById != userId && !isAdmin)
                return Result.Failure("Bạn không có quyền xóa chi tiêu này");

            // Soft delete
            expense.IsDeleted = true;
            expense.UpdatedAt = DateTime.UtcNow;
            await _expenseRepository.UpdateAsync(expense);

            // Log activity
            await LogActivityAsync(userId, expense.GroupId, ActivityAction.Delete, "Expense", expense.Id);

            return Result.Success("Xóa chi tiêu thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense {ExpenseId}", expenseId);
            return Result.Failure(Messages.Exception);
        }
    }

    public async Task<Result<UserBalanceDto>> GetUserBalanceInGroupAsync(Guid groupId, Guid userId)
    {
        try
        {
            var isMember = await _groupRepository.IsUserMemberAsync(groupId, userId);
            if (!isMember)
                return Result<UserBalanceDto>.Failure(Messages.UserNotInGroup);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<UserBalanceDto>.Failure("Không tìm thấy người dùng");

            var expenses = await _expenseRepository.GetGroupExpensesAsync(groupId);

            var totalPaid = expenses.Where(e => e.PaidById == userId).Sum(e => e.Amount);
            var totalOwed = expenses.SelectMany(e => e.Splits).Where(s => s.UserId == userId).Sum(s => s.Amount);
            var balance = totalPaid - totalOwed;

            var result = new UserBalanceDto
            {
                UserId = userId,
                UserName = user.Name,
                TotalPaid = totalPaid,
                TotalOwed = totalOwed,
                Balance = balance,
                Debts = new List<UserDebtDto>()
            };

            // Calculate individual debts
            var userDebts = new Dictionary<Guid, decimal>();

            foreach (var expense in expenses)
            {
                if (expense.PaidById == userId)
                {
                    // User paid, others owe them
                    foreach (var split in expense.Splits.Where(s => s.UserId != userId))
                    {
                        if (!userDebts.ContainsKey(split.UserId))
                            userDebts[split.UserId] = 0;
                        userDebts[split.UserId] += split.Amount;
                    }
                }
                else
                {
                    // Others paid, user owes them
                    var userSplit = expense.Splits.FirstOrDefault(s => s.UserId == userId);
                    if (userSplit != null)
                    {
                        if (!userDebts.ContainsKey(expense.PaidById))
                            userDebts[expense.PaidById] = 0;
                        userDebts[expense.PaidById] -= userSplit.Amount;
                    }
                }
            }

            // Build debt list
            foreach (var (otherUserId, amount) in userDebts.Where(d => Math.Abs(d.Value) > 0.01m))
            {
                var otherUser = await _userRepository.GetByIdAsync(otherUserId);
                if (otherUser != null && amount < 0) // User owes this person
                {
                    result.Debts.Add(new UserDebtDto
                    {
                        ToUserId = otherUserId,
                        ToUserName = otherUser.Name,
                        Amount = Math.Abs(amount)
                    });
                }
            }

            return Result<UserBalanceDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user balance");
            return Result<UserBalanceDto>.Failure(Messages.Exception);
        }
    }

    public async Task<Result<List<BalanceSummaryDto>>> GetGroupBalancesAsync(Guid groupId, Guid userId)
    {
        try
        {
            var isMember = await _groupRepository.IsUserMemberAsync(groupId, userId);
            if (!isMember)
                return Result<List<BalanceSummaryDto>>.Failure(Messages.UserNotInGroup);

            var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
            if (group == null)
                return Result<List<BalanceSummaryDto>>.Failure(Messages.GroupNotFound);

            var expenses = await _expenseRepository.GetGroupExpensesAsync(groupId);
            var balances = new Dictionary<Guid, Dictionary<Guid, decimal>>();

            // Initialize balances
            foreach (var member in group.Members.Where(m => m.IsActive))
            {
                balances[member.UserId] = new Dictionary<Guid, decimal>();
            }

            // Calculate balances
            foreach (var expense in expenses)
            {
                var paidBy = expense.PaidById;

                foreach (var split in expense.Splits)
                {
                    if (split.UserId != paidBy)
                    {
                        if (!balances.ContainsKey(paidBy))
                            balances[paidBy] = new Dictionary<Guid, decimal>();

                        if (!balances.ContainsKey(split.UserId))
                            balances[split.UserId] = new Dictionary<Guid, decimal>();

                        // paidBy lent money to split.UserId
                        if (!balances[paidBy].ContainsKey(split.UserId))
                            balances[paidBy][split.UserId] = 0;

                        if (!balances[split.UserId].ContainsKey(paidBy))
                            balances[split.UserId][paidBy] = 0;

                        balances[paidBy][split.UserId] += split.Amount;
                        balances[split.UserId][paidBy] -= split.Amount;
                    }
                }
            }

            // Build result
            var result = new List<BalanceSummaryDto>();
            foreach (var member in group.Members.Where(m => m.IsActive))
            {
                var summary = new BalanceSummaryDto
                {
                    UserId = member.UserId,
                    UserName = member.User.Name,
                    Balance = 0,
                    Details = new List<BalanceDetailDto>()
                };

                if (balances.ContainsKey(member.UserId))
                {
                    foreach (var (otherUserId, amount) in balances[member.UserId])
                    {
                        if (Math.Abs(amount) > 0.01m) // Only show non-zero balances
                        {
                            var otherMember = group.Members.FirstOrDefault(m => m.UserId == otherUserId);
                            if (otherMember != null)
                            {
                                summary.Details.Add(new BalanceDetailDto
                                {
                                    OtherUserId = otherUserId,
                                    OtherUserName = otherMember.User.Name,
                                    Amount = Math.Abs(amount),
                                    Type = amount > 0 ? "lent" : "owes"
                                });
                                summary.Balance += amount;
                            }
                        }
                    }
                }

                result.Add(summary);
            }

            return Result<List<BalanceSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating group balances");
            return Result<List<BalanceSummaryDto>>.Failure(Messages.Exception);
        }
    }

    private async Task LogActivityAsync(Guid userId, Guid groupId, ActivityAction action, string entityType, Guid entityId)
    {
        var activity = new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = groupId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _activityLogRepository.AddAsync(activity);
    }
}