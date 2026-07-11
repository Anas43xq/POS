using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class ProductTranslationService : IProductTranslationService
    {
        private readonly IProductTranslationRepository _repo;

        public ProductTranslationService(IProductTranslationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ProductTranslationDto>> GetByProductIdAsync(int productId)
        {
            var entities = await _repo.GetByProductIdAsync(productId);
            return entities.Select(MapToDto);
        }

        public async Task<ProductTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
        {
            var entity = await _repo.GetByNameAndLanguageCodeAsync(name, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<ProductTranslationDto?> GetByIdAndLanguageCodeAsync(int productId, string languageCode)
        {
            var entity = await _repo.GetByIdAndLanguageCodeAsync(productId, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<ProductTranslationDto>> GetAllByLanguageCodeAsync(string languageCode)
        {
            var entities = await _repo.GetAllByLanguageCodeAsync(languageCode);
            return entities.Select(MapToDto);
        }

        public async Task AddAsync(ProductTranslationDto dto)
        {
            var entity = new ProductTranslation
            {
                ProductId = dto.ProductId,
                LanguageCode = dto.LanguageCode,
                Name = dto.TranslatedName,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(ProductTranslationDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.ProductTranslationId);
            if (existing is null) return;

            existing.Name = dto.TranslatedName;
            existing.LanguageCode = dto.LanguageCode;
            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int translationId)
        {
            await _repo.DeleteAsync(translationId);
        }

        private static ProductTranslationDto MapToDto(ProductTranslation e) => new()
        {
            ProductTranslationId = e.ProductTranslationId,
            ProductId = e.ProductId,
            LanguageCode = e.LanguageCode,
            TranslatedName = e.Name
        };
    }
}