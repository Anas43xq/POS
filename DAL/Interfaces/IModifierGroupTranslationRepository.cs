using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IModifierGroupTranslationRepository : IRepository<ModifierGroupTranslation>
    {
        Task<IEnumerable<ModifierGroupTranslation>> GetByModifierGroupIdAsync(int modifierGroupId);

        Task<ModifierGroupTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<ModifierGroupTranslation?> GetByIdAndLanguageCodeAsync(int modifierGroupId, string languageCode);

        Task<IEnumerable<ModifierGroupTranslation>> GetAllByLanguageCodeAsync(string languageCode);
    }
}
