namespace Contracts.Transactions;

public class CreateTransactionRequest
{
    public int ShiftId { get; set; }
    public int CashierId { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;
    public decimal AmountTendered { get; set; }
    public decimal ChangeGiven { get; set; }
    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public List<CreateTransactionItemRequest> Items { get; set; } = new();
}