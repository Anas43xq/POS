using DAL.Entities;

namespace DAL.Interfaces;

public interface IProductVariantRepository : IRepository<ProductVariant>
{
    Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Attempts to hard-delete the variant. If it's referenced by
    /// historical TransactionItems (FK is NoAction, so the delete would
    /// otherwise throw), it's deactivated instead so the size stops
    /// being sellable without touching sale history.
    /// </summary>
    Task<VariantDeleteOutcome> TryDeleteAsync(int variantId);
}