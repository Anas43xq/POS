using BLL.Models;
using DAL.Entities;

namespace BLL.Interfaces
{
    public interface IProductService
    {
        Task<Result<List<Product>>> GetAllProductsAsync();

        Task<Result<List<Product>>> GetAllProductsWithTaxRateAsync();

        Task<Product?> GetProductByIdAsync(int id);

        Task AddProductAsync(Product Product);

        Task UpdateProductAsync(Product Product);

        Task DeleteProductAsync(int id);
    }
}