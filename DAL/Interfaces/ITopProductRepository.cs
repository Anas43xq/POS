using Contracts.Sales;
using Contracts.Transactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ITopProductRepository
    {
        Task<List<TopProductDto>> GetTopProductsAsync(int take = 7);
        Task<List<TopProductDto>> GetTopProductsAsync(GetTransactionKpisRequest request, int take = 7);
    }
}
