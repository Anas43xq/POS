using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class TransactionItem
    {
        public int TransactionItemId { get; set; }

        public int TransactionId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal TaxRate { get; set; }

        public decimal LineSubtotal { get; set; }

        public decimal LineTax { get; set; }

        public decimal LineTotal { get; set; }


        // Navigation

        public Transaction? Transaction { get; set; }

        public Product? Product { get; set; }
    }
}