using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class TaxRateService : ITaxRateService
    {
        private readonly ITaxRateRepository _taxraterepo;

        public TaxRateService(ITaxRateRepository TaxRateRepo)
        {
            _taxraterepo = TaxRateRepo;
        }

        public async Task<Result<List<TaxRate>>> GetAllTaxRatesAsync()
        {
            var taxRates = await _taxraterepo.GetAllAsync();
            return Result<List<TaxRate>>.Success(taxRates.ToList());
        }

        public async Task<Result<TaxRate?>> GetTaxRateByIdAsync(int id)
        {
            var taxRate = await _taxraterepo.GetByIdAsync(id);
            return Result<TaxRate?>.Success(taxRate);
        }

        public async Task AddTaxRateAsync(TaxRate TaxRate) =>
            await _taxraterepo.AddAsync(TaxRate);

        public async Task UpdateTaxRateAsync(TaxRate TaxRate) =>
            await _taxraterepo.UpdateAsync(TaxRate);

        public async Task DeleteTaxRateAsync(int id) =>
            await _taxraterepo.DeleteAsync(id);
    }
}
