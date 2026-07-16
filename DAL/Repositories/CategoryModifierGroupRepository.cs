using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class CategoryModifierGroupRepository : Repository<CategoryModifierGroup>, ICategoryModifierGroupRepository
{
    private readonly IDbContextFactory<PosDbContext> _asyncContextFactory;

    public CategoryModifierGroupRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
    {
        _asyncContextFactory = contextFactory;
    }

    public async Task<IEnumerable<CategoryModifierGroup>> GetByCategoryIdAsync(int categoryId)
    {
        await using var context = await _asyncContextFactory.CreateDbContextAsync();
        return await context.CategoryModifierGroups
            .Include(cmg => cmg.ModifierGroup)
            .Where(cmg => cmg.CategoryId == categoryId)
            .AsNoTracking()
            .ToListAsync();
    }
}