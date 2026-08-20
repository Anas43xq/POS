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

    public async Task<IEnumerable<ModifierGroup>> GetByIdsWithOptionsAndTranslationsAsync(IEnumerable<int> groupIds)
    {
        var idSet = groupIds as ICollection<int> ?? groupIds.ToList();
        if (idSet.Count == 0)
            return Enumerable.Empty<ModifierGroup>();

        await using var context = await _asyncContextFactory.CreateDbContextAsync();
        return await context.ModifierGroups
            .Where(mg => idSet.Contains(mg.ModifierGroupId))
            .Include(mg => mg.ModifierOptions)
                .ThenInclude(o => o.ModifierOptionTranslations)
            .Include(mg => mg.ModifierGroupTranslations)
            .OrderBy(mg => mg.SortOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ModifierGroup?> GetByIdWithAssignmentsAsync(int groupId)
    {
        await using var context = await _asyncContextFactory.CreateDbContextAsync();
        return await context.ModifierGroups
            .Where(mg => mg.ModifierGroupId == groupId)
            .Include(mg => mg.CategoryModifierGroups)
            .Include(mg => mg.ProductModifierGroups)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}