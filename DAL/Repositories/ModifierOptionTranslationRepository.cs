using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class ModifierOptionTranslationRepository
    : Repository<ModifierOptionTranslation>, IModifierOptionTranslationRepository
{
    public ModifierOptionTranslationRepository(IDbContextFactory<PosDbContext> contextFactory)
        : base(contextFactory)
    {
    }

    public async Task<IEnumerable<ModifierOptionTranslation>> GetByModifierOptionIdAsync(int modifierOptionId)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ModifierOptionTranslations
            .Where(t => t.ModifierOptionId == modifierOptionId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ModifierOptionTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ModifierOptionTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Name == name &&
                t.LanguageCode == languageCode);
    }

    public async Task<ModifierOptionTranslation?> GetByIdAndLanguageCodeAsync(int modifierOptionId, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ModifierOptionTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.ModifierOptionId == modifierOptionId &&
                t.LanguageCode == languageCode);
    }

    public async Task<IEnumerable<ModifierOptionTranslation>> GetAllByLanguageCodeAsync(string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ModifierOptionTranslations
            .Where(t => t.LanguageCode == languageCode)
            .AsNoTracking()
            .ToListAsync();
    }
}
