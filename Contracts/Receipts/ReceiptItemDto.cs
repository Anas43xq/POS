
namespace POS.Contracts.Receipts;

public class ReceiptItemDto
{
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    /// <summary>
    /// Non-default modifier selections for this item.
    /// Default options (e.g. "Regular Dough") are excluded.
    /// </summary>
    public List<ReceiptModifierDto> Modifiers { get; set; } = new();
}
