using System.Data;
using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories
{
    public class ShiftRepository : Repository<Shift>, IShiftRepository
    {
        public ShiftRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<Shift?> GetOpenShiftAsync(int userId)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Shifts.
                FirstOrDefaultAsync(s => s.UserId == userId
                                     && s.Status == ShiftStatus.Open);
        }

        public async Task<IEnumerable<Shift>> GetLastShiftsAsync(int count)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Shifts
                .Include(s => s.User)
                .OrderByDescending(s => s.OpenedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<decimal> GetShiftTotalSalesAsync(int shiftId, CancellationToken ct = default)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync(ct);
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            const string sql = "SELECT dbo.FN_GetShiftTotalSales(@ShiftId)";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;

            var result = await command.ExecuteScalarAsync(ct);
            return result is DBNull ? 0m : Convert.ToDecimal(result);
        }

        public async Task<decimal> GetShiftCashTotalAsync(int shiftId, CancellationToken ct = default)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync(ct);
            string connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Connection string not found.");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            const string sql = "SELECT dbo.FN_GetShiftCashTotal(@ShiftId)";

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.Add("@ShiftId", SqlDbType.Int).Value = shiftId;

            var result = await command.ExecuteScalarAsync(ct);
            return result is DBNull ? 0m : Convert.ToDecimal(result);
        }

        public async Task AddOpenShiftAsync(Shift shift)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();

            try
            {
                await context.Shifts.AddAsync(shift);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx)
            {
                throw TranslateSqlException(sqlEx);
            }
        }

        /// <summary>
        /// Translates the SQL Server unique-index violation raised by
        /// UX_Shifts_OpenShift_User (error 2601, or 2627 if it's ever
        /// redefined as a constraint) into the same friendly message
        /// ShiftService's in-memory pre-check already returns for the
        /// common case. Any other SQL error is passed through as a
        /// generic failure rather than leaking raw SQL Server text.
        /// </summary>
        private static Exception TranslateSqlException(SqlException ex)
        {
            if (ex.Number is 2601 or 2627)
            {
                return new InvalidOperationException(
                    "Cannot start a new shift. An open shift already exists for this user.", ex);
            }

            return new InvalidOperationException("An error occurred while opening the shift.", ex);
        }
    }
}
