using Contracts.Shifts;
using DAL.Entities;

namespace DAL.Interfaces
{
    public interface IShiftManagementRepository
    {
        Task<List<ShiftManagementDto>> GetAllShiftManagementAsync();
    }
}
