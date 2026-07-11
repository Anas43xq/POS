using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class ProductTranslationRepository
    : Repository<ProductTranslation>, IProductTranslationRepository
{
    public ProductTranslationRepository(IDbContextFactory<PosDbContext> contextFactory)
        : base(contextFactory)
    {
    }

    public async Task<IEnumerable<ProductTranslation>> GetByProductIdAsync(int productId)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ProductTranslations
            .Where(t => t.ProductId == productId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ProductTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ProductTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.Name == name &&
                t.LanguageCode == languageCode);
    }

    public async Task<ProductTranslation?> GetByIdAndLanguageCodeAsync(int productId, string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ProductTranslations
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.ProductId == productId &&
                t.LanguageCode == languageCode);
    }

    public async Task<IEnumerable<ProductTranslation>> GetAllByLanguageCodeAsync(string languageCode)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.ProductTranslations
            .Where(t => t.LanguageCode == languageCode)
            .AsNoTracking()
            .ToListAsync();
    }
}
