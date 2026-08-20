using Contracts.Shifts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IShiftSummaryRepository
    {
        Task<List<ShiftSummaryView>> GetLatestShiftSummariesAsync(int take = 5);
    }
}
