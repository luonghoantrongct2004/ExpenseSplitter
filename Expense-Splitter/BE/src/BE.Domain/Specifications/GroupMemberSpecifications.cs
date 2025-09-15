using BE.Common;
using BE.Domain.Entities;

namespace BE.Domain.Specifications;

public class GroupMemberByUserAndGroupSpec : BaseSpecification<GroupMember>
{
    public GroupMemberByUserAndGroupSpec(Guid groupId, Guid userId)
        : base(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive)
    {
        AddInclude(gm => gm.User);
        AddInclude(gm => gm.Group);
    }
}

public class ActiveGroupMembersSpec : BaseSpecification<GroupMember>
{
    public ActiveGroupMembersSpec(Guid groupId)
        : base(gm => gm.GroupId == groupId && gm.IsActive)
    {
        AddInclude(gm => gm.User);
        ApplyOrderBy(gm => gm.JoinedAt);
    }
}

public class GroupAdminsSpec : BaseSpecification<GroupMember>
{
    public GroupAdminsSpec(Guid groupId)
        : base(gm => gm.GroupId == groupId &&
                    gm.Role == CommonData.GroupRole.Admin &&
                    gm.IsActive)
    {
        AddInclude(gm => gm.User);
    }
}

public class InactiveGroupMemberSpec : BaseSpecification<GroupMember>
{
    public InactiveGroupMemberSpec(Guid groupId, Guid userId)
        : base(gm => gm.GroupId == groupId && gm.UserId == userId && !gm.IsActive)
    {
        AddInclude(gm => gm.User);
    }
}