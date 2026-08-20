using System.Collections.Generic;

namespace Contracts.Products
{
    /// <summary>
    /// API response shape for a single product: one row per product,
    /// carrying its sellable Size/Price variants nested underneath —
    /// never one row per size. ProductVariant.UnitPrice is the only
    /// selling price; the product itself carries none.
    /// </summary>
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int TaxRateId { get; set; }
        public string TaxRateName { get; set; } = string.Empty;
        public decimal TaxRatePercentage { get; set; }
        public bool IsActive { get; set; }
        public List<ProductVariantResponse> Variants { get; set; } = new();
    }

    public class ProductVariantResponse
    {
        public int VariantId { get; set; }
        public string Size { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
    }
}
