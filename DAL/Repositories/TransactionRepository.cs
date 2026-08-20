using System.Data;
using Contracts.Transactions;
using DAL.Entities;
using DAL.Entities.Data;
using DAL.Infrastructure;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        private readonly ISqlConnectionStringProvider _connectionStringProvider;

        public TransactionRepository(
            IDbContextFactory<PosDbContext> contextFactory,
            ISqlConnectionStringProvider connectionStringProvider) : base(contextFactory)
        {
            _connectionStringProvider = connectionStringProvider;
        }

        public async Task<PagedResult<TransactionListItemDto>> GetTransactionsListAsync(
            GetTransactionsListRequest request,
            CancellationToken ct = default)
        {
            // If Custom period is requested but FromDate is null, default to Today
            if (string.Equals(request.PeriodType, "Custom", StringComparison.OrdinalIgnoreCase) && !request.FromDate.HasValue)
            {
                request.PeriodType = "Today";
            }

            await using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
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
                    items.Add(new TransactionListItemDto
                    {
                        TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
                        ReceiptNumber = reader.GetString(reader.GetOrdinal("ReceiptNumber")),
                        GrandTotal = reader.GetDecimal(reader.GetOrdinal("GrandTotal")),
                        Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                            ? null
                            : reader.GetString(reader.GetOrdinal("Notes")),
                        PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                        Status = reader.IsDBNull(reader.GetOrdinal("Status"))
                            ? string.Empty
                            : reader.GetString(reader.GetOrdinal("Status")),
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

        private const int SearchResultCap = 25;

        public async Task<IEnumerable<TransactionListItemDto>> SearchByReceiptNumberAsync(
            string receiptNumber,
            CancellationToken ct = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);

            var matches = await context.Set<Transaction>()
                .AsNoTracking()
                .Include(t => t.Payments)
                .Where(t => EF.Functions.Like(t.ReceiptNumber, $"%{receiptNumber}%"))
                .OrderByDescending(t => t.TransactionDate)
                .Take(SearchResultCap)
                .ToListAsync(ct);

            return matches.Select(t => new TransactionListItemDto
            {
                TransactionId = t.TransactionId,
                ReceiptNumber = t.ReceiptNumber,
                GrandTotal = t.GrandTotal,
                Notes = t.Notes,
                PaymentMethod = t.Payments.FirstOrDefault()?.PaymentMethod ?? string.Empty,
                Status = t.Status.ToString(),
                TransactionDate = t.TransactionDate
            });
        }

        /// <summary>
        /// Atomically voids a transaction by calling dbo.SP_VoidTransaction,
        /// which updates only completed transactions and returns the number
        /// of rows changed.
        /// </summary>
        public async Task<bool> VoidTransactionAsync(
            int transactionId,
            string? voidReason,
            CancellationToken ct = default)
        {
            await using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
            await connection.OpenAsync(ct);

            await using var command = new SqlCommand("SP_VoidTransaction", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@TransactionId", SqlDbType.Int).Value = transactionId;
            command.Parameters.Add("@VoidReason", SqlDbType.NVarChar, 500).Value =
                (object?)voidReason ?? DBNull.Value;

            object? result = await command.ExecuteScalarAsync(ct);
            int rowsUpdated = Convert.ToInt32(result ?? 0);

            return rowsUpdated == 1;
        }

        public async Task<Transaction?> GetTransactionDetailAsync(int transactionId, CancellationToken ct = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);

            return await context.Set<Transaction>()
                .AsNoTracking()
                .Include(t => t.TransactionItems)
                    .ThenInclude(i => i.ModifierItems)
                .Include(t => t.Payments)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId, ct);
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
