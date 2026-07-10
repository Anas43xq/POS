using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class CategoryTranslationRepository
    : Repository<CategoryTranslation>, ICategoryTranslationRepository
{
    public CategoryTranslationRepository(IDbContextFactory<PosDbContext> contextFactory)
        : base(contextFactory)
    {
    }

    public async Task<IEnumerable<CategoryTranslation>> GetByCategoryIdAsync(int categoryId)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.CategoryTranslations
            .Where(t => t.CategoryId == categoryId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CategoryTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.CategoryTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Name == name &&
                t.LanguageCode == languageCode);
    }

    public async Task<CategoryTranslation?> GetByIdAndLanguageCodeAsync(int categoryId, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.CategoryTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.CategoryId == categoryId &&
                t.LanguageCode == languageCode);
    }
}
