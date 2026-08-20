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
             .AsNoTracking()
             .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductSummariesAsync()
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            var products = await context.Products
                .Where(p => p.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return products;
        }

        public async Task<IEnumerable<ProductVariant>> GetAllVariantsAsync()
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.ProductVariants
                .AsNoTracking()
                .Where(v => v.IsActive)
                .Include(v => v.Product)
                    .ThenInclude(p => p.TaxRate)
                .Include(v => v.Product)
                    .ThenInclude(p => p.Category)
                .Include(v => v.Size)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllProductsWithVariantsAsync()
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Products
                .Include(p => p.TaxRate)
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Size)
                .Where(p => p.IsActive)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<Product?> GetByIdWithVariantsAsync(int id)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Products
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.Size)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }
    }
}
