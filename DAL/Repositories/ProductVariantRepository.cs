using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class ProductVariantRepository
    : Repository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(IDbContextFactory<PosDbContext> contextFactory)
        : base(contextFactory)
    {
    }

    public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ProductVariants
            .Include(v => v.Size)
            .Where(v => v.ProductId == productId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<VariantDeleteOutcome> TryDeleteAsync(int variantId)
    {
        await using (var context = await _contextFactory!.CreateDbContextAsync())
        {
            var entity = await context.ProductVariants.FindAsync(variantId);
            if (entity is null)
                return VariantDeleteOutcome.NotFound;

            context.ProductVariants.Remove(entity);
            try
            {
                await context.SaveChangesAsync();
                return VariantDeleteOutcome.Deleted;
            }
            catch (DbUpdateException)
            {
                // FK from TransactionItems prevents deletion; deactivate instead
                // using fresh context since tracked delete failed.
            }
        }

        await using var fallbackContext = await _contextFactory!.CreateDbContextAsync();
        var toDeactivate = await fallbackContext.ProductVariants.FindAsync(variantId);
        if (toDeactivate is null)
            return VariantDeleteOutcome.NotFound;

        toDeactivate.IsActive = false;
        await fallbackContext.SaveChangesAsync();
        return VariantDeleteOutcome.Deactivated;
    }
}