using Contracts.Sales;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ITopProductService
    {
        Task<List<TopProductDto>> GetTopProductsAsync(int take = 7);
    }
}
