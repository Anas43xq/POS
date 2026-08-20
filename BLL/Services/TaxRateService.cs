using BLL.DTOs;
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

        public async Task<Result<List<TaxRateDto>>> GetAllTaxRatesAsync()
        {
            var taxRates = await _taxraterepo.GetAllAsync();
            return Result<List<TaxRateDto>>.Success(taxRates.Select(MapToDto).ToList());
        }

        public async Task<Result<TaxRateDto?>> GetTaxRateByIdAsync(int id)
        {
            var taxRate = await _taxraterepo.GetByIdAsync(id);
            return Result<TaxRateDto?>.Success(taxRate is null ? null : MapToDto(taxRate));
        }

        public async Task AddTaxRateAsync(TaxRateDto TaxRate) =>
            await _taxraterepo.AddAsync(MapToEntity(TaxRate));

        public async Task UpdateTaxRateAsync(TaxRateDto TaxRate) =>
            await _taxraterepo.UpdateAsync(MapToEntity(TaxRate));

        public async Task DeleteTaxRateAsync(int id) =>
            await _taxraterepo.DeleteAsync(id);

        private static TaxRateDto MapToDto(TaxRate e) => new()
        {
            TaxRateId = e.TaxRateId,
            Name = e.Name,
            Rate = e.Rate
        };

        private static TaxRate MapToEntity(TaxRateDto d) => new()
        {
            TaxRateId = d.TaxRateId,
            Name = d.Name,
            Rate = d.Rate
        };
    }
}