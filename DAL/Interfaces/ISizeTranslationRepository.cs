using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ISizeTranslationRepository : IRepository<SizeTranslation>
    {
        Task<IEnumerable<SizeTranslation>> GetBySizeIdAsync(int sizeId);

        Task<SizeTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<SizeTranslation?> GetByIdAndLanguageCodeAsync(int sizeId, string languageCode);
    }
}
