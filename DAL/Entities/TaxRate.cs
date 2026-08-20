using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class TaxRate
    {
        public int TaxRateId { get; set; }

        public string Name { get; set; } = string.Empty; // e.g. 'Standard VAT'

        public Decimal Rate { get; set; } // 0.1100 = 11%

        public DateTime CreatedAt {  get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<Product> Products { get; set; }
           = new List<Product>();

        public override string ToString() => $"{Name} ({Rate:P0})";
    }
}
