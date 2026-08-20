using BLL.Interfaces;
using Contracts.Shifts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftManagementService _shiftManagementService;

    public ShiftsController(IShiftManagementService shiftManagementService)
    {
        _shiftManagementService = shiftManagementService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetList(
        [FromQuery] string periodType = "Today",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? statusFilter = null,
        [FromQuery] int? cashierId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var request = new GetShiftsListRequest
        {
            PeriodType = periodType,
            FromDate = fromDate,
            ToDate = toDate,
            StatusFilter = statusFilter,
            CashierId = cashierId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _shiftManagementService.GetShiftsListAsync(request, ct);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _shiftManagementService.GetShiftDetailAsync(id, ct);
        if (!result.IsSuccess)
        {
            if (IsNotFound(result.Error))
                return NotFound();

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    private static bool IsNotFound(string? error)
    {
        return !string.IsNullOrWhiteSpace(error) &&
               error.Contains("not found", StringComparison.OrdinalIgnoreCase);
    }
}
