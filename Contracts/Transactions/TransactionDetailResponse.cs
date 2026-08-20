namespace Contracts.Transactions
{
    public class TransactionDetailResponse
    {
        public int TransactionId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public List<TransactionDetailItemResponse> Items { get; set; } = new();
    }
}
