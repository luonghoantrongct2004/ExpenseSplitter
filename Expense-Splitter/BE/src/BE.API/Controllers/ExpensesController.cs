using BE.Application.DTOs.Expense.Expense;
using BE.Application.Models;
using BE.Common;
using BE.Infrastructure.Interfaces.Expenses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(IExpenseService expenseService, ILogger<ExpensesController> logger)
    {
        _expenseService = expenseService;
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
    /// Tạo chi tiêu mới cho nhóm
    /// </summary>
    [HttpPost("groups/{groupId}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CreateExpense(Guid groupId, [FromBody] CreateExpenseDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _expenseService.CreateExpenseAsync(userId, groupId, dto);

            if (result.Succeeded)
                return StatusCode(201, ApiResponse<ExpenseDto>.Ok(result.Data!, result.Message));

            return BadRequest(ApiResponse<object>.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense for group {GroupId}", groupId);
            return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lấy danh sách chi tiêu của nhóm
    /// </summary>
    [HttpGet("groups/{groupId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedList<ExpenseDto>>), 200)]
    public async Task<IActionResult> GetGroupExpenses(Guid groupId, [FromQuery] ExpenseFilterDto filter)
    {
        try
        {
            var userId = GetUserId();
            var result = await _expenseService.GetGroupExpensesAsync(groupId, userId, filter);

            if (result.Succeeded)
                return Ok(ApiResponse<PagedList<ExpenseDto>>.Ok(result.Data!, result.Message));

            return BadRequest(ApiResponse<object>.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group expenses");
            return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lấy thông tin chi tiết một chi tiêu
    /// </summary>
    [HttpGet("{expenseId}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetExpenseById(Guid expenseId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _expenseService.GetExpenseByIdAsync(expenseId, userId);

            if (result.Succeeded)
                return Ok(ApiResponse<ExpenseDto>.Ok(result.Data!, result.Message));

            return NotFound(ApiResponse<object>.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense {ExpenseId}", expenseId);
            return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Cập nhật chi tiêu
    /// </summary>
    [HttpPut("{expenseId}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> UpdateExpense(Guid expenseId, [FromBody] UpdateExpenseDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _expenseService.UpdateExpenseAsync(userId, expenseId, dto);

            if (result.Succeeded)
                return Ok(ApiResponse<ExpenseDto>.Ok(result.Data!, result.Message));

            if (result.Message.Contains("quyền"))
                return StatusCode(403, ApiResponse<object>.Fail(result.Message));

            return BadRequest(ApiResponse<object>.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense {ExpenseId}", expenseId);
            return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Xóa chi tiêu
    /// </summary>
    [HttpDelete("{expenseId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    public async Task<IActionResult> DeleteExpense(Guid expenseId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _expenseService.DeleteExpenseAsync(userId, expenseId);

            if (result.Succeeded)
                return Ok(ApiResponse<object>.Ok(null, result.Message));

            if (result.Message.Contains("quyền"))
                return StatusCode(403, ApiResponse<object>.Fail(result.Message));

            return BadRequest(ApiResponse<object>.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense {ExpenseId}", expenseId);
            return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lấy số dư của người dùng trong nhóm
    /// </summary>
    [HttpGet("groups/{groupId}/balances/me")]
    [ProducesResponseType(typeof(ApiResponse<UserBalanceDto>), 200)]
    public async Task<IActionResult> GetMyBalance(Guid groupId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _expenseService.GetUserBalanceInGroupAsync(groupId, userId);

            if (result.Succeeded)
                return Ok(ApiResponse<UserBalanceDto>.Ok(result.Data!, result.Message));

            return BadRequest(ApiResponse<object>.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user balance");
            return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lấy số dư của một thành viên trong nhóm
    /// </summary>
    [HttpGet("groups/{groupId}/balances/{memberId}")]
    [ProducesResponseType(typeof(ApiResponse<UserBalanceDto>), 200)]
    public async Task<IActionResult> GetUserBalance(Guid groupId, Guid memberId)
    {
        try
        {
            var userId = GetUserId();
            // Verify requesting user is member of group
            var result = await _expenseService.GetUserBalanceInGroupAsync(groupId, memberId);

            if (result.Succeeded)
                return Ok(ApiResponse<UserBalanceDto>.Ok(result.Data!, result.Message));

            return BadRequest(ApiResponse<object>.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user balance");
            return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    /// <summary>
    /// Lấy bảng cân đối của tất cả thành viên trong nhóm
    /// </summary>
    [HttpGet("groups/{groupId}/balances")]
    [ProducesResponseType(typeof(ApiResponse<List<BalanceSummaryDto>>), 200)]
    public async Task<IActionResult> GetGroupBalances(Guid groupId)
    {
        try
        {
            var userId = GetUserId();
            var result = await _expenseService.GetGroupBalancesAsync(groupId, userId);

            if (result.Succeeded)
                return Ok(ApiResponse<List<BalanceSummaryDto>>.Ok(result.Data!, result.Message));

            return BadRequest(ApiResponse<object>.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group balances");
            return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
        }
    }

    ///// <summary>
    ///// Lấy chi tiêu theo danh mục trong nhóm
    ///// </summary>
    //[HttpGet("groups/{groupId}/by-category")]
    //[ProducesResponseType(typeof(ApiResponse<List<CategoryExpenseSummaryDto>>), 200)]
    //public async Task<IActionResult> GetExpensesByCategory(Guid groupId, [FromQuery] DateRangeDto dateRange)
    //{
    //    try
    //    {
    //        var userId = GetUserId();
    //        var filter = new ExpenseFilterDto
    //        {
    //            StartDate = dateRange.StartDate,
    //            EndDate = dateRange.EndDate,
    //            PageNumber = 1,
    //            PageSize = 1000 // Get all for summary
    //        };

    //        var result = await _expenseService.GetGroupExpensesAsync(groupId, userId, filter);
    //        if (!result.Succeeded)
    //            return BadRequest(ApiResponse<object>.Fail(result.Message));

    //        // Group by category
    //        var categoryGroups = result.Data!.Items
    //            .GroupBy(e => e.Category ?? ExpenseCategory.Other)
    //            .Select(g => new CategoryExpenseSummaryDto
    //            {
    //                Category = g.Key,
    //                CategoryName = g.Key.ToString(),
    //                TotalAmount = g.Sum(e => e.Amount),
    //                Count = g.Count(),
    //                Percentage = 0 // Will calculate after
    //            })
    //            .OrderByDescending(c => c.TotalAmount)
    //            .ToList();

    //        // Calculate percentages
    //        var totalAmount = categoryGroups.Sum(c => c.TotalAmount);
    //        if (totalAmount > 0)
    //        {
    //            foreach (var category in categoryGroups)
    //            {
    //                category.Percentage = Math.Round((category.TotalAmount / totalAmount) * 100, 2);
    //            }
    //        }

    //        return Ok(ApiResponse<List<CategoryExpenseSummaryDto>>.Ok(categoryGroups));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting expenses by category");
    //        return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
    //    }
    //}

    ///// <summary>
    ///// Lấy chi tiêu của người dùng hiện tại
    ///// </summary>
    //[HttpGet("my-expenses")]
    //[ProducesResponseType(typeof(ApiResponse<PagedList<ExpenseDto>>), 200)]
    //public async Task<IActionResult> GetMyExpenses([FromQuery] MyExpenseFilterDto filter)
    //{
    //    try
    //    {
    //        var userId = GetUserId();
    //        var expenseFilter = new ExpenseFilterDto
    //        {
    //            StartDate = filter.StartDate,
    //            EndDate = filter.EndDate,
    //            Category = filter.Category,
    //            PaidById = filter.OnlyPaidByMe ? userId : null,
    //            ParticipantId = userId,
    //            PageNumber = filter.PageNumber,
    //            PageSize = filter.PageSize
    //        };

    //        // Get expenses from all groups
    //        // This would require a different service method to get expenses across all groups
    //        // For now, returning error
    //        return BadRequest(ApiResponse<object>.Fail("Please specify a group to get expenses"));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting my expenses");
    //        return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
    //    }
    //}

    ///// <summary>
    ///// Xuất chi tiêu ra file Excel
    ///// </summary>
    //[HttpGet("groups/{groupId}/export")]
    //[ProducesResponseType(typeof(FileContentResult), 200)]
    //public async Task<IActionResult> ExportGroupExpenses(Guid groupId, [FromQuery] DateRangeDto dateRange)
    //{
    //    try
    //    {
    //        var userId = GetUserId();
    //        var filter = new ExpenseFilterDto
    //        {
    //            StartDate = dateRange.StartDate,
    //            EndDate = dateRange.EndDate,
    //            PageNumber = 1,
    //            PageSize = 10000 // Get all for export
    //        };

    //        var result = await _expenseService.GetGroupExpensesAsync(groupId, userId, filter);
    //        if (!result.Succeeded)
    //            return BadRequest(ApiResponse<object>.Fail(result.Message));

    //        // Here you would generate Excel file
    //        // For now, returning CSV
    //        var csv = "Date,Description,Paid By,Amount,Category,Splits\n";
    //        foreach (var expense in result.Data!.Items)
    //        {
    //            var splits = string.Join("; ", expense.Splits.Select(s => $"{s.UserName}: {s.Amount:N0}"));
    //            csv += $"{expense.ExpenseDate:yyyy-MM-dd},{expense.Description},{expense.PaidByName},{expense.Amount:N0},{expense.Category},{splits}\n";
    //        }

    //        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
    //        return File(bytes, "text/csv", $"expenses_{groupId}_{DateTime.Now:yyyyMMdd}.csv");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error exporting expenses");
    //        return StatusCode(500, ApiResponse<object>.Fail(Messages.Exception));
    //    }
    //}
}