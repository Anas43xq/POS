using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories;

public class SizeRepository : Repository<Size>, ISizeRepository
{
    public SizeRepository(IDbContextFactory<PosDbContext> contextFactory)
        : base(contextFactory)
    {
    }

    public async Task<IEnumerable<Size>> GetAllOrderedAsync()
    {
        await using var context = await _contextFactory!.CreateDbContextAsync();
        return await context.Sizes
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }
}