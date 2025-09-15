using BE.Application.DTOs.Group;
using BE.Application.Models;
using BE.Common;
using BE.Infrastructure.Interfaces.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IGroupService groupService, ILogger<GroupsController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException(Messages.Unauthorized);

        return Guid.Parse(userIdClaim);
    }

    /// <summary>
    /// Tạo nhóm mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GroupDetailDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.CreateGroupAsync(userId, dto);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return CreatedAtAction(
                nameof(GetGroup),
                new { id = result.Data!.Id },
                ApiResponse<GroupDetailDto>.Ok(result.Data, result.Message)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateGroup");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lấy danh sách nhóm của user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedList<GroupListDto>>), 200)]
    public async Task<IActionResult> GetUserGroups([FromQuery] PaginationParams paginationParams)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.GetUserGroupsAsync(userId, paginationParams);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            Response.Headers.Add("X-Pagination", result.Data!.GetPaginationHeader());
            return Ok(ApiResponse<PagedList<GroupListDto>>.Ok(result.Data, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserGroups");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết nhóm
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GroupDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.GetGroupByIdAsync(id, userId);

            if (!result.Succeeded)
            {
                if (result.Error == Messages.GroupNotFound)
                    return NotFound(ApiResponse<object>.Fail(result.Error));

                return BadRequest(ApiResponse<object>.Fail(result.Error));
            }

            return Ok(ApiResponse<GroupDetailDto>.Ok(result.Data!, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetGroup");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Cập nhật thông tin nhóm
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GroupDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateGroupDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.UpdateGroupAsync(id, userId, dto);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<GroupDetailDto>.Ok(result.Data!, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateGroup");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Xóa nhóm
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.DeleteGroupAsync(id, userId);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteGroup");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lưu trữ nhóm
    /// </summary>
    [HttpPost("{id}/archive")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> ArchiveGroup(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.ArchiveGroupAsync(id, userId);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ArchiveGroup");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }
    /// <summary>
    /// Lưu trữ nhóm
    /// </summary>
    [HttpPost("{id}/unarchive")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> UnArchiveGroup(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.UnArchiveGroupAsync(id, userId);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ArchiveGroup");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Thêm thành viên vào nhóm
    /// </summary>
    [HttpPost("{id}/members")]
    [ProducesResponseType(typeof(ApiResponse<GroupMemberDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] InviteMemberDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.AddMemberAsync(id, userId, dto);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return CreatedAtAction(
                nameof(GetGroup),
                new { id },
                ApiResponse<GroupMemberDto>.Ok(result.Data!, result.Message)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddMember");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Xóa thành viên khỏi nhóm
    /// </summary>
    [HttpDelete("{id}/members/{memberId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> RemoveMember(Guid id, Guid memberId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.RemoveMemberAsync(id, userId, memberId);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RemoveMember");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Cập nhật vai trò thành viên
    /// </summary>
    [HttpPut("{id}/members/{memberId}/role")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid memberId, [FromBody] UpdateMemberRoleDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.UpdateMemberRoleAsync(id, userId, memberId, dto);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateMemberRole");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Tham gia nhóm bằng mã mời
    /// </summary>
    [HttpPost("join")]
    [ProducesResponseType(typeof(ApiResponse<GroupDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> JoinGroup([FromBody] JoinGroupDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.JoinGroupAsync(userId, dto);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<GroupDetailDto>.Ok(result.Data!, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JoinGroup");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Rời khỏi nhóm
    /// </summary>
    [HttpPost("{id}/leave")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> LeaveGroup(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.LeaveGroupAsync(id, userId);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Ok(null, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LeaveGroup");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Tạo mã mời mới
    /// </summary>
    [HttpPost("{id}/invite-code")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> GenerateInviteCode(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.GenerateInviteCodeAsync(id, userId);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Ok(new { inviteCode = result.Data }, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GenerateInviteCode");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lấy thống kê nhóm
    /// </summary>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(ApiResponse<GroupStatisticsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> GetGroupStatistics(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var result = await _groupService.GetGroupStatisticsAsync(id, userId);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<GroupStatisticsDto>.Ok(result.Data!, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetGroupStatistics");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lấy QR code mời vào nhóm
    /// </summary>
    [HttpGet("{id}/invite-qr")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> GetInviteQRCode(Guid id)
    {
        try
        {
            var userId = GetUserId();

            var result = await _groupService.GenerateInviteCodeAsync(id, userId);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            var inviteUrl = $"{Request.Scheme}://{Request.Host}/join?code={result.Data}";

            return Ok(ApiResponse<object>.Ok(new
            {
                inviteCode = result.Data,
                inviteUrl = inviteUrl,
                qrData = inviteUrl
            }, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetInviteQRCode");
            return BadRequest(ApiResponse<object>.Fail(Messages.Exception));
        }
    }
}
