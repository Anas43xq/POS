using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICategoryTranslationRepository : IRepository<CategoryTranslation>
    {
        Task<IEnumerable<CategoryTranslation>> GetByCategoryIdAsync(int categoryId);

        Task<CategoryTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<CategoryTranslation?> GetByIdAndLanguageCodeAsync(int categoryId, string languageCode);
    }
}
