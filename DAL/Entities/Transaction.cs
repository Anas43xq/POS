using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        public string ReceiptNumber { get; set; } = string.Empty;

        public int ShiftId { get; set; }

        public int CashierId { get; set; }

        public DateTime TransactionDate { get; set; }

        public decimal Subtotal { get; set; }

        public decimal TaxTotal { get; set; }

        public decimal GrandTotal { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }


        // Navigation

        public Shift? Shift { get; set; }

        public User? Cashier { get; set; }

        public ICollection<TransactionItem> TransactionItems { get; set; }
            = new List<TransactionItem>();

        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
    }
}