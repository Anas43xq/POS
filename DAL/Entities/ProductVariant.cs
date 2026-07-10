using System;
using System.Collections.Generic;

namespace DAL.Entities;

public class ProductVariant
{
    public int VariantId { get; set; }

    public int ProductId { get; set; }

    public int SizeId { get; set; }

    public decimal UnitPrice { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;

    public Size Size { get; set; } = null!;

    public ICollection<TransactionItem> TransactionItems { get; set; } = [];
}
