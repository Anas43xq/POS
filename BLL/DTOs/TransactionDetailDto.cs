using Contracts.Enum;

namespace BLL.DTOs;

/// <summary>
/// Full projection of a transaction for a detail view: header, payment, and line items.
/// </summary>
public sealed class TransactionDetailDto
{
    public int TransactionId { get; init; }

    public string ReceiptNumber { get; init; } = string.Empty;

    public DateTime TransactionDate { get; init; }

    public decimal Subtotal { get; init; }

    public decimal TaxTotal { get; init; }

    public decimal GrandTotal { get; init; }

    public TransactionStatus Status { get; init; }

    public string? Notes { get; init; }

    /// <summary>Empty when the transaction has no recorded payment (e.g. still pending).</summary>
    public string PaymentMethod { get; init; } = string.Empty;

    public IReadOnlyList<TransactionDetailItemDto> Items { get; init; }
        = Array.Empty<TransactionDetailItemDto>();
}
