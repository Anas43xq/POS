using Contracts.Sales;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class TopProductRepository : ITopProductRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public TopProductRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(int take = 7)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TransactionItems
                .AsNoTracking()
                .GroupBy(ti => ti.ProductName)
                .Select(g => new TopProductDto
                {
                    ProductName = g.Key,
                    TotalSales = g.Sum(ti => ti.LineTotal)
                })
                .OrderByDescending(x => x.TotalSales)
                .ThenBy(x => x.ProductName)
                .Take(take)
                .ToListAsync();
        }
    }
}