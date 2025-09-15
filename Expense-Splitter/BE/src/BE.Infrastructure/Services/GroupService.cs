using AutoMapper;
using BE.Application.DTOs.Group;
using BE.Application.Models;
using BE.Common;
using BE.Domain.Entities;
using BE.Domain.Interfaces;
using BE.Infrastructure.Interfaces.Groups;
using Microsoft.Extensions.Logging;

namespace BE.Infrastructure.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepository;
    private readonly IGroupMemberRepository _groupMemberRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Expense> _expenseRepository;
    private readonly IRepository<Notification> _notificationRepository;
    private readonly IRepository<ActivityLog> _activityLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GroupService> _logger;

    public GroupService(
        IGroupRepository groupRepository,
        IGroupMemberRepository groupMemberRepository,
        IRepository<User> userRepository,
        IRepository<Expense> expenseRepository,
        IRepository<Notification> notificationRepository,
        IRepository<ActivityLog> activityLogRepository,
        IMapper mapper,
        ILogger<GroupService> logger)
    {
        _groupRepository = groupRepository;
        _groupMemberRepository = groupMemberRepository;
        _userRepository = userRepository;
        _expenseRepository = expenseRepository;
        _notificationRepository = notificationRepository;
        _activityLogRepository = activityLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<GroupDetailDto>> CreateGroupAsync(Guid userId, CreateGroupDto dto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<GroupDetailDto>.Failure(string.Format(Messages.NotFound, "Người dùng"));

            var group = _mapper.Map<Group>(dto);
            group.CreatedById = userId;
            group.InviteCode = GenerateInviteCode();

            await _groupRepository.AddAsync(group);

            var member = new GroupMember
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                UserId = userId,
                Role = CommonData.GroupRole.Admin,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _groupMemberRepository.AddAsync(member);
            await LogActivityAsync(userId, group.Id, CommonData.ActivityAction.Create, "Group", group.Id);

            var result = await GetGroupByIdAsync(group.Id, userId);
            if (!result.Succeeded)
                return result;

            return Result<GroupDetailDto>.Success(result.Data!, string.Format(Messages.Created, "nhóm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group");
            return Result<GroupDetailDto>.Failure(Messages.Exception);
        }
    }

    public async Task<Result<GroupDetailDto>> GetGroupByIdAsync(Guid groupId, Guid userId)
    {
        try
        {
            var isMember = await _groupRepository.IsUserMemberAsync(groupId, userId);
            if (!isMember)
                return Result<GroupDetailDto>.Failure(Messages.UserNotInGroup);

            var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
            if (group == null)
                return Result<GroupDetailDto>.Failure(Messages.GroupNotFound);

            var groupDetail = _mapper.Map<GroupDetailDto>(group);

            var currentUserMember = group.Members.FirstOrDefault(m => m.UserId == userId);
            groupDetail.IsAdmin = currentUserMember?.Role == CommonData.GroupRole.Admin;
            groupDetail.UserBalance = await CalculateUserBalanceAsync(group.Id, userId);

            if (!groupDetail.IsAdmin)
                groupDetail.InviteCode = "";

            foreach (var memberDto in groupDetail.Members)
            {
                memberDto.Balance = await CalculateUserBalanceAsync(groupId, memberDto.UserId);
            }

            groupDetail.Statistics = await CalculateGroupStatisticsAsync(group);

            return Result<GroupDetailDto>.Success(groupDetail, string.Format(Messages.Retrieved, "thông tin nhóm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupId}", groupId);
            return Result<GroupDetailDto>.Failure(Messages.Exception);
        }
    }

    public async Task<Result<PagedList<GroupListDto>>> GetUserGroupsAsync(Guid userId, PaginationParams paginationParams)
    {
        try
        {
            var groups = await _groupRepository.GetUserGroupsAsync(userId);
            var groupsList = groups.ToList();

            var totalCount = groupsList.Count;
            var pagedGroups = groupsList
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToList();

            var groupDtos = _mapper.Map<List<GroupListDto>>(pagedGroups);

            foreach (var (dto, group) in groupDtos.Zip(pagedGroups))
            {
                dto.UserBalance = await CalculateUserBalanceAsync(group.Id, userId);
                dto.IsAdmin = group.Members.Any(m => m.UserId == userId && m.Role == CommonData.GroupRole.Admin);
            }

            var pagedList = new PagedList<GroupListDto>(
                groupDtos,
                totalCount,
                paginationParams.PageNumber,
                paginationParams.PageSize);

            return Result<PagedList<GroupListDto>>.Success(pagedList, string.Format(Messages.Retrieved, "danh sách nhóm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user groups");
            return Result<PagedList<GroupListDto>>.Failure(Messages.Exception);
        }
    }

    public async Task<Result<GroupMemberDto>> AddMemberAsync(Guid groupId, Guid userId, InviteMemberDto dto)
    {
        try
        {
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, userId);
            if (!isAdmin)
                return Result<GroupMemberDto>.Failure(string.Format(Messages.NotPermission, "thêm thành viên"));

            var invitedUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (invitedUser == null)
                return Result<GroupMemberDto>.Failure(string.Format(Messages.NotFound, "Email"));

            var existingMember = await _groupMemberRepository.GetMemberAsync(groupId, invitedUser.Id);

            if (existingMember != null)
            {
                if (existingMember.IsActive)
                    return Result<GroupMemberDto>.Failure(Messages.AlreadyInGroup);

                existingMember.IsActive = true;
                existingMember.JoinedAt = DateTime.UtcNow;
                existingMember.Role = dto.Role;
                existingMember.UpdatedAt = DateTime.UtcNow;
                await _groupMemberRepository.UpdateAsync(existingMember);
            }
            else
            {
                existingMember = _mapper.Map<GroupMember>(dto);
                existingMember.GroupId = groupId;
                existingMember.UserId = invitedUser.Id;
                await _groupMemberRepository.AddAsync(existingMember);
            }

            var group = await _groupRepository.GetByIdAsync(groupId);
            await CreateNotificationAsync(
                invitedUser.Id,
                CommonData.NotificationType.GroupInvite,
                "Lời mời tham gia nhóm",
                $"Bạn được mời vào nhóm {group.Name}"
            );

            var memberWithDetails = await _groupMemberRepository.GetMemberAsync(groupId, invitedUser.Id);
            var memberDto = _mapper.Map<GroupMemberDto>(memberWithDetails);
            memberDto.Balance = await CalculateUserBalanceAsync(groupId, invitedUser.Id);

            return Result<GroupMemberDto>.Success(memberDto, string.Format(Messages.Added, "thành viên"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to group {GroupId}", groupId);
            return Result<GroupMemberDto>.Failure(Messages.Exception);
        }
    }

    public async Task<Result<GroupDetailDto>> UpdateGroupAsync(Guid groupId, Guid userId, UpdateGroupDto dto)
    {
        try
        {
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, userId);
            if (!isAdmin)
                return Result<GroupDetailDto>.Failure(string.Format(Messages.NotPermission, "cập nhật nhóm"));

            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                return Result<GroupDetailDto>.Failure(Messages.GroupNotFound);

            if (!string.IsNullOrEmpty(dto.Name))
                group.Name = dto.Name;
            if (dto.Description != null)
                group.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Currency))
                group.Currency = dto.Currency;
            if (dto.IsActive.HasValue)
                group.IsActive = dto.IsActive.Value;

            group.UpdatedAt = DateTime.UtcNow;

            await _groupRepository.UpdateAsync(group);
            await LogActivityAsync(userId, groupId, CommonData.ActivityAction.Update, "Group", groupId);

            var result = await GetGroupByIdAsync(groupId, userId);
            if (!result.Succeeded)
                return result;

            return Result<GroupDetailDto>.Success(result.Data!, string.Format(Messages.Updated, "nhóm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupId}", groupId);
            return Result<GroupDetailDto>.Failure(Messages.Exception);
        }
    }

    public async Task<Result> DeleteGroupAsync(Guid groupId, Guid userId)
    {
        try
        {
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, userId);
            if (!isAdmin)
                return Result.Failure(string.Format(Messages.NotPermission, "xóa nhóm"));

            var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
            if (group == null)
                return Result.Failure(Messages.GroupNotFound);

            var hasUnsettledBalances = await HasUnsettledBalancesAsync(group);
            if (hasUnsettledBalances)
                return Result.Failure(Messages.GroupHasDebt);

            await _groupRepository.DeleteAsync(group);
            await LogActivityAsync(userId, groupId, CommonData.ActivityAction.Delete, "Group", groupId);

            return Result.Success(string.Format(Messages.Deleted, "nhóm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupId}", groupId);
            return Result.Failure(Messages.Exception);
        }
    }

    public async Task<Result> ArchiveGroupAsync(Guid groupId, Guid userId)
    {
        try
        {
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, userId);
            if (!isAdmin)
                return Result.Failure(string.Format(Messages.NotPermission, "lưu trữ nhóm"));

            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                return Result.Failure(Messages.GroupNotFound);

            group.IsActive = false;
            group.UpdatedAt = DateTime.UtcNow;

            await _groupRepository.UpdateAsync(group);
            await LogActivityAsync(userId, groupId, CommonData.ActivityAction.Update, "Group", groupId);

            return Result.Success(string.Format(Messages.Archived, "nhóm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving group {GroupId}", groupId);
            return Result.Failure(Messages.Exception);
        }
    }
    public async Task<Result> UnArchiveGroupAsync(Guid groupId, Guid userId)
    {
        try
        {
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, userId);
            if (!isAdmin)
                return Result.Failure(string.Format(Messages.NotPermission, "bỏ lưu trữ nhóm"));

            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                return Result.Failure(Messages.GroupNotFound);

            group.IsActive = true;
            group.UpdatedAt = DateTime.UtcNow;

            await _groupRepository.UpdateAsync(group);
            await LogActivityAsync(userId, groupId, CommonData.ActivityAction.Update, "Group", groupId);

            return Result.Success(string.Format(Messages.Archived, "nhóm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving group {GroupId}", groupId);
            return Result.Failure(Messages.Exception);
        }
    }

    public async Task<Result> RemoveMemberAsync(Guid groupId, Guid userId, Guid memberId)
    {
        try
        {
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, userId);
            if (!isAdmin)
                return Result.Failure(string.Format(Messages.NotPermission, "xóa thành viên"));

            if (userId == memberId)
                return Result.Failure(Messages.CannotRemoveSelf);

            var memberToRemove = await _groupMemberRepository.GetMemberAsync(groupId, memberId);
            if (memberToRemove == null)
                return Result.Failure(Messages.MemberNotFound);

            memberToRemove.IsActive = false;
            memberToRemove.LeftAt = DateTime.UtcNow;
            memberToRemove.UpdatedAt = DateTime.UtcNow;

            await _groupMemberRepository.UpdateAsync(memberToRemove);

            return Result.Success(string.Format(Messages.Removed, "thành viên"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member {MemberId} from group {GroupId}", memberId, groupId);
            return Result.Failure(Messages.Exception);
        }
    }

    public async Task<Result> UpdateMemberRoleAsync(Guid groupId, Guid userId, Guid memberId, UpdateMemberRoleDto dto)
    {
        try
        {
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, userId);
            if (!isAdmin)
                return Result.Failure(string.Format(Messages.NotPermission, "thay đổi vai trò"));

            var memberToUpdate = await _groupMemberRepository.GetMemberAsync(groupId, memberId);
            if (memberToUpdate == null)
                return Result.Failure(Messages.MemberNotFound);

            memberToUpdate.Role = dto.Role;
            memberToUpdate.UpdatedAt = DateTime.UtcNow;

            await _groupMemberRepository.UpdateAsync(memberToUpdate);

            return Result.Success(string.Format(Messages.Updated, "vai trò"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member role for {MemberId} in group {GroupId}", memberId, groupId);
            return Result.Failure(Messages.Exception);
        }
    }

    public async Task<Result<GroupDetailDto>> JoinGroupAsync(Guid userId, JoinGroupDto dto)
    {
        try
        {
            var group = await _groupRepository.GetByInviteCodeAsync(dto.InviteCode);
            if (group == null)
                return Result<GroupDetailDto>.Failure(Messages.InviteCodeInvalid);

            var existingMember = await _groupMemberRepository.GetMemberAsync(group.Id, userId);
            if (existingMember != null && existingMember.IsActive)
                return Result<GroupDetailDto>.Failure(Messages.AlreadyInGroup);

            if (existingMember != null)
            {
                existingMember.IsActive = true;
                existingMember.JoinedAt = DateTime.UtcNow;
                existingMember.UpdatedAt = DateTime.UtcNow;
                await _groupMemberRepository.UpdateAsync(existingMember);
            }
            else
            {
                var member = new GroupMember
                {
                    Id = Guid.NewGuid(),
                    GroupId = group.Id,
                    UserId = userId,
                    Role = CommonData.GroupRole.Member,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _groupMemberRepository.AddAsync(member);
            }

            var admins = await _groupMemberRepository.GetGroupAdminsAsync(group.Id);
            var user = await _userRepository.GetByIdAsync(userId);

            foreach (var admin in admins)
            {
                await CreateNotificationAsync(
                    admin.UserId,
                    CommonData.NotificationType.MemberJoined,
                    "Thành viên mới",
                    $"{user?.Name} vừa tham gia nhóm"
                );
            }

            await LogActivityAsync(userId, group.Id, CommonData.ActivityAction.Join, "Group", group.Id);

            var result = await GetGroupByIdAsync(group.Id, userId);
            if (!result.Succeeded)
                return result;

            return Result<GroupDetailDto>.Success(result.Data!, string.Format(Messages.Joined, "nhóm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group with code {InviteCode}", dto.InviteCode);
            return Result<GroupDetailDto>.Failure(Messages.Exception);
        }
    }

    public async Task<Result> LeaveGroupAsync(Guid groupId, Guid userId)
    {
        try
        {
            var member = await _groupMemberRepository.GetMemberAsync(groupId, userId);
            if (member == null)
                return Result.Failure(Messages.UserNotInGroup);

            if (member.Role == CommonData.GroupRole.Admin)
            {
                var adminCount = await _groupMemberRepository.GetActiveAdminCountAsync(groupId);
                if (adminCount <= 1)
                    return Result.Failure(Messages.CannotLeaveAsAdmin);
            }

            member.IsActive = false;
            member.LeftAt = DateTime.UtcNow;
            member.UpdatedAt = DateTime.UtcNow;

            await _groupMemberRepository.UpdateAsync(member);
            await LogActivityAsync(userId, groupId, CommonData.ActivityAction.Leave, "Group", groupId);

            return Result.Success(string.Format(Messages.Left, "nhóm"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupId}", groupId);
            return Result.Failure(Messages.Exception);
        }
    }

    public async Task<Result<string>> GenerateInviteCodeAsync(Guid groupId, Guid userId)
    {
        try
        {
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, userId);
            if (!isAdmin)
                return Result<string>.Failure(string.Format(Messages.NotPermission, "tạo mã mời"));

            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                return Result<string>.Failure(Messages.GroupNotFound);

            group.InviteCode = GenerateInviteCode();
            group.UpdatedAt = DateTime.UtcNow;

            await _groupRepository.UpdateAsync(group);

            return Result<string>.Success(group.InviteCode, string.Format(Messages.Generated, "mã mời"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invite code for group {GroupId}", groupId);
            return Result<string>.Failure(Messages.Exception);
        }
    }

    public async Task<Result<GroupStatisticsDto>> GetGroupStatisticsAsync(Guid groupId, Guid userId)
    {
        try
        {
            var isMember = await _groupRepository.IsUserMemberAsync(groupId, userId);
            if (!isMember)
                return Result<GroupStatisticsDto>.Failure(Messages.UserNotInGroup);

            var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
            if (group == null)
                return Result<GroupStatisticsDto>.Failure(Messages.GroupNotFound);

            var statistics = await CalculateGroupStatisticsAsync(group);
            return Result<GroupStatisticsDto>.Success(statistics, string.Format(Messages.Retrieved, "thống kê"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for group {GroupId}", groupId);
            return Result<GroupStatisticsDto>.Failure(Messages.Exception);
        }
    }

    #region Helper

    private async Task<decimal> CalculateUserBalanceAsync(Guid groupId, Guid userId)
    {
        var group = await _groupRepository.GetGroupWithMembersAsync(groupId);
        if (group == null) return 0;

        var totalPaid = group.Expenses
            .Where(e => !e.IsDeleted && e.PaidById == userId)
            .Sum(e => e.Amount);

        var totalOwed = group.Expenses
            .Where(e => !e.IsDeleted)
            .SelectMany(e => e.Splits)
            .Where(s => s.UserId == userId)
            .Sum(s => s.Amount);

        return totalPaid - totalOwed;
    }

    private async Task<GroupStatisticsDto> CalculateGroupStatisticsAsync(Group group)
    {
        var activeMembers = group.Members.Where(m => m.IsActive).ToList();
        var validExpenses = group.Expenses.Where(e => !e.IsDeleted).ToList();

        var statistics = new GroupStatisticsDto
        {
            TotalExpenses = validExpenses.Sum(e => e.Amount),
            TotalTransactions = validExpenses.Count,
            AverageExpensePerMember = activeMembers.Count > 0
                ? validExpenses.Sum(e => e.Amount) / activeMembers.Count
                : 0
        };

        // Expenses by category
        statistics.ExpensesByCategory = validExpenses
            .Where(e => e.Category.HasValue)
            .GroupBy(e => e.Category!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        // Member balances
        foreach (var member in activeMembers)
        {
            var balance = await CalculateUserBalanceAsync(group.Id, member.UserId);
            statistics.MemberBalances[member.UserId] = new UserBalanceDto
            {
                UserId = member.UserId,
                UserName = member.User?.Name ?? "",
                Balance = balance,
                TotalPaid = validExpenses.Where(e => e.PaidById == member.UserId).Sum(e => e.Amount),
                TotalOwed = validExpenses
                    .SelectMany(e => e.Splits)
                    .Where(s => s.UserId == member.UserId)
                    .Sum(s => s.Amount)
            };
        }

        return statistics;
    }

    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private async Task LogActivityAsync(Guid userId, Guid? groupId, CommonData.ActivityAction action, string entityType, Guid entityId)
    {
        var activityLog = new ActivityLog
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

        await _activityLogRepository.AddAsync(activityLog);
    }

    private async Task CreateNotificationAsync(Guid userId, CommonData.NotificationType type, string title, string message)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification);
    }

    private async Task<bool> HasUnsettledBalancesAsync(Group group)
    {
        foreach (var member in group.Members.Where(m => m.IsActive))
        {
            var balance = await CalculateUserBalanceAsync(group.Id, member.UserId);
            if (Math.Abs(balance) > 0.01m)
                return true;
        }
        return false;
    }

    #endregion Helper
}