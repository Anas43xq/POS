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
