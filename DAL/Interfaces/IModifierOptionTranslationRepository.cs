using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IModifierOptionTranslationRepository : IRepository<ModifierOptionTranslation>
    {
        Task<IEnumerable<ModifierOptionTranslation>> GetByModifierOptionIdAsync(int modifierOptionId);

        Task<ModifierOptionTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<ModifierOptionTranslation?> GetByIdAndLanguageCodeAsync(int modifierOptionId, string languageCode);

        Task<IEnumerable<ModifierOptionTranslation>> GetAllByLanguageCodeAsync(string languageCode);
    }
}
