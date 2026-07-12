using BLL.Models;
using Contracts.Shifts;
using Contracts.Transactions;

namespace BLL.Interfaces
{
    public interface IShiftManagementService
    {
        Task<List<ShiftManagementDto>> GetAllShiftManagementAsync();
        Task<Result<PagedResult<ShiftListItemDto>>> GetShiftsListAsync(
            GetShiftsListRequest request, CancellationToken ct = default);
        Task<Result<ShiftDetailDto>> GetShiftDetailAsync(int shiftId, CancellationToken ct = default);
    }
}
