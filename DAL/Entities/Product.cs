using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string? Description { get; set; }

        public decimal UnitPrice { get; set; }

        public int TaxRateId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Category? Category { get; set; }

        public TaxRate? TaxRate { get; set; }

        public ICollection<TransactionItem> TransactionItems { get; set; } = new List<TransactionItem>();
    }
}