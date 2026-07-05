using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<Supplier?> GetByCompanyNameAsync(string companyName)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.CompanyName == companyName);
        }
    }
}
