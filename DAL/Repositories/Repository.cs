using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
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
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity is not null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
