using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ICategoryTranslationService
    {
        Task<IEnumerable<CategoryTranslationDto>> GetByCategoryIdAsync(int categoryId);

        Task<CategoryTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<CategoryTranslationDto?> GetByIdAndLanguageCodeAsync(int categoryId, string languageCode);

        Task<IEnumerable<CategoryTranslationDto>> GetAllByLanguageCodeAsync(string languageCode);

        Task AddAsync(CategoryTranslationDto dto);

        Task UpdateAsync(CategoryTranslationDto dto);

        Task DeleteAsync(int translationId);
    }
}