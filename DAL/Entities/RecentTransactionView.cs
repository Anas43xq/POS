using System;

namespace DAL.Entities
{
    public class RecentTransactionView
    {
        public int TransactionId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public byte Status { get; set; }
    }
}
