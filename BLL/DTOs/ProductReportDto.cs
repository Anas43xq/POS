namespace BLL.DTOs;

/// <summary>
/// Row in a product-based sales report.
/// </summary>
public sealed class ProductReportDto
{
    public string ReceiptNumber { get; init; } = string.Empty;

    public DateTime TransactionDate { get; init; }

    public string PaymentMethod { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal LineTotal { get; init; }
}