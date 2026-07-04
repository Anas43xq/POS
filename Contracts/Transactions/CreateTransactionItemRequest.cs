namespace Contracts.Transactions;

public class CreateTransactionItemRequest
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TaxRate { get; set; }

    public decimal LineSubtotal { get; set; }

    public decimal LineTax { get; set; }

    public decimal LineTotal { get; set; }
}