using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly IDbContextFactory<PosDbContext> _contextFactory;

        public Repository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<T>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<T>().FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var trackedEntity = await context.Set<T>().FindAsync(id);
            if (trackedEntity is not null)
            {
                context.Set<T>().Remove(trackedEntity);
                await context.SaveChangesAsync();
            }
        }
    }
}
