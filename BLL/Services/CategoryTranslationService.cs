using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class CategoryTranslationService : ICategoryTranslationService
    {
        private readonly ICategoryTranslationRepository _repo;

        public CategoryTranslationService(ICategoryTranslationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<CategoryTranslationDto>> GetByCategoryIdAsync(int categoryId)
        {
            var entities = await _repo.GetByCategoryIdAsync(categoryId);
            return entities.Select(MapToDto);
        }

        public async Task<CategoryTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
        {
            var entity = await _repo.GetByNameAndLanguageCodeAsync(name, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<CategoryTranslationDto?> GetByIdAndLanguageCodeAsync(int categoryId, string languageCode)
        {
            var entity = await _repo.GetByIdAndLanguageCodeAsync(categoryId, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        private static CategoryTranslationDto MapToDto(CategoryTranslation e) => new()
        {
            CategoryTranslationId = e.CategoryTranslationId,
            CategoryId = e.CategoryId,
            LanguageCode = e.LanguageCode,
            TranslatedName = e.Name
        };
    }
}