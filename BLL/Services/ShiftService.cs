using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _shiftrepo;
        private readonly IPaymentService _paymentService;
        private readonly ITransactionRepository _transactionRepository;

        public ShiftService(IShiftRepository ShiftRepo, IPaymentService paymentService, ITransactionRepository transactionRepository)
        {
            _shiftrepo = ShiftRepo;
            _paymentService = paymentService;
            _transactionRepository = transactionRepository;
        }

        public async Task<IEnumerable<Shift>> GetAllShiftsAsync() =>
            await _shiftrepo.GetAllAsync();

        public async Task<Shift?> GetShiftByIdAsync(int id) =>
            await _shiftrepo.GetByIdAsync(id);

        public async Task AddShiftAsync(Shift Shift) =>
            await _shiftrepo.AddAsync(Shift);

        public async Task UpdateShiftAsync(Shift Shift) =>
            await _shiftrepo.UpdateAsync(Shift);

        public async Task DeleteShiftAsync(int id) =>
            await _shiftrepo.DeleteAsync(id);

        /// <summary>
        /// Gets the current open shift for a user.
        /// </summary>
        public async Task<Result<Shift>> GetOpenShiftAsync(int userId)
        {
            if (userId <= 0)
                return Result<Shift>.Failure("Invalid user ID.");

            Shift? shift = await _shiftrepo.GetOpenShiftAsync(userId);
            if (shift != null)
            {
                return Result<Shift>.Success(shift);
            }
            return Result<Shift>.Failure("No active shift. Please start a shift first.");
        }

        /// <summary>
        /// Opens a new shift for a cashier.
        /// Enforces business rule: only one open shift per cashier.
        /// </summary>
        public async Task<Result<Shift>> OpenShiftAsync(int userId, decimal openingCash)
        {
            try
            {
                if (userId <= 0)
                    return Result<Shift>.Failure("Invalid user ID.");

                if (openingCash < 0)
                    return Result<Shift>.Failure("Opening cash cannot be negative.");

                // Check if user already has an open shift
                var existingOpenShift = await _shiftrepo.GetOpenShiftAsync(userId);
                if (existingOpenShift != null)
                {
                    return Result<Shift>.Failure(
                        $"Cannot start a new shift. An open shift already exists (opened at {existingOpenShift.OpenedAt:g}).");
                }

                var shift = new Shift
                {
                    UserId = userId,
                    OpeningCash = openingCash,
                    OpenedAt = DateTime.Now,
                    Status = ShiftStatus.Open,
                    ClosedAt = null,
                    ClosingCash = null
                };

                await _shiftrepo.AddAsync(shift);
                return Result<Shift>.Success(shift);
            }
            catch (Exception ex)
            {
                return Result<Shift>.Failure($"Error opening shift: {ex.Message}");
            }
        }

        /// <summary>
        /// Closes a shift for a cashier.
        /// Enforces business rules: cannot close already closed shift, calculates cash reconciliation.
        /// </summary>
        public async Task<Result<Shift>> CloseShiftAsync(int shiftId, decimal closingCash)
        {
            try
            {
                if (shiftId <= 0)
                    return Result<Shift>.Failure("Invalid shift ID.");

                if (closingCash < 0)
                    return Result<Shift>.Failure("Closing cash cannot be negative.");

                var shift = await _shiftrepo.GetByIdAsync(shiftId);
                if (shift == null)
                {
                    return Result<Shift>.Failure("Shift not found.");
                }

                // Prevent closing already closed shift
                if (shift.Status == ShiftStatus.Closed)
                {
                    return Result<Shift>.Failure(
                        $"This shift is already closed (closed at {shift.ClosedAt:g}).");
                }

                // Update shift closing details
                shift.ClosingCash = closingCash;
                shift.ClosedAt = DateTime.Now;
                shift.Status = ShiftStatus.Closed;

                // Calculate expected cash: opening cash + sum of cash payments for transactions in this shift
                var payments = (await _paymentService.GetAllPaymentsAsync()).ToList();
                var transactions = (await _transactionRepository.GetAllAsync()).ToList();
                var txIdsInShift = transactions
                    .Where(t => t.ShiftId == shift.ShiftId)
                    .Select(t => t.TransactionId)
                    .ToHashSet();

                var cashTotal = payments
                    .Where(p => string.Equals(p.PaymentMethod, "Cash", StringComparison.OrdinalIgnoreCase)
                                && txIdsInShift.Contains(p.TransactionId))
                    .Sum(p => p.AmountTendered);

                shift.ExpectedCash = shift.OpeningCash + cashTotal;
                shift.CashDifference = closingCash - shift.ExpectedCash;

                await _shiftrepo.UpdateAsync(shift);
                return Result<Shift>.Success(shift);
            }
            catch (Exception ex)
            {
                return Result<Shift>.Failure($"Error closing shift: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Shift>> GetLastShiftsAsync(int count)
        {
            return await _shiftrepo.GetLastShiftsAsync(count);
        }
    }
}