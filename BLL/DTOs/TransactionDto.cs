using Contracts.Enum;

namespace BLL.DTOs;

/// <summary>
/// Projection of a transaction header.
/// </summary>
public sealed class TransactionDto
{
    public int TransactionId { get; init; }

    public string ReceiptNumber { get; init; } = string.Empty;

    public int ShiftId { get; init; }

    public int CashierId { get; init; }

    public DateTime TransactionDate { get; init; }

    public decimal Subtotal { get; init; }

    public decimal TaxTotal { get; init; }

    public decimal GrandTotal { get; init; }

    public TransactionStatus Status { get; init; }

    public string? Notes { get; init; }
}