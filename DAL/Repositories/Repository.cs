using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories
{
    public class Repository<T> : IRepository<T>, IDisposable where T : class
    {
        protected readonly PosDbContext _context;
        protected readonly IDbContextFactory<PosDbContext>? _contextFactory;
        private readonly DbSet<T> _dbSet;

        public Repository(PosDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public Repository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            _context = contextFactory.CreateDbContext();
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            if (_contextFactory is null)
            {
                return await _dbSet
                    .AsNoTracking()
                    .ToListAsync();
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<T>()
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            if (_contextFactory is null)
            {
                return await _dbSet.FindAsync(id);
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<T>().FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            if (_contextFactory is null)
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                return;
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            if (_contextFactory is null)
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return;
            }

            // Use a fresh, short-lived context per update. Reusing the
            // long-lived _context created in the constructor would keep
            // tracking whatever entity instance was last saved through it;
            // a later save of a *different* instance with the same key
            // (e.g. re-fetched via GetByIdAsync) then throws
            // "instance ... cannot be tracked because another instance
            // with the same key value is already being tracked."
            await using var context = await _contextFactory.CreateDbContextAsync();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            if (_contextFactory is null)
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity is not null)
                {
                    _dbSet.Remove(entity);
                    await _context.SaveChangesAsync();
                }
                return;
            }

            await using var context = await _contextFactory.CreateDbContextAsync();
            var trackedEntity = await context.Set<T>().FindAsync(id);
            if (trackedEntity is not null)
            {
                context.Set<T>().Remove(trackedEntity);
                await context.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
