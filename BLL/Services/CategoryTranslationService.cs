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

        public async Task<IEnumerable<CategoryTranslationDto>> GetAllByLanguageCodeAsync(string languageCode)
        {
            var entities = await _repo.GetAllByLanguageCodeAsync(languageCode);
            return entities.Select(MapToDto);
        }

        public async Task AddAsync(CategoryTranslationDto dto)
        {
            var entity = new CategoryTranslation
            {
                CategoryId = dto.CategoryId,
                LanguageCode = dto.LanguageCode,
                Name = dto.TranslatedName,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(CategoryTranslationDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.CategoryTranslationId);
            if (existing is null) return;

            existing.Name = dto.TranslatedName;
            existing.LanguageCode = dto.LanguageCode;
            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int translationId)
        {
            await _repo.DeleteAsync(translationId);
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