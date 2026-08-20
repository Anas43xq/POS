using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface IProductService
    {
        Task<Result<List<ProductSummaryDto>>> GetAllProductsAsync();

        Task<Result<List<ProductSummaryDto>>> GetAllProductsAsync(string languageCode);

        Task<Result<List<ProductSummaryDto>>> GetProductSummariesAsync(string? languageCode = null);

        Task<Result<List<ProductDto>>> GetAllVariantsAsync();

        Task<Result<List<ProductDto>>> GetAllVariantsAsync(string languageCode);

        /// <summary>
        /// Product → Variants projection for API consumers: one entry per
        /// product with its active Size/Price variants nested underneath.
        /// </summary>
        Task<Result<List<ProductWithVariantsDto>>> GetAllProductsWithVariantsAsync();

        Task<ProductWriteDto?> GetProductByIdAsync(int id);

        Task AddProductAsync(ProductWriteDto Product);

        /// <summary>
        /// Saves the product and reconciles its ProductVariants against
        /// <paramref name="Product"/>.Variants. Returns the raw size name
        /// of each variant that couldn't be removed because it has
        /// historical sales — those are deactivated instead of deleted.
        /// This is raw data, not a presentation string: the caller is
        /// responsible for formatting/localizing it for display.
        /// </summary>
        Task<List<string>> UpdateProductAsync(ProductWriteDto Product);

        Task DeleteProductAsync(int id);
    }
}