using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class ModifierOptionRepository : Repository<ModifierOption>, IModifierOptionRepository
{
    private readonly IDbContextFactory<PosDbContext> _asyncContextFactory;

    public ModifierOptionRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
    {
        _asyncContextFactory = contextFactory;
    }

    public async Task<IEnumerable<ModifierOption>> GetByGroupIdAsync(int modifierGroupId)
    {
        await using var context = await _asyncContextFactory.CreateDbContextAsync();
        return await context.ModifierOptions
            .Include(o => o.ModifierOptionTranslations)
            .Where(o => o.ModifierGroupId == modifierGroupId && o.IsActive)
            .OrderBy(o => o.SortOrder)
            .AsNoTracking()
            .ToListAsync();
    }
}