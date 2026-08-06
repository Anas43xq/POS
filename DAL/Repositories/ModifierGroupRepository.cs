using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class ModifierGroupRepository : Repository<ModifierGroup>, IModifierGroupRepository
{
    private readonly IDbContextFactory<PosDbContext> _asyncContextFactory;

    public ModifierGroupRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
    {
        _asyncContextFactory = contextFactory;
    }

    public async Task<IEnumerable<ModifierGroup>> GetAllWithOptionsAndTranslationsAsync()
    {
        await using var context = await _asyncContextFactory.CreateDbContextAsync();
        return await context.ModifierGroups
            .Include(mg => mg.ModifierOptions)
                .ThenInclude(o => o.ModifierOptionTranslations)
            .Include(mg => mg.ModifierGroupTranslations)
            .OrderBy(mg => mg.SortOrder)
            .AsNoTracking()
            .ToListAsync();
    }
}