using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductsWithTaxRateAsync();

        Task<IEnumerable<Product>> GetProductSummariesAsync();

        Task<IEnumerable<ProductVariant>> GetAllVariantsAsync();

        /// <summary>
        /// Loads every active product together with its TaxRate and its
        /// ProductVariants (+ Size) in one query (split into a small,
        /// fixed number of round trips via AsSplitQuery rather than one
        /// query per product), so callers can derive a price range without
        /// N+1 variant lookups.
        /// </summary>
        Task<IEnumerable<Product>> GetAllProductsWithVariantsAsync();

        /// <summary>
        /// Loads a single product together with its ProductVariants (+ Size)
        /// for the product management edit form.
        /// </summary>
        Task<Product?> GetByIdWithVariantsAsync(int id);
    }
}
