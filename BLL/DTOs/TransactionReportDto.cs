namespace BLL.DTOs;

/// <summary>
/// Row in a transaction-based sales report.
/// </summary>
public sealed class TransactionReportDto
{
    public int TransactionId { get; init; }
    public string ReceiptNumber { get; init; } = string.Empty;

    public DateTime TransactionDate { get; init; }

    public string PaymentMethod { get; init; } = string.Empty;

    public decimal GrandTotal { get; init; }

    public string? Status { get; init; }

    public string Note { get; init; } = string.Empty;
}