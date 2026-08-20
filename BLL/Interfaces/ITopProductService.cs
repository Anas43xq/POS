using Contracts.Sales;
using Contracts.Transactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ITopProductService
    {
        Task<List<TopProductDto>> GetTopProductsAsync(int take = 7);
        Task<List<TopProductDto>> GetTopProductsAsync(GetTransactionKpisRequest request, int take = 7);
    }
}
