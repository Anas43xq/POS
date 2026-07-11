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
}