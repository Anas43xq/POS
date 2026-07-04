using Contracts.Sales;
using Contracts.Transactions;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class KpiRepository : IKpiRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public KpiRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<KpiDto> GetKpisAsync(GetTransactionKpisRequest request, CancellationToken ct = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var command = new SqlCommand("SP_GetTransactionKpis", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@PeriodType", SqlDbType.NVarChar, 10).Value = request.PeriodType;
            command.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object?)request.FromDate ?? DBNull.Value;
            command.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object?)request.ToDate ?? DBNull.Value;

            try
            {
                await using SqlDataReader reader = await command.ExecuteReaderAsync(ct);

                if (!await reader.ReadAsync(ct))
                    throw new InvalidOperationException("KPI query returned no result.");

                return new KpiDto
                {
                    TotalSales = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                    AverageSale = reader.GetDecimal(reader.GetOrdinal("TotalAverage")),
                    TotalOrders = reader.GetInt32(reader.GetOrdinal("TotalOrders")),
                    TotalCash = reader.GetDecimal(reader.GetOrdinal("TotalCash")),
                    TotalCard = reader.GetDecimal(reader.GetOrdinal("TotalCard"))
                };
            }
            catch (SqlException ex)
            {
                throw TranslateSqlException(ex);
            }
        }

        private static Exception TranslateSqlException(SqlException ex)
        {
            // Messages raised by SP_GetTransactionKpis's RAISERROR calls.
            // SqlException.Number is 50000 for RAISERROR-raised errors without an explicit error number.
            if (ex.Number == 50000)
            {
                if (ex.Message.Contains("FromDate is required", StringComparison.OrdinalIgnoreCase))
                    return new InvalidOperationException("FromDate is required for a custom period.", ex);

                if (ex.Message.Contains("Invalid PeriodType", StringComparison.OrdinalIgnoreCase))
                    return new InvalidOperationException("Invalid PeriodType. Expected Today, Week, Month, or Custom.", ex);

                return new InvalidOperationException(ex.Message, ex);
            }

            return new InvalidOperationException("An error occurred while retrieving transaction KPIs.", ex);
        }
    }
}
