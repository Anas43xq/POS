using DAL.Entities;

namespace DAL.Interfaces
{
    public interface IShiftRepository : IRepository<Shift>
    {
        Task<Shift?> GetOpenShiftAsync(int userId);
        Task<IEnumerable<Shift>> GetLastShiftsAsync(int count);
        Task<decimal> GetShiftTotalSalesAsync(int shiftId, CancellationToken ct = default);
        Task<decimal> GetShiftCashTotalAsync(int shiftId, CancellationToken ct = default);

        /// <summary>
        /// Inserts a new shift, relying on the DB-level filtered unique
        /// index (UX_Shifts_OpenShift_User: UserId WHERE Status = 1) as
        /// the actual authority against two open shifts for the same
        /// user. The in-memory check in ShiftService is a fast/friendly
        /// pre-check only; this is the backstop for the case where two
        /// concurrent open-shift attempts (e.g. two terminals) both pass
        /// that pre-check before either has inserted. Throws
        /// <see cref="InvalidOperationException"/> with a user-facing
        /// message if the index rejects the insert.
        /// </summary>
        Task AddOpenShiftAsync(Shift shift);
    }
}
