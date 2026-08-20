using BLL.Interfaces;
using Contracts.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private const int LatestTransactionsCount = 5;
    private const int DefaultPageSize = 50;

    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("latest")]
    [Authorize]
    public async Task<IActionResult> GetLatest(CancellationToken ct = default)
    {
        var request = new GetTransactionsListRequest
        {
            PeriodType = "Today",
            PageNumber = 1,
            PageSize = LatestTransactionsCount
        };

        try
        {
            var result = await _transactionService.GetTransactionsListAsync(request, ct);
            return Ok(result.Items);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetList(
        [FromQuery] string periodType = "Today",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        [FromQuery] string? statusFilter = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken ct = default)
    {
        var request = new GetTransactionsListRequest
        {
            PeriodType = periodType,
            PageNumber = pageNumber,
            PageSize = pageSize,
            StatusFilter = statusFilter,
            FromDate = fromDate,
            ToDate = toDate
        };

        try
        {
            var result = await _transactionService.GetTransactionsListAsync(request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("search")]
    [Authorize]
    public async Task<IActionResult> SearchByReceiptNumber(
        [FromQuery] string receiptNumber,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(receiptNumber))
            return Ok(Enumerable.Empty<TransactionListItemDto>());

        var results = await _transactionService.SearchByReceiptNumberAsync(receiptNumber, ct);
        return Ok(results);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var detail = await _transactionService.GetTransactionDetailAsync(id, ct);
        if (detail is null)
            return NotFound();

        return Ok(MapToResponse(detail));
    }

    private static TransactionDetailResponse MapToResponse(BLL.DTOs.TransactionDetailDto dto) => new()
    {
        TransactionId = dto.TransactionId,
        ReceiptNumber = dto.ReceiptNumber,
        TransactionDate = dto.TransactionDate,
        Subtotal = dto.Subtotal,
        TaxTotal = dto.TaxTotal,
        GrandTotal = dto.GrandTotal,
        Status = dto.Status.ToString(),
        Notes = dto.Notes,
        PaymentMethod = dto.PaymentMethod,
        Items = dto.Items.Select(i => new TransactionDetailItemResponse
        {
            TransactionItemId = i.TransactionItemId,
            ProductName = i.ProductName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal,
            Modifiers = i.Modifiers.Select(m => new TransactionDetailItemModifierResponse
            {
                GroupName = m.GroupName,
                OptionName = m.OptionName,
                Quantity = m.Quantity,
                PriceAdd = m.PriceAdd,
                LineTotal = m.LineTotal
            }).ToList()
        }).ToList()
    };
}

