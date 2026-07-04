using BLL.Interfaces;
using Contracts.Shifts;
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
    }
}
