using BLL.Interfaces;
using Contracts.Sales;
using Contracts.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IKpiService _kpiService;

    public DashboardController(IKpiService kpiService)
    {
        _kpiService = kpiService;
    }

    [HttpGet("kpis")]
    [Authorize]
    public async Task<IActionResult> GetKpis(
        [FromQuery] string periodType = "Today",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        try
        {
            var kpis = await LoadKpisAsync(periodType, fromDate, toDate, ct);
            return Ok(kpis);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<KpiDto> LoadKpisAsync(
        string periodType,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken ct)
    {
        var request = new GetTransactionKpisRequest
        {
            PeriodType = periodType,
            FromDate = fromDate,
            ToDate = toDate
        };

        return await _kpiService.GetKpisAsync(request, ct);
    }
}