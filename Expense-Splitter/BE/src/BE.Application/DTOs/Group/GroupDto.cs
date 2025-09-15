namespace BE.Application.DTOs.Group
{
    public class GroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Currency { get; set; }
        public bool IsActive { get; set; }
        public string InviteCode { get; set; }
        public int MemberCount { get; set; }
        public decimal TotalExpenses { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsAdmin { get; set; }
        public decimal UserBalance { get; set; }
    }

    public class GroupDetailDto : GroupDto
    {
        public List<GroupMemberDto> Members { get; set; } = new();
        public GroupStatisticsDto Statistics { get; set; }
    }

    public class GroupListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
        public decimal UserBalance { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class GroupStatisticsDto
    {
        public decimal TotalExpenses { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageExpensePerMember { get; set; }
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
        public Dictionary<Guid, UserBalanceDto> MemberBalances { get; set; } = new();
    }

    public class UserBalanceDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOwed { get; set; }
    }
}