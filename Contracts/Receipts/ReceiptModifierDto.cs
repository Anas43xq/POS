namespace POS.Contracts.Receipts;

/// <summary>
/// A single non-default modifier line for display on a receipt.
/// </summary>
public class ReceiptModifierDto
{
    public string OptionName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public decimal PriceAdd { get; set; }
}