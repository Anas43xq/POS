using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface ITaxRateService
    {
        Task<Result<List<TaxRateDto>>> GetAllTaxRatesAsync();

        Task<Result<TaxRateDto?>> GetTaxRateByIdAsync(int id);

        Task AddTaxRateAsync(TaxRateDto TaxRate);

        Task UpdateTaxRateAsync(TaxRateDto TaxRate);

        Task DeleteTaxRateAsync(int id);
    }
}