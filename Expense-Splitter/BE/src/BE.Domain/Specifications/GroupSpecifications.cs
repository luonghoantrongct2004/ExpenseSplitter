using BE.Domain.Entities;

namespace BE.Domain.Specifications;

public class GroupByIdSpec : BaseSpecification<Group>
{
    public GroupByIdSpec(Guid groupId)
        : base(g => g.Id == groupId)
    {
        AddInclude(g => g.Members);
        AddInclude("Members.User");
        AddInclude(g => g.Expenses);
        AddInclude("Expenses.Splits");
    }
}

public class GroupsByUserSpec : BaseSpecification<Group>
{
    public GroupsByUserSpec(Guid userId, bool includeInactive = false)
        : base(g => g.Members.Any(m => m.UserId == userId && m.IsActive))
    {
        if (includeInactive)
        {
            Criteria = g => g.Members.Any(m => m.UserId == userId);
        }

        AddInclude(g => g.Members);
        AddInclude(g => g.Expenses);
        ApplyOrderByDescending(g => g.UpdatedAt);
    }
}

public class GroupByInviteCodeSpec : BaseSpecification<Group>
{
    public GroupByInviteCodeSpec(string inviteCode)
        : base(g => g.InviteCode == inviteCode && g.IsActive)
    {
        AddInclude(g => g.Members);
    }
}

public class ActiveGroupsSpec : BaseSpecification<Group>
{
    public ActiveGroupsSpec()
        : base(g => g.IsActive)
    {
        ApplyOrderByDescending(g => g.CreatedAt);
    }
}

public class GroupWithDetailsSpec : BaseSpecification<Group>
{
    public GroupWithDetailsSpec(Guid groupId)
        : base(g => g.Id == groupId)
    {
        AddInclude(g => g.Members);
        AddInclude("Members.User");
        AddInclude(g => g.Expenses);
        AddInclude("Expenses.Splits");
        AddInclude("Expenses.PaidBy");
        AddInclude(g => g.Settlements);
    }
}