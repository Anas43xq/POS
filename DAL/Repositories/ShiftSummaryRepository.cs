using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContractShiftSummaryView = Contracts.Shifts.ShiftSummaryView;
using EntityShiftSummaryView = DAL.Entities.ShiftSummaryView;

namespace DAL.Repositories
{
    public class ShiftSummaryRepository : IShiftSummaryRepository
    {
        private readonly IDbContextFactory<PosDbContext> _contextFactory;

        public ShiftSummaryRepository(IDbContextFactory<PosDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ContractShiftSummaryView>> GetLatestShiftSummariesAsync(int take = 5)
        {
            await using var context = _contextFactory.CreateDbContext();

            return await context.Set<EntityShiftSummaryView>()
                .AsNoTracking()
                .OrderByDescending(x => x.OpenTime)
                .Take(take)
                .Select(x => new ContractShiftSummaryView
                {
                    ShiftId = x.ShiftId,
                    CashierName = x.CashierName,
                    OpenTime = x.OpenTime,
                    OpeningCash = x.OpeningCash,
                    CloseTime = x.CloseTime,
                    ClosingCash = x.ClosingCash,
                    ExpectedCash = x.ExpectedCash,
                    CashDifference = x.CashDifference,
                    Status = x.Status.ToString()
                })
                .ToListAsync();
        }
    }
}
