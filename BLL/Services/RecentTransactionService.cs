using BLL.Interfaces;
using Contracts.Sales;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class RecentTransactionService : IRecentTransactionService
    {
        private readonly IRecentTransactionRepository _recentTransactionRepository;
        private readonly ISessionService _sessionService;

        public RecentTransactionService(
            IRecentTransactionRepository recentTransactionRepository,
            ISessionService sessionService)
        {
            _recentTransactionRepository = recentTransactionRepository;
            _sessionService = sessionService;
        }

        public async Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int take = 10)
        {
            if (!_sessionService.IsAuthenticated)
                throw new InvalidOperationException("Not logged in.");

            if (!string.Equals(_sessionService.CurrentUser?.RoleName, "Manager", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only managers allowed.");

            return await _recentTransactionRepository.GetRecentTransactionsAsync(take);
        }
    }
}
