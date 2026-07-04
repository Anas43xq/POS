using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<IEnumerable<Product>> GetAllProductsWithTaxRateAsync()
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Products
             .Include(p => p.TaxRate)
             .Where(p => p.IsActive)
             .ToListAsync();
        }
    }
}

