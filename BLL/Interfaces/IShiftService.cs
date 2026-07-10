using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface IShiftService
    {
        Task<IEnumerable<ShiftDto>> GetAllShiftsAsync();

        Task<ShiftDto?> GetShiftByIdAsync(int id);

        Task AddShiftAsync(ShiftDto Shift);

        Task UpdateShiftAsync(ShiftDto Shift);

        Task DeleteShiftAsync(int id);

        Task<Result<ShiftDto>> GetOpenShiftAsync(int userId);

        Task<Result<ShiftDto>> OpenShiftAsync(int userId, decimal openingCash);

        Task<Result<ShiftDto>> CloseShiftAsync(int shiftId, decimal closingCash);

        Task<IEnumerable<ShiftDto>> GetLastShiftsAsync(int count);
    }
}