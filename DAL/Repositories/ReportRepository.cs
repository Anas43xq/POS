using System.Data;
using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Contracts.Services;

namespace DAL.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public ReportRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<TransactionReportEntity>> GetTransactionsReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SP_GetTransactionsReport", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // SP uses exclusive end-date logic: TransactionDate >= @FromDate AND TransactionDate < @ToDate
            // So we pass the raw toDate as-is — the SP adds 1 day internally for the exclusive upper bound
            command.Parameters.Add("@PeriodType", SqlDbType.NVarChar, 10).Value = periodType;
            command.Parameters.Add("@FromDate", SqlDbType.DateTime2).Value = (object?)fromDate ?? DBNull.Value;
            command.Parameters.Add("@ToDate", SqlDbType.DateTime2).Value = (object?)toDate ?? DBNull.Value;

            await using var reader = await command.ExecuteReaderAsync();

            var items = new List<TransactionReportEntity>();

            while (await reader.ReadAsync())
            {

                items.Add(new TransactionReportEntity
                {
                    TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
                    ReceiptNumber = Helpers.GetStringSafe(reader, "ReceiptNumber"),
                    TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate")),
                    PaymentMethod = Helpers.GetStringSafe(reader, "PaymentMethod"),
                    GrandTotal = reader.GetDecimal(reader.GetOrdinal("GrandTotal")),
                    Status = Helpers.GetStringSafe(reader, "Status"),
                    Note = Helpers.GetStringSafe(reader, "Notes")
                });
            }

            return items;
        }

        public async Task<List<ProductReportEntity>> GetProductSalesReportAsync(
            string periodType,
            DateTime? fromDate,
            DateTime? toDate,
            int? productId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SP_GetProductSalesReport", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@PeriodType", SqlDbType.NVarChar, 10).Value = periodType;
            command.Parameters.Add("@FromDate", SqlDbType.DateTime2).Value = (object?)fromDate ?? DBNull.Value;
            command.Parameters.Add("@ToDate", SqlDbType.DateTime2).Value = (object?)toDate ?? DBNull.Value;
            command.Parameters.Add("@ProductId", SqlDbType.Int).Value = (object?)productId ?? DBNull.Value;

            await using var reader = await command.ExecuteReaderAsync();

            var items = new List<ProductReportEntity>();

            while (await reader.ReadAsync())
            {
                items.Add(new ProductReportEntity
                {
                    ReceiptNumber = Helpers.GetStringSafe(reader, "ReceiptNumber"),
                    TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate")),
                    PaymentMethod = Helpers.GetStringSafe(reader, "PaymentMethod"),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    LineTotal = reader.GetDecimal(reader.GetOrdinal("LineTotal"))
                });
            }

            return items;
        }
    }
}