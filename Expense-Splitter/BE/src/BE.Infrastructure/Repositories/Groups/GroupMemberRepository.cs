using BE.Domain;
using BE.Domain.Entities;
using BE.Domain.Specifications;
using BE.Infrastructure.Data.Repositories;
using BE.Infrastructure.Interfaces.Groups;

namespace BE.Infrastructure.Repositories.Groups;

public class GroupMemberRepository : Repository<GroupMember>, IGroupMemberRepository
{
    public GroupMemberRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<GroupMember?> GetMemberAsync(Guid groupId, Guid userId)
    {
        var spec = new GroupMemberByUserAndGroupSpec(groupId, userId);
        return await FirstOrDefaultAsync(spec);
    }

    public async Task<IEnumerable<GroupMember>> GetGroupMembersAsync(Guid groupId)
    {
        var spec = new ActiveGroupMembersSpec(groupId);
        return await ListAsync(spec);
    }

    public async Task<IEnumerable<GroupMember>> GetGroupAdminsAsync(Guid groupId)
    {
        var spec = new GroupAdminsSpec(groupId);
        return await ListAsync(spec);
    }

    public async Task<int> GetActiveAdminCountAsync(Guid groupId)
    {
        var spec = new GroupAdminsSpec(groupId);
        return await CountAsync(spec);
    }
}