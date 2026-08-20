using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class Supplier
    {
        public int SupplierId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string? TRN { get; set; }

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PurchaseReceipt> PurchaseReceipts { get; set; }
            = new List<PurchaseReceipt>();
    }
}
