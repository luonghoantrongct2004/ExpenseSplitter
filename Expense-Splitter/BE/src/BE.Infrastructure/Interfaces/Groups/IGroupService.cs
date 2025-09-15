using BE.Application.DTOs.Group;
using BE.Application.Models;
using BE.Common;

namespace BE.Infrastructure.Interfaces.Groups
{
    public interface IGroupService
    {
        // Group CRUD
        Task<Result<GroupDetailDto>> CreateGroupAsync(Guid userId, CreateGroupDto dto);

        Task<Result<GroupDetailDto>> GetGroupByIdAsync(Guid groupId, Guid userId);

        Task<Result<PagedList<GroupListDto>>> GetUserGroupsAsync(Guid userId, PaginationParams paginationParams);

        Task<Result<GroupDetailDto>> UpdateGroupAsync(Guid groupId, Guid userId, UpdateGroupDto dto);

        Task<Result> DeleteGroupAsync(Guid groupId, Guid userId);

        Task<Result> ArchiveGroupAsync(Guid groupId, Guid userId);
        Task<Result> UnArchiveGroupAsync(Guid groupId, Guid userId);

        // Member Management
        Task<Result<GroupMemberDto>> AddMemberAsync(Guid groupId, Guid userId, InviteMemberDto dto);

        Task<Result> RemoveMemberAsync(Guid groupId, Guid userId, Guid memberId);

        Task<Result> UpdateMemberRoleAsync(Guid groupId, Guid userId, Guid memberId, UpdateMemberRoleDto dto);

        Task<Result<GroupDetailDto>> JoinGroupAsync(Guid userId, JoinGroupDto dto);

        Task<Result> LeaveGroupAsync(Guid groupId, Guid userId);

        // Invite Management
        Task<Result<string>> GenerateInviteCodeAsync(Guid groupId, Guid userId);

        Task<Result<GroupStatisticsDto>> GetGroupStatisticsAsync(Guid groupId, Guid userId);
    }
}