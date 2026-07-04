using System;

namespace Contracts.Sales
{
    public class RecentTransactionDto
    {
        public int TransactionId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string? CashierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
