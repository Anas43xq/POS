namespace BLL.DTOs;

public sealed class TransactionDetailItemModifierDto
{
    public string GroupName { get; init; } = string.Empty;

    public string OptionName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public decimal PriceAdd { get; init; }

    public decimal LineTotal { get; init; }
}
