namespace DAL.Entities
{
    public class TransactionItemModifier
    {
        public int TransactionItemModifierId { get; set; }

        public int TransactionItemId { get; set; }

        public int? ModifierOptionId { get; set; }

        public int ModifierGroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public string OptionName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal PriceAdd { get; set; }

        public decimal LineTotal { get; set; }

        public TransactionItem? TransactionItem { get; set; }

        public ModifierOption? ModifierOption { get; set; }

        public ModifierGroup? ModifierGroup { get; set; }
    }
}