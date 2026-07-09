using System.Data;
using Contracts.Transactions;
using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<PagedResult<TransactionListItemDto>> GetTransactionsListAsync(
            GetTransactionsListRequest request,
            CancellationToken ct = default)
        {
            if (string.Equals(request.PeriodType, "Custom", StringComparison.OrdinalIgnoreCase) && !request.FromDate.HasValue)
            {
                throw new InvalidOperationException("FromDate is required for a custom period.");
            }

            await using var context = await _contextFactory!.CreateDbContextAsync(ct);
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var command = new SqlCommand("SP_GetTransactionsList", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@PeriodType", SqlDbType.NVarChar, 10).Value = request.PeriodType;
            command.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object?)request.FromDate ?? DBNull.Value;
            command.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object?)request.ToDate ?? DBNull.Value;
            command.Parameters.Add("@StatusFilter", SqlDbType.NVarChar, 20).Value =
                (object?)request.StatusFilter ?? DBNull.Value;
            command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = request.PageNumber;
            command.Parameters.Add("@PageSize", SqlDbType.Int).Value = request.PageSize;

            try
            {
                await using SqlDataReader reader = await command.ExecuteReaderAsync(ct);

                if (!await reader.ReadAsync(ct))
                    throw new InvalidOperationException("Transaction list query returned no count result.");

                int totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

                if (!await reader.NextResultAsync(ct))
                    throw new InvalidOperationException("Transaction list query returned no rows result set.");

                var items = new List<TransactionListItemDto>();

                while (await reader.ReadAsync(ct))
                {
                    int statusOrdinal = reader.GetOrdinal("Status");
                    items.Add(new TransactionListItemDto
                    {
                        TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
                        ReceiptNumber = reader.GetString(reader.GetOrdinal("ReceiptNumber")),
                        GrandTotal = reader.GetDecimal(reader.GetOrdinal("GrandTotal")),
                        Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Notes")),
                        PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        Status = reader.GetString(reader.GetOrdinal("Status")) ?? String.Empty,
                        TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate"))
                    });
                }

                return new PagedResult<TransactionListItemDto>
                {
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    Items = items
                };
            }
            catch (SqlException ex)
            {
                throw TranslateSqlException(ex);
            }
        }

        /// <summary>
        /// Atomically voids a transaction by issuing a single UPDATE with a
        /// status guard in the WHERE clause.  Avoids the load-mutate-save
        /// round-trip (and any cross-context tracking pitfalls).
        /// </summary>
        public async Task<bool> VoidTransactionAsync(int transactionId, CancellationToken ct = default)
        {
            if (_contextFactory is null)
                throw new InvalidOperationException("VoidTransactionAsync requires a DbContextFactory.");

            await using var context = await _contextFactory.CreateDbContextAsync(ct);

            int rows = await context.Set<Transaction>()
                .Where(t => t.TransactionId == transactionId
                            && t.Status == TransactionStatus.Completed)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(t => t.Status, TransactionStatus.Voided),
                    ct);

            return rows == 1;
        }

        private static Exception TranslateSqlException(SqlException ex)
        {
            if (ex.Number == 50000)
            {
                return new InvalidOperationException(ex.Message, ex);
            }

            return new InvalidOperationException("An error occurred while retrieving transactions.", ex);
        }
    }
}
