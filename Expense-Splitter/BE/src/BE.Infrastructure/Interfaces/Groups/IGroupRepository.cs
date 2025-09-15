using BE.Domain.Entities;
using BE.Domain.Interfaces;

namespace BE.Infrastructure.Interfaces.Groups
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<Group?> GetGroupWithMembersAsync(Guid groupId);

        Task<IEnumerable<Group>> GetUserGroupsAsync(Guid userId, bool includeInactive = false);

        Task<Group?> GetByInviteCodeAsync(string inviteCode);

        Task<bool> IsUserMemberAsync(Guid groupId, Guid userId);

        Task<bool> IsUserAdminAsync(Guid groupId, Guid userId);
    }
}