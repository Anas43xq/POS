using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class ModifierGroupTranslationRepository
    : Repository<ModifierGroupTranslation>, IModifierGroupTranslationRepository
{
    public ModifierGroupTranslationRepository(IDbContextFactory<PosDbContext> contextFactory)
        : base(contextFactory)
    {
    }

    public async Task<IEnumerable<ModifierGroupTranslation>> GetByModifierGroupIdAsync(int modifierGroupId)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ModifierGroupTranslations
            .Where(t => t.ModifierGroupId == modifierGroupId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ModifierGroupTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ModifierGroupTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Name == name &&
                t.LanguageCode == languageCode);
    }

    public async Task<ModifierGroupTranslation?> GetByIdAndLanguageCodeAsync(int modifierGroupId, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ModifierGroupTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.ModifierGroupId == modifierGroupId &&
                t.LanguageCode == languageCode);
    }

    public async Task<IEnumerable<ModifierGroupTranslation>> GetAllByLanguageCodeAsync(string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ModifierGroupTranslations
            .Where(t => t.LanguageCode == languageCode)
            .AsNoTracking()
            .ToListAsync();
    }
}
