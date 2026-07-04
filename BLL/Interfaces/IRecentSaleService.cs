using Contracts.Sales;

namespace BLL.Interfaces
{
    public interface IRecentSaleService
    {
        public Task<List<RecentTransactionDto>> GetRecentTransactionsByCashierId(int cashierId, int shiftId, int take = 10);
    }
}