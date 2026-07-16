using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class ProductModifierGroupRepository : Repository<ProductModifierGroup>, IProductModifierGroupRepository
{
    private readonly IDbContextFactory<PosDbContext> _asyncContextFactory;

    public ProductModifierGroupRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
    {
        _asyncContextFactory = contextFactory;
    }

    public async Task<IEnumerable<ProductModifierGroup>> GetByProductIdAsync(int productId)
    {
        await using var context = await _asyncContextFactory.CreateDbContextAsync();
        return await context.ProductModifierGroups
            .Include(pmg => pmg.ModifierGroup)
            .Where(pmg => pmg.ProductId == productId)
            .AsNoTracking()
            .ToListAsync();
    }
}