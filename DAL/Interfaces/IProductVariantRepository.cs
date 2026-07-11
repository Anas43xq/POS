using DAL.Entities;

namespace DAL.Interfaces;

public interface IProductVariantRepository : IRepository<ProductVariant>
{
    Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId);
}