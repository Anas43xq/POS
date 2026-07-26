using DAL.Entities;

namespace DAL.Interfaces
{
    public interface IShiftRepository : IRepository<Shift>
    {
        Task<Shift?> GetOpenShiftAsync(int userId);
        Task<IEnumerable<Shift>> GetLastShiftsAsync(int count);
        Task<decimal> GetShiftTotalSalesAsync(int shiftId, CancellationToken ct = default);
    }
}