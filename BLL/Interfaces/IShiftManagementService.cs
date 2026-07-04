using Contracts.Shifts;

namespace BLL.Interfaces
{
    public interface IShiftManagementService
    {
        Task<List<ShiftManagementDto>> GetAllShiftManagementAsync();
    }
}
