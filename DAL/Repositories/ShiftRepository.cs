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
    }
}
