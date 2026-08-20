namespace BLL.DTOs;

public sealed class TransactionDetailItemDto
{
    public int TransactionItemId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int Quantity { get; init; }

    public decimal LineTotal { get; init; }

    public IReadOnlyList<TransactionDetailItemModifierDto> Modifiers { get; init; }
        = Array.Empty<TransactionDetailItemModifierDto>();
}
