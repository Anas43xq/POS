using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ISizeTranslationService
    {
        Task<IEnumerable<SizeTranslationDto>> GetBySizeIdAsync(int sizeId);

        Task<SizeTranslationDto?> GetByNameAndLanguageCodeAsync(string name, string languageCode);

        Task<SizeTranslationDto?> GetByIdAndLanguageCodeAsync(int sizeId, string languageCode);
    }
}