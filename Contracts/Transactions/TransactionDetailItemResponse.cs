namespace Contracts.Transactions
{
    public class TransactionDetailItemResponse
    {
        public int TransactionItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public List<TransactionDetailItemModifierResponse> Modifiers { get; set; } = new();
    }
}
