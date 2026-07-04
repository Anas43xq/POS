using BLL.Models;
using DAL.Entities;

namespace BLL.Interfaces
{
    public interface IShiftService
    {
        Task<IEnumerable<Shift>> GetAllShiftsAsync();

        Task<Shift?> GetShiftByIdAsync(int id);

        Task AddShiftAsync(Shift Shift);

        Task UpdateShiftAsync(Shift Shift);

        Task DeleteShiftAsync(int id);

        Task<Result<Shift>> GetOpenShiftAsync(int userId);

        Task<Result<Shift>> OpenShiftAsync(int userId, decimal openingCash);

        Task<Result<Shift>> CloseShiftAsync(int shiftId, decimal closingCash);

        Task<IEnumerable<Shift>> GetLastShiftsAsync(int count);
    }
}