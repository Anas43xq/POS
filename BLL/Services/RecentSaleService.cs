using BLL.Interfaces;
using Contracts.Sales;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class RecentSaleService : IRecentSaleService
    {
        private readonly IRecentSaleRepository _recentSaleRepository;
        private readonly ISessionService _sessionService;

        public RecentSaleService(
            IRecentSaleRepository recentSaleRepository,
            ISessionService sessionService)
        {
            _recentSaleRepository = recentSaleRepository;
            _sessionService = sessionService;
        }

        public async Task<List<RecentTransactionDto>> GetRecentTransactionsByCashierId(int cashierId, int shiftId, int take = 10)
        {
            if (!_sessionService.IsAuthenticated)
                throw new InvalidOperationException("Not logged in");

            if (!string.Equals(_sessionService.CurrentUser?.Role?.RoleName, "Cashier", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only cashier allowed");

            if (_sessionService.CurrentShift?.Status != ShiftStatus.Open)
                throw new InvalidOperationException("Shift closed");

            return await _recentSaleRepository.GetRecentTransactionsByCashierId(cashierId, shiftId, take);
        }
    }
}
