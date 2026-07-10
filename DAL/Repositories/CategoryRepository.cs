using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
    {
    }

    public async Task<IEnumerable<Category>> GetAllWithChildrenAsync()
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.Categories
            .Include(c => c.Products)
            .Include(c => c.ChildCategories)
                .ThenInclude(cc => cc.Products)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetChildrenAsync(int parentCategoryId)
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.Categories
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .AsNoTracking()
            .ToListAsync();
    }
}

