using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IModifierOptionTranslationService
    {
        Task<IEnumerable<ModifierOptionTranslationDto>> GetByModifierOptionIdAsync(int modifierOptionId);

        Task<ModifierOptionTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<ModifierOptionTranslationDto?> GetByIdAndLanguageCodeAsync(int modifierOptionId, string languageCode);

        Task<IEnumerable<ModifierOptionTranslationDto>> GetAllByLanguageCodeAsync(string languageCode);

        Task AddAsync(ModifierOptionTranslationDto dto);

        Task UpdateAsync(ModifierOptionTranslationDto dto);

        Task DeleteAsync(int translationId);
    }
}
