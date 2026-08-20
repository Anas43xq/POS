using System.Collections.Generic;

namespace DAL.Entities
{
    public class PurchaseReceiptType
    {
        public byte ReceiptTypeId { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<PurchaseReceipt> PurchaseReceipts { get; set; }
            = new List<PurchaseReceipt>();
    }
}
