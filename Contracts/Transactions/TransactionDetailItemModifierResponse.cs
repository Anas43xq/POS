namespace Contracts.Transactions
{
    public class TransactionDetailItemModifierResponse
    {
        public string GroupName { get; set; } = string.Empty;
        public string OptionName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAdd { get; set; }
        public decimal LineTotal { get; set; }
    }
}
