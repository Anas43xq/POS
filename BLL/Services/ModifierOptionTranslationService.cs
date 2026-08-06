using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class ModifierOptionTranslationService : IModifierOptionTranslationService
    {
        private readonly IModifierOptionTranslationRepository _repo;

        public ModifierOptionTranslationService(IModifierOptionTranslationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ModifierOptionTranslationDto>> GetByModifierOptionIdAsync(int modifierOptionId)
        {
            var entities = await _repo.GetByModifierOptionIdAsync(modifierOptionId);
            return entities.Select(MapToDto);
        }

        public async Task<ModifierOptionTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
        {
            var entity = await _repo.GetByNameAndLanguageCodeAsync(name, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<ModifierOptionTranslationDto?> GetByIdAndLanguageCodeAsync(int modifierOptionId, string languageCode)
        {
            var entity = await _repo.GetByIdAndLanguageCodeAsync(modifierOptionId, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<ModifierOptionTranslationDto>> GetAllByLanguageCodeAsync(string languageCode)
        {
            var entities = await _repo.GetAllByLanguageCodeAsync(languageCode);
            return entities.Select(MapToDto);
        }

        public async Task AddAsync(ModifierOptionTranslationDto dto)
        {
            var entity = new ModifierOptionTranslation
            {
                ModifierOptionId = dto.ModifierOptionId,
                LanguageCode = dto.LanguageCode,
                Name = dto.TranslatedName,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(ModifierOptionTranslationDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.ModifierOptionTranslationId);
            if (existing is null) return;

            existing.Name = dto.TranslatedName;
            existing.LanguageCode = dto.LanguageCode;
            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int translationId)
        {
            await _repo.DeleteAsync(translationId);
        }

        private static ModifierOptionTranslationDto MapToDto(ModifierOptionTranslation e) => new()
        {
            ModifierOptionTranslationId = e.ModifierOptionTranslationId,
            ModifierOptionId = e.ModifierOptionId,
            LanguageCode = e.LanguageCode,
            TranslatedName = e.Name
        };
    }
}
