using Contracts.Sales;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IRecentTransactionService
    {
        Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int take = 10);
    }
}
