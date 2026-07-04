using Contracts.Sales;
using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ITopProductRepository
    {
        Task<List<TopProductDto>> GetTopProductsAsync(int take = 7);
    }
}
