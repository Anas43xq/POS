using System;

namespace DAL.Entities
{
    public class ProductReportEntity
    {
        public string ReceiptNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}