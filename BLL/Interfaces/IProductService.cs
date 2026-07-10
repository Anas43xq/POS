using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface IProductService
    {
        Task<Result<List<ProductSummaryDto>>> GetAllProductsAsync();

        Task<Result<List<ProductDto>>> GetAllVariantsAsync();

        Task<ProductWriteDto?> GetProductByIdAsync(int id);

        Task AddProductAsync(ProductWriteDto Product);

        Task UpdateProductAsync(ProductWriteDto Product);

        Task DeleteProductAsync(int id);
    }
}