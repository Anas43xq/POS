namespace POS.Contracts.Receipts;

public class PurchaseReceiptSearchRequest
{
    public string? SearchText { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int? SupplierId { get; set; }

    public string? Category { get; set; }

    public byte? ReceiptTypeId { get; set; }
}
