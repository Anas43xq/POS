using BLL.Interfaces;
using Contracts.Sales;
using Contracts.Transactions;
using DAL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class TopProductService : ITopProductService
    {
        private readonly ITopProductRepository _topProductRepository;

        public TopProductService(ITopProductRepository topProductRepository)
        {
            _topProductRepository = topProductRepository;
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(int take = 7)
        {
            return await _topProductRepository.GetTopProductsAsync(take);
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(GetTransactionKpisRequest request, int take = 7)
        {
            return await _topProductRepository.GetTopProductsAsync(request, take);
        }
    }
}
