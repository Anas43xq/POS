using Contracts.Shifts;
using Contracts.Transactions;

namespace DAL.Interfaces
{
    public interface IShiftManagementRepository
    {
        Task<List<ShiftManagementDto>> GetAllShiftManagementAsync();
        Task<PagedResult<ShiftListItemDto>> GetShiftsListAsync(
            GetShiftsListRequest request, CancellationToken ct = default);
        Task<ShiftDetailDto?> GetShiftDetailAsync(int shiftId, CancellationToken ct = default);
    }
}
