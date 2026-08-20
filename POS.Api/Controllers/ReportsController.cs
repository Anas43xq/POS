using BLL.DTOs;
using BLL.Interfaces;
using Contracts.Sales;
using Contracts.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IKpiService _kpiService;
    private readonly IReportService _reportService;

    public ReportsController(IKpiService kpiService, IReportService reportService)
    {
        _kpiService = kpiService;
        _reportService = reportService;
    }

    [HttpGet("sales-summary")]
    [Authorize]
    public async Task<IActionResult> GetSalesSummary(
        [FromQuery] string periodType = "Today",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        try
        {
            var kpiRequest = new GetTransactionKpisRequest
            {
                PeriodType = periodType,
                FromDate = fromDate,
                ToDate = toDate
            };

            var kpis = await _kpiService.GetKpisAsync(kpiRequest, ct);
            var chart = await _reportService.GetSalesChartAsync(periodType, fromDate, toDate);

            return Ok(MapToSalesSummaryResponse(kpis, chart));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("top-categories")]
    [Authorize]
    public async Task<IActionResult> GetTopCategories(
        [FromQuery] string periodType = "Today",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        try
        {
            var categories = await _reportService.GetTopCategoriesAsync(periodType, fromDate, toDate);
            return Ok(categories.Select(MapToTopCategoryResponse).ToList());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("categories/{categoryId:int}/top-products")]
    [Authorize]
    public async Task<IActionResult> GetCategoryTopProducts(
        int categoryId,
        [FromQuery] string periodType = "Today",
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        try
        {
            var products = await _reportService.GetCategoryTopProductsAsync(categoryId, periodType, fromDate, toDate);
            return Ok(products.Select(MapToCategoryTopProductResponse).ToList());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private static SalesSummaryResponse MapToSalesSummaryResponse(KpiDto kpis, List<SalesChartBucketDto> chart) => new()
    {
        TotalSales = kpis.TotalSales,
        TotalOrders = kpis.TotalOrders,
        Chart = chart.Select(b => new SalesSummaryBucketResponse
        {
            Label = b.Label,
            TotalSales = b.TotalSales
        }).ToList()
    };

    private static TopCategoryResponse MapToTopCategoryResponse(TopCategoryAggregateDto dto) => new()
    {
        CategoryId = dto.CategoryId,
        CategoryName = dto.CategoryName,
        TotalSales = dto.TotalSales,
        Quantity = dto.Quantity
    };

    private static CategoryTopProductResponse MapToCategoryTopProductResponse(TopProductAggregateDto dto) => new()
    {
        ProductId = dto.ProductId,
        ProductName = dto.ProductName,
        TotalSales = dto.TotalSales,
        Quantity = dto.Quantity
    };
}
