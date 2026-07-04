using Contracts.Shifts;
using DAL.Entities.Data;
using DAL.Interfaces;
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
    }
}
