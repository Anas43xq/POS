using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class ModifierGroupTranslationService : IModifierGroupTranslationService
    {
        private readonly IModifierGroupTranslationRepository _repo;

        public ModifierGroupTranslationService(IModifierGroupTranslationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ModifierGroupTranslationDto>> GetByModifierGroupIdAsync(int modifierGroupId)
        {
            var entities = await _repo.GetByModifierGroupIdAsync(modifierGroupId);
            return entities.Select(MapToDto);
        }

        public async Task<ModifierGroupTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode)
        {
            var entity = await _repo.GetByNameAndLanguageCodeAsync(name, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<ModifierGroupTranslationDto?> GetByIdAndLanguageCodeAsync(int modifierGroupId, string languageCode)
        {
            var entity = await _repo.GetByIdAndLanguageCodeAsync(modifierGroupId, languageCode);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task<IEnumerable<ModifierGroupTranslationDto>> GetAllByLanguageCodeAsync(string languageCode)
        {
            var entities = await _repo.GetAllByLanguageCodeAsync(languageCode);
            return entities.Select(MapToDto);
        }

        public async Task AddAsync(ModifierGroupTranslationDto dto)
        {
            var entity = new ModifierGroupTranslation
            {
                ModifierGroupId = dto.ModifierGroupId,
                LanguageCode = dto.LanguageCode,
                Name = dto.TranslatedName,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(entity);
        }

        public async Task UpdateAsync(ModifierGroupTranslationDto dto)
        {
            var existing = await _repo.GetByIdAsync(dto.ModifierGroupTranslationId);
            if (existing is null) return;

            existing.Name = dto.TranslatedName;
            existing.LanguageCode = dto.LanguageCode;
            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int translationId)
        {
            await _repo.DeleteAsync(translationId);
        }

        private static ModifierGroupTranslationDto MapToDto(ModifierGroupTranslation e) => new()
        {
            ModifierGroupTranslationId = e.ModifierGroupTranslationId,
            ModifierGroupId = e.ModifierGroupId,
            LanguageCode = e.LanguageCode,
            TranslatedName = e.Name
        };
    }
}
