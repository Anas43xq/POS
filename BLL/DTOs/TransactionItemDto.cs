namespace BLL.DTOs;

/// <summary>
/// Projection of a single line item within a transaction.
/// </summary>
public sealed class TransactionItemDto
{
    public int TransactionItemId { get; init; }

    public int TransactionId { get; init; }

    public int VariantId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int Quantity { get; init; }

    public decimal TaxRate { get; init; }

    public decimal LineSubtotal { get; init; }

    public decimal LineTax { get; init; }

    public decimal LineTotal { get; init; }
}