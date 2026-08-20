using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class SizeTranslationRepository
    : Repository<SizeTranslation>, ISizeTranslationRepository
{
    public SizeTranslationRepository(IDbContextFactory<PosDbContext> contextFactory)
        : base(contextFactory)
    {
    }

    public async Task<IEnumerable<SizeTranslation>> GetBySizeIdAsync(int sizeId)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.SizeTranslations
            .Where(t => t.SizeId == sizeId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SizeTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.SizeTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Name == name &&
                t.LanguageCode == languageCode);
    }

    public async Task<SizeTranslation?> GetByIdAndLanguageCodeAsync(int sizeId, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.SizeTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.SizeId == sizeId &&
                t.LanguageCode == languageCode);
    }

    public async Task<IEnumerable<SizeTranslation>> GetAllByLanguageCodeAsync(string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.SizeTranslations
            .Where(t => t.LanguageCode == languageCode)
            .AsNoTracking()
            .ToListAsync();
    }
}
