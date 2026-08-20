using Contracts.Sales;
using Contracts.Transactions;
using DAL.Entities;
using DAL.Entities.Data;
using DAL.Infrastructure;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class TopProductRepository : ITopProductRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;
        private readonly ISqlConnectionStringProvider _connectionStringProvider;

        public TopProductRepository(
            IDbContextFactory<PosDbContext> contextFactory,
            ISqlConnectionStringProvider connectionStringProvider)
        {
            _contextFactory = contextFactory;
            _connectionStringProvider = connectionStringProvider;
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(int take = 7)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TransactionItems
                .AsNoTracking()
                .Where(ti => ti.Transaction != null && ti.Transaction.Status == TransactionStatus.Completed)
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

        public async Task<List<TopProductDto>> GetTopProductsAsync(GetTransactionKpisRequest request, int take = 7)
        {
            await using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SP_GetTopProducts", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@PeriodType", SqlDbType.NVarChar, 10).Value = request.PeriodType;
            command.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object?)request.FromDate ?? DBNull.Value;
            command.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object?)request.ToDate ?? DBNull.Value;
            command.Parameters.Add("@TopCount", SqlDbType.Int).Value = take;

            await using var reader = await command.ExecuteReaderAsync();

            var items = new List<TopProductDto>();
            while (await reader.ReadAsync())
            {
                items.Add(new TopProductDto
                {
                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                    TotalSales = reader.GetDecimal(reader.GetOrdinal("TotalSales"))
                });
            }

            return items;
        }
    }
}
