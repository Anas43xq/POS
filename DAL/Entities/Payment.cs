using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int TransactionId { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public decimal AmountTendered { get; set; }

        public decimal ChangeGiven { get; set; }

        public string? ReferenceNumber { get; set; }

        public DateTime PaidAt { get; set; }


        // Navigation

        public Transaction? Transaction { get; set; }
    }
}
