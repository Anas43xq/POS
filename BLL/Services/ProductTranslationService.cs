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

        private static ProductTranslationDto MapToDto(ProductTranslation e) => new()
        {
            ProductTranslationId = e.ProductTranslationId,
            ProductId = e.ProductId,
            LanguageCode = e.LanguageCode,
            TranslatedName = e.Name
        };
    }
}