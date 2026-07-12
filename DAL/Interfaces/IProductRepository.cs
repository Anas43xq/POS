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
    }
}
