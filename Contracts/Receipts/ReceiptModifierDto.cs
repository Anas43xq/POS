namespace POS.Contracts.Receipts;

public class ReceiptModifierDto
{
    public string OptionName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public decimal PriceAdd { get; set; }
}