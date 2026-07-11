
namespace POS.Contracts.Receipts;

public class ReceiptDetailsDto
{
    public int TransactionId { get; set; }

    public string ReceiptNumber { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public string StoreName { get; set; } = "Cafeteria Hawa";

    public string CashierName { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal TaxTotal { get; set; }

    public decimal GrandTotal { get; set; }

    public decimal DiscountTotal { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public decimal AmountTendered { get; set; }

    public decimal ChangeGiven { get; set; }

    public string? ReferenceNumber { get; set; }

    public List<ReceiptItemDto> Items { get; set; } = new();
}
