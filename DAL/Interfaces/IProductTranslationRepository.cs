using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IProductTranslationRepository : IRepository<ProductTranslation>
    {
        Task<IEnumerable<ProductTranslation>> GetByProductIdAsync(int productId);

        Task<ProductTranslation?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<ProductTranslation?> GetByIdAndLanguageCodeAsync(int productId, string languageCode);
    }
}
