using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _shiftrepo;
        private readonly IPaymentService _paymentService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<ShiftService> _logger;

        public ShiftService(
            IShiftRepository ShiftRepo,
            IPaymentService paymentService,
            ITransactionRepository transactionRepository,
            ILogger<ShiftService> logger)
        {
            _shiftrepo = ShiftRepo;
            _paymentService = paymentService;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<ShiftDto>> GetAllShiftsAsync()
        {
            var entities = await _shiftrepo.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<ShiftDto?> GetShiftByIdAsync(int id)
        {
            var entity = await _shiftrepo.GetByIdAsync(id);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task AddShiftAsync(ShiftDto shift) =>
            await _shiftrepo.AddAsync(MapToEntity(shift));

        public async Task UpdateShiftAsync(ShiftDto shift) =>
            await _shiftrepo.UpdateAsync(MapToEntity(shift));

        public async Task DeleteShiftAsync(int id) =>
            await _shiftrepo.DeleteAsync(id);

        public async Task<Result<ShiftDto>> GetOpenShiftAsync(int userId)
        {
            if (userId <= 0)
                return Result<ShiftDto>.Failure("Invalid user ID.");

            Shift? shift = await _shiftrepo.GetOpenShiftAsync(userId);
            if (shift != null)
            {
                return Result<ShiftDto>.Success(MapToDto(shift));
            }
            return Result<ShiftDto>.Failure("No active shift. Please start a shift first.");
        }

        public async Task<Result<ShiftDto>> OpenShiftAsync(int userId, decimal openingCash)
        {
            try
            {
                if (userId <= 0)
                    return Result<ShiftDto>.Failure("Invalid user ID.");

                if (openingCash < 0)
                    return Result<ShiftDto>.Failure("Opening cash cannot be negative.");

                var existingOpenShift = await _shiftrepo.GetOpenShiftAsync(userId);
                if (existingOpenShift != null)
                {
                    return Result<ShiftDto>.Failure(
                        $"Cannot start a new shift. An open shift already exists (opened at {existingOpenShift.OpenedAt:g}).");
                }

                var shift = new Shift
                {
                    UserId = userId,
                    OpeningCash = openingCash,
                    OpenedAt = DateTime.Now,
                    Status = DAL.Entities.ShiftStatus.Open,
                    ClosedAt = null,
                    ClosingCash = null
                };

                await _shiftrepo.AddAsync(shift);
                return Result<ShiftDto>.Success(MapToDto(shift));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open shift for user {UserId}", userId);
                return Result<ShiftDto>.Failure($"Error opening shift: {ex.Message}");
            }
        }

        public async Task<Result<ShiftDto>> CloseShiftAsync(int shiftId, decimal closingCash)
        {
            try
            {
                if (shiftId <= 0)
                    return Result<ShiftDto>.Failure("Invalid shift ID.");

                if (closingCash < 0)
                    return Result<ShiftDto>.Failure("Closing cash cannot be negative.");

                var shift = await _shiftrepo.GetByIdAsync(shiftId);
                if (shift == null)
                {
                    return Result<ShiftDto>.Failure("Shift not found.");
                }

                if (shift.Status == DAL.Entities.ShiftStatus.Closed)
                {
                    return Result<ShiftDto>.Failure(
                        $"This shift is already closed (closed at {shift.ClosedAt:g}).");
                }

                shift.ClosingCash = closingCash;
                shift.ClosedAt = DateTime.Now;
                shift.Status = DAL.Entities.ShiftStatus.Closed;

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
                return Result<ShiftDto>.Success(MapToDto(shift));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to close shift {ShiftId}", shiftId);
                return Result<ShiftDto>.Failure($"Error closing shift: {ex.Message}");
            }
        }

        public async Task<IEnumerable<ShiftDto>> GetLastShiftsAsync(int count)
        {
            var entities = await _shiftrepo.GetLastShiftsAsync(count);
            return entities.Select(MapToDto);
        }

        private static ShiftDto MapToDto(Shift e) => new()
        {
            ShiftId = e.ShiftId,
            UserId = e.UserId,
            OpenedAt = e.OpenedAt,
            ClosedAt = e.ClosedAt,
            OpeningCash = e.OpeningCash,
            ClosingCash = e.ClosingCash,
            ExpectedCash = e.ExpectedCash,
            CashDifference = e.CashDifference,
            Status = (Contracts.Enum.ShiftStatus)(byte)e.Status
        };

        private static Shift MapToEntity(ShiftDto d) => new()
        {
            ShiftId = d.ShiftId,
            UserId = d.UserId,
            OpenedAt = d.OpenedAt,
            ClosedAt = d.ClosedAt,
            OpeningCash = d.OpeningCash,
            ClosingCash = d.ClosingCash,
            ExpectedCash = d.ExpectedCash,
            CashDifference = d.CashDifference,
            Status = (DAL.Entities.ShiftStatus)(byte)d.Status
        };
    }
}