using BE.Common;
using BE.Domain;
using BE.Domain.Entities;
using BE.Domain.Specifications;
using BE.Infrastructure.Data.Repositories;
using BE.Infrastructure.Interfaces.Groups;
using Microsoft.EntityFrameworkCore;

namespace BE.Infrastructure.Repositories.Groups
{
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        public GroupRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Group?> GetGroupWithMembersAsync(Guid groupId)
        {
            var spec = new GroupWithDetailsSpec(groupId);
            return await FirstOrDefaultAsync(spec);
        }

        public async Task<IEnumerable<Group>> GetUserGroupsAsync(Guid userId, bool includeInactive = false)
        {
            var spec = new GroupsByUserSpec(userId, includeInactive);
            return await ListAsync(spec);
        }

        public async Task<Group?> GetByInviteCodeAsync(string inviteCode)
        {
            var spec = new GroupByInviteCodeSpec(inviteCode);
            return await FirstOrDefaultAsync(spec);
        }

        public async Task<bool> IsUserMemberAsync(Guid groupId, Guid userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId &&
                               gm.UserId == userId &&
                               gm.IsActive);
        }

        public async Task<bool> IsUserAdminAsync(Guid groupId, Guid userId)
        {
            return await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId &&
                               gm.UserId == userId &&
                               gm.Role == CommonData.GroupRole.Admin &&
                               gm.IsActive);
        }
    }
}