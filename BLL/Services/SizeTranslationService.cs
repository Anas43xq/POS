using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class SizeTranslationService : ISizeTranslationService
    {
        private readonly ISizeTranslationRepository _repo;

        public SizeTranslationService(ISizeTranslationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<SizeTranslationDto>> GetBySizeIdAsync(int sizeId)
        {
            var entities = await _repo.GetBySizeIdAsync(sizeId);
            return entities.Select(MapToDto);
        }

        public async Task<SizeTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
        {
            var entity = await _repo.GetByNameAndLanguageCodeAsync(name, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<SizeTranslationDto?> GetByIdAndLanguageCodeAsync(int sizeId, string languageCode)
        {
            var entity = await _repo.GetByIdAndLanguageCodeAsync(sizeId, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<SizeTranslationDto>> GetAllByLanguageCodeAsync(string languageCode)
        {
            var entities = await _repo.GetAllByLanguageCodeAsync(languageCode);
            return entities.Select(MapToDto);
        }

        public async Task AddAsync(SizeTranslationDto dto)
        {
            var entity = new SizeTranslation
            {
                SizeId = dto.SizeId,
                LanguageCode = dto.LanguageCode,
                Name = dto.TranslatedName,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(SizeTranslationDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.SizeTranslationId);
            if (existing is null) return;

            existing.Name = dto.TranslatedName;
            existing.LanguageCode = dto.LanguageCode;
            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int translationId)
        {
            await _repo.DeleteAsync(translationId);
        }

        private static SizeTranslationDto MapToDto(SizeTranslation e) => new()
        {
            SizeTranslationId = e.SizeTranslationId,
            SizeId = e.SizeId,
            LanguageCode = e.LanguageCode,
            TranslatedName = e.Name
        };
    }
}