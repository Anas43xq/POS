using System;

namespace DAL.Entities
{
    public class TransactionReportEntity
    {
        public int TransactionId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public string ?Status { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}