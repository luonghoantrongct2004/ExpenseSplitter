using BE.Domain.Entities;
using BE.Domain.Interfaces;

namespace BE.Infrastructure.Interfaces.Groups
{
    public interface IGroupMemberRepository : IRepository<GroupMember>
    {
        Task<GroupMember?> GetMemberAsync(Guid groupId, Guid userId);

        Task<IEnumerable<GroupMember>> GetGroupMembersAsync(Guid groupId);

        Task<IEnumerable<GroupMember>> GetGroupAdminsAsync(Guid groupId);

        Task<int> GetActiveAdminCountAsync(Guid groupId);
    }
}