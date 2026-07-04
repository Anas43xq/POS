using BLL.Models;
using DAL.Entities;

namespace BLL.Interfaces
{
    public interface ITaxRateService
    {
        Task<Result<List<TaxRate>>> GetAllTaxRatesAsync();

        Task<Result<TaxRate?>> GetTaxRateByIdAsync(int id);

        Task AddTaxRateAsync(TaxRate TaxRate);

        Task UpdateTaxRateAsync(TaxRate TaxRate);

        Task DeleteTaxRateAsync(int id);
    }
}
