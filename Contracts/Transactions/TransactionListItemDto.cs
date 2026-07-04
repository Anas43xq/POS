namespace Contracts.Transactions
{
    public class TransactionListItemDto
    {
        public int TransactionId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string ? Status { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
