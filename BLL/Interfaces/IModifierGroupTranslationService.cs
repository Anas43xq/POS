using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IModifierGroupTranslationService
    {
        Task<IEnumerable<ModifierGroupTranslationDto>> GetByModifierGroupIdAsync(int modifierGroupId);

        Task<ModifierGroupTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<ModifierGroupTranslationDto?> GetByIdAndLanguageCodeAsync(int modifierGroupId, string languageCode);

        Task<IEnumerable<ModifierGroupTranslationDto>> GetAllByLanguageCodeAsync(string languageCode);

        Task AddAsync(ModifierGroupTranslationDto dto);

        Task UpdateAsync(ModifierGroupTranslationDto dto);

        Task DeleteAsync(int translationId);
    }
}
