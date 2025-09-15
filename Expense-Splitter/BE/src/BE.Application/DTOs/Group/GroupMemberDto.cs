using System.ComponentModel.DataAnnotations;
using static BE.Common.CommonData;

namespace BE.Application.DTOs.Group;

public class GroupMemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string UserAvatar { get; set; }
    public GroupRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime JoinedAt { get; set; }
    public decimal Balance { get; set; }
}

public class InviteMemberDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public GroupRole Role { get; set; } = GroupRole.Member;
}

public class UpdateMemberRoleDto
{
    [Required]
    public GroupRole Role { get; set; }
}

public class JoinGroupDto
{
    [Required]
    public string InviteCode { get; set; }
}