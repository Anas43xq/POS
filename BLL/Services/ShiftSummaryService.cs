using BLL.Interfaces;
using Contracts.Shifts;
using DAL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class ShiftSummaryService : IShiftSummaryService
    {
        private readonly IShiftSummaryRepository _repository;

        public ShiftSummaryService(IShiftSummaryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ShiftSummaryView>> GetLatestShiftSummariesAsync(int take = 5)
        {
            return await _repository.GetLatestShiftSummariesAsync(take);
        }
    }
}
