using Contracts.Sales;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IRecentTransactionRepository
    {
        Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int take = 10);
    }
}
