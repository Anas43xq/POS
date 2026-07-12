using BLL.Interfaces;
using BLL.Models;
using Contracts.Shifts;
using Contracts.Transactions;
using DAL.Interfaces;

namespace BLL.Services
{
    public class ShiftManagementService : IShiftManagementService
    {
        private readonly IShiftManagementRepository _repository;

        public ShiftManagementService(IShiftManagementRepository repository)
        {
            _repository = repository;
        }

        public Task<List<ShiftManagementDto>> GetAllShiftManagementAsync()
        {
            return _repository.GetAllShiftManagementAsync();
        }

        public async Task<Result<PagedResult<ShiftListItemDto>>> GetShiftsListAsync(
            GetShiftsListRequest request, CancellationToken ct = default)
        {
            if (request == null)
                return Result<PagedResult<ShiftListItemDto>>.Failure("Request cannot be null.");

            var result = await _repository.GetShiftsListAsync(request, ct);
            return Result<PagedResult<ShiftListItemDto>>.Success(result);
        }

        public async Task<Result<ShiftDetailDto>> GetShiftDetailAsync(int shiftId, CancellationToken ct = default)
        {
            if (shiftId <= 0)
                return Result<ShiftDetailDto>.Failure("Invalid shift id.");

            var result = await _repository.GetShiftDetailAsync(shiftId, ct);

            if (result == null)
                return Result<ShiftDetailDto>.Failure("Shift not found.");

            return Result<ShiftDetailDto>.Success(result);
        }
    }
}
