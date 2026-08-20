namespace Contracts.Transactions;

public class CreateTransactionItemModifierRequest
{
    public int? ModifierOptionId { get; set; }

    public int ModifierGroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public string OptionName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public decimal PriceAdd { get; set; }

    public bool IsDefault { get; set; }
}