using Contracts.Shifts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IShiftSummaryService
    {
        Task<List<ShiftSummaryView>> GetLatestShiftSummariesAsync(int take = 5);
    }
}
