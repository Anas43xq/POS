using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using POS.Contracts.Receipts;

namespace BLL.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(ISupplierRepository supplierRepository, ILogger<SupplierService> logger)
        {
            _supplierRepository = supplierRepository;
            _logger = logger;
        }

        public async Task<Result<List<SupplierDto>>> GetAllAsync()
        {
            try
            {
                var suppliers = await _supplierRepository.GetAllAsync();
                return Result<List<SupplierDto>>.Success(suppliers.Select(MapToDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load suppliers");
                return Result<List<SupplierDto>>.Failure(ex.Message);
            }
        }

        public async Task<Result<SupplierDto?>> GetByIdAsync(int supplierId)
        {
            try
            {
                ValidateSupplierId(supplierId);
                var supplier = await _supplierRepository.GetByIdAsync(supplierId);
                return Result<SupplierDto?>.Success(supplier is null ? null : MapToDto(supplier));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load supplier {SupplierId}", supplierId);
                return Result<SupplierDto?>.Failure(ex.Message);
            }
        }

        public async Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest request)
        {
            try
            {
                ValidateRequest(request);

                var existing = await _supplierRepository.GetByCompanyNameAsync(request.CompanyName.Trim());
                if (existing is not null)
                {
                    return Result<SupplierDto>.Failure("A supplier with this company name already exists.");
                }

                var entity = new Supplier
                {
                    CompanyName = request.CompanyName.Trim(),
                    TRN = request.TRN?.Trim(),
                    Address = request.Address?.Trim(),
                    Phone = request.Phone?.Trim(),
                    Email = request.Email?.Trim(),
                    Notes = request.Notes?.Trim()
                };

                await _supplierRepository.AddAsync(entity);
                return Result<SupplierDto>.Success(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create supplier");
                return Result<SupplierDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<SupplierDto>> UpdateAsync(int supplierId, UpdateSupplierRequest request)
        {
            try
            {
                ValidateSupplierId(supplierId);
                ValidateRequest(request);

                var existing = await _supplierRepository.GetByIdAsync(supplierId);
                if (existing is null)
                {
                    return Result<SupplierDto>.Failure("Supplier not found.");
                }

                existing.CompanyName = request.CompanyName.Trim();
                existing.TRN = request.TRN?.Trim();
                existing.Address = request.Address?.Trim();
                existing.Phone = request.Phone?.Trim();
                existing.Email = request.Email?.Trim();
                existing.Notes = request.Notes?.Trim();

                await _supplierRepository.UpdateAsync(existing);
                return Result<SupplierDto>.Success(MapToDto(existing));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update supplier {SupplierId}", supplierId);
                return Result<SupplierDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> DeleteAsync(int supplierId)
        {
            try
            {
                ValidateSupplierId(supplierId);
                await _supplierRepository.DeleteAsync(supplierId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete supplier {SupplierId}", supplierId);
                return Result<bool>.Failure(ex.Message);
            }
        }

        private static void ValidateSupplierId(int supplierId)
        {
            if (supplierId <= 0)
            {
                throw new InvalidOperationException("Invalid supplier id.");
            }
        }

        private static void ValidateRequest(CreateSupplierRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                throw new InvalidOperationException("Company name is required.");
            }
        }

        private static void ValidateRequest(UpdateSupplierRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                throw new InvalidOperationException("Company name is required.");
            }
        }

        private static SupplierDto MapToDto(Supplier supplier) => new()
        {
            SupplierId = supplier.SupplierId,
            CompanyName = supplier.CompanyName,
            TRN = supplier.TRN,
            Address = supplier.Address,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Notes = supplier.Notes,
            CreatedAt = supplier.CreatedAt
        };
    }
}
