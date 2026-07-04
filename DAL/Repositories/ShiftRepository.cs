using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
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
    }
}

