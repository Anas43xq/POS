using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IProductTranslationService
    {
        Task<IEnumerable<ProductTranslationDto>> GetByProductIdAsync(int productId);

        Task<ProductTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<ProductTranslationDto?> GetByIdAndLanguageCodeAsync(int productId, string languageCode);

        Task<IEnumerable<ProductTranslationDto>> GetAllByLanguageCodeAsync(string languageCode);
    }
}