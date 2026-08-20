using System;

namespace DAL.Entities
{
    public class PurchaseReceipt
    {
        public int ReceiptId { get; set; }

        public byte ReceiptTypeId { get; set; }

        public int? SupplierId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime InvoiceDate { get; set; }

        public string Category { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Subtotal { get; set; }

        public decimal VatRate { get; set; }

        public decimal VatAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string? Notes { get; set; }

        public string? ImagePath { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public PurchaseReceiptType? ReceiptType { get; set; }

        public Supplier? Supplier { get; set; }

        public User? CreatedByUser { get; set; }
    }
}
