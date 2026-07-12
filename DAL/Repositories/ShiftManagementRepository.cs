using System.Data;
using Contracts.Shifts;
using Contracts.Transactions;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ShiftManagementRepository : IShiftManagementRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public ShiftManagementRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ShiftManagementDto>> GetAllShiftManagementAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Set<DAL.Entities.ShiftManagementView>()
                .AsNoTracking()
                .OrderByDescending(x => x.OpenedAt)
                .Select(x => new ShiftManagementDto
                {
                    ShiftId = x.ShiftId,
                    UserId = x.UserId,
                    CashierName = x.CashierName,
                    OpenedAt = x.OpenedAt,
                    ClosedAt = x.ClosedAt,
                    OpeningCash = x.OpeningCash,
                    ClosingCash = x.ClosingCash,
                    ExpectedCash = x.ExpectedCash,
                    CashDifference = x.CashDifference,
                    StatusLabel = x.StatusLabel,
                    DurationHours = x.DurationHours
                })
                .ToListAsync();
        }

        public async Task<PagedResult<ShiftListItemDto>> GetShiftsListAsync(
            GetShiftsListRequest request, CancellationToken ct = default)
        {
            if (string.Equals(request.PeriodType, "Custom", StringComparison.OrdinalIgnoreCase)
                && !request.FromDate.HasValue)
            {
                request.PeriodType = "Today";
            }

            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var command = new SqlCommand("SP_GetShiftsList", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@PeriodType", SqlDbType.NVarChar, 10).Value = request.PeriodType;
            command.Parameters.Add("@FromDate", SqlDbType.Date).Value = (object?)request.FromDate ?? DBNull.Value;
            command.Parameters.Add("@ToDate", SqlDbType.Date).Value = (object?)request.ToDate ?? DBNull.Value;
            command.Parameters.Add("@StatusFilter", SqlDbType.NVarChar, 10).Value =
                (object?)request.StatusFilter ?? DBNull.Value;
            command.Parameters.Add("@CashierId", SqlDbType.Int).Value =
                (object?)request.CashierId ?? DBNull.Value;
            command.Parameters.Add("@PageNumber", SqlDbType.Int).Value = request.PageNumber;
            command.Parameters.Add("@PageSize", SqlDbType.Int).Value = request.PageSize;

            try
            {
                await using SqlDataReader reader = await command.ExecuteReaderAsync(ct);

                if (!await reader.ReadAsync(ct))
                    throw new InvalidOperationException("Shift list query returned no count result.");

                int totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));

                if (!await reader.NextResultAsync(ct))
                    throw new InvalidOperationException("Shift list query returned no rows result set.");

                var items = new List<ShiftListItemDto>();

                while (await reader.ReadAsync(ct))
                {
                    items.Add(new ShiftListItemDto
                    {
                        ShiftId = reader.GetInt32(reader.GetOrdinal("ShiftId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        CashierName = reader.GetString(reader.GetOrdinal("CashierName")),
                        OpenedAt = reader.GetDateTime(reader.GetOrdinal("OpenedAt")),
                        ClosedAt = reader.IsDBNull(reader.GetOrdinal("ClosedAt"))
                            ? null
                            : reader.GetDateTime(reader.GetOrdinal("ClosedAt")),
                        OpeningCash = reader.GetDecimal(reader.GetOrdinal("OpeningCash")),
                        ClosingCash = reader.IsDBNull(reader.GetOrdinal("ClosingCash"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("ClosingCash")),
                        ExpectedCash = reader.IsDBNull(reader.GetOrdinal("ExpectedCash"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("ExpectedCash")),
                        CashDifference = reader.IsDBNull(reader.GetOrdinal("CashDifference"))
                            ? null
                            : reader.GetDecimal(reader.GetOrdinal("CashDifference")),
                        StatusLabel = reader.GetString(reader.GetOrdinal("StatusLabel")),
                        DurationHours = Convert.ToDouble(reader.GetValue(reader.GetOrdinal("DurationHours")) ?? 0.0)
                    });
                }

                return new PagedResult<ShiftListItemDto>
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

        public async Task<ShiftDetailDto?> GetShiftDetailAsync(int shiftId, CancellationToken ct = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var command = new SqlCommand("SP_GetShiftDetail", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;

            try
            {
                await using SqlDataReader reader = await command.ExecuteReaderAsync(ct);

                // Result Set 1: Shift info + stats
                if (!await reader.ReadAsync(ct))
                    return null;

                var detail = new ShiftDetailDto
                {
                    ShiftId = reader.GetInt32(reader.GetOrdinal("ShiftId")),
                    CashierName = reader.GetString(reader.GetOrdinal("CashierName")),
                    OpenedAt = reader.GetDateTime(reader.GetOrdinal("OpenedAt")),
                    ClosedAt = reader.IsDBNull(reader.GetOrdinal("ClosedAt"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("ClosedAt")),
                    DurationHours = Convert.ToDouble(reader.GetValue(reader.GetOrdinal("DurationHours")) ?? 0.0),
                    OpeningCash = reader.GetDecimal(reader.GetOrdinal("OpeningCash")),
                    ClosingCash = reader.IsDBNull(reader.GetOrdinal("ClosingCash"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("ClosingCash")),
                    ExpectedCash = reader.IsDBNull(reader.GetOrdinal("ExpectedCash"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("ExpectedCash")),
                    CashDifference = reader.IsDBNull(reader.GetOrdinal("CashDifference"))
                        ? null
                        : reader.GetDecimal(reader.GetOrdinal("CashDifference")),
                    StatusLabel = reader.GetString(reader.GetOrdinal("StatusLabel")),
                    TotalTransactions = reader.GetInt32(reader.GetOrdinal("TotalTransactions")),
                    CashTransactions = reader.GetInt32(reader.GetOrdinal("CashTransactions")),
                    CardTransactions = reader.GetInt32(reader.GetOrdinal("CardTransactions")),
                    CashSales = reader.GetDecimal(reader.GetOrdinal("CashSales")),
                    CardSales = reader.GetDecimal(reader.GetOrdinal("CardSales")),
                    TotalSales = reader.GetDecimal(reader.GetOrdinal("TotalSales")),
                    RefundsCount = reader.GetInt32(reader.GetOrdinal("RefundsCount"))
                };

                // Result Set 2: Transactions
                if (await reader.NextResultAsync(ct))
                {
                    while (await reader.ReadAsync(ct))
                    {
                        detail.Transactions.Add(new ShiftTransactionDto
                        {
                            TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
                            ReceiptNumber = reader.GetString(reader.GetOrdinal("ReceiptNumber")),
                            TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate")),
                            PaymentMethod = reader.IsDBNull(reader.GetOrdinal("PaymentMethod"))
                                ? string.Empty
                                : reader.GetString(reader.GetOrdinal("PaymentMethod")),
                            GrandTotal = reader.GetDecimal(reader.GetOrdinal("GrandTotal")),
                            Status = reader.GetString(reader.GetOrdinal("Status"))
                        });
                    }
                }

                return detail;
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

            return new InvalidOperationException("An error occurred while retrieving shift data.", ex);
        }
    }
}
