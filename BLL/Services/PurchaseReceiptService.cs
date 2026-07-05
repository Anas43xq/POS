using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using POS.Contracts.Receipts;

namespace BLL.Services
{
    public class PurchaseReceiptService : IPurchaseReceiptService
    {
        private readonly IPurchaseReceiptRepository _purchaseReceiptRepository;
        private readonly ILogger<PurchaseReceiptService> _logger;

        public PurchaseReceiptService(IPurchaseReceiptRepository purchaseReceiptRepository, ILogger<PurchaseReceiptService> logger)
        {
            _purchaseReceiptRepository = purchaseReceiptRepository;
            _logger = logger;
        }

        public async Task<Result<List<PurchaseReceiptDto>>> GetAllAsync(PurchaseReceiptSearchRequest? request = null)
        {
            try
            {
                var receipts = await _purchaseReceiptRepository.GetAllAsync(request);
                return Result<List<PurchaseReceiptDto>>.Success(receipts.Select(MapToDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load purchase receipts");
                return Result<List<PurchaseReceiptDto>>.Failure(ex.Message);
            }
        }

        public async Task<Result<PurchaseReceiptDto?>> GetByIdAsync(int receiptId)
        {
            try
            {
                ValidateReceiptId(receiptId);
                var receipt = await _purchaseReceiptRepository.GetByIdAsync(receiptId);
                return Result<PurchaseReceiptDto?>.Success(receipt is null ? null : MapToDto(receipt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load purchase receipt {ReceiptId}", receiptId);
                return Result<PurchaseReceiptDto?>.Failure(ex.Message);
            }
        }

        public async Task<Result<PurchaseReceiptDto>> CreateAsync(CreatePurchaseReceiptRequest request)
        {
            try
            {
                ValidateRequest(request);

                var entity = new PurchaseReceipt
                {
                    ReceiptTypeId = request.ReceiptTypeId,
                    SupplierId = request.SupplierId,
                    InvoiceNumber = request.InvoiceNumber.Trim(),
                    InvoiceDate = request.InvoiceDate,
                    Category = request.Category.Trim(),
                    Description = request.Description?.Trim(),
                    Subtotal = request.Subtotal,
                    VatRate = request.VatRate,
                    VatAmount = request.VatAmount,
                    GrandTotal = request.GrandTotal,
                    Notes = request.Notes?.Trim(),
                    ImagePath = request.ImagePath?.Trim(),
                    CreatedBy = request.CreatedBy
                };

                await _purchaseReceiptRepository.AddAsync(entity);
                return Result<PurchaseReceiptDto>.Success(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create purchase receipt");
                return Result<PurchaseReceiptDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<PurchaseReceiptDto>> UpdateAsync(int receiptId, UpdatePurchaseReceiptRequest request)
        {
            try
            {
                ValidateReceiptId(receiptId);
                ValidateRequest(request);

                var existing = await _purchaseReceiptRepository.GetByIdAsync(receiptId);
                if (existing is null)
                {
                    return Result<PurchaseReceiptDto>.Failure("Purchase receipt not found.");
                }

                existing.ReceiptTypeId = request.ReceiptTypeId;
                existing.SupplierId = request.SupplierId;
                existing.InvoiceNumber = request.InvoiceNumber.Trim();
                existing.InvoiceDate = request.InvoiceDate;
                existing.Category = request.Category.Trim();
                existing.Description = request.Description?.Trim();
                existing.Subtotal = request.Subtotal;
                existing.VatRate = request.VatRate;
                existing.VatAmount = request.VatAmount;
                existing.GrandTotal = request.GrandTotal;
                existing.Notes = request.Notes?.Trim();
                existing.ImagePath = request.ImagePath?.Trim();

                await _purchaseReceiptRepository.UpdateAsync(existing);
                return Result<PurchaseReceiptDto>.Success(MapToDto(existing));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update purchase receipt {ReceiptId}", receiptId);
                return Result<PurchaseReceiptDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<bool>> DeleteAsync(int receiptId)
        {
            try
            {
                ValidateReceiptId(receiptId);
                await _purchaseReceiptRepository.DeleteAsync(receiptId);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete purchase receipt {ReceiptId}", receiptId);
                return Result<bool>.Failure(ex.Message);
            }
        }

        private static void ValidateReceiptId(int receiptId)
        {
            if (receiptId <= 0)
            {
                throw new InvalidOperationException("Invalid receipt id.");
            }
        }

        private static void ValidateRequest(CreatePurchaseReceiptRequest request)
        {
            if (request.ReceiptTypeId == 0)
                throw new InvalidOperationException("Receipt type is required.");

            if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
                throw new InvalidOperationException("Invoice number is required.");

            if (string.IsNullOrWhiteSpace(request.Category))
                throw new InvalidOperationException("Category is required.");

            if (request.CreatedBy <= 0)
                throw new InvalidOperationException("Created by is required.");
        }

        private static void ValidateRequest(UpdatePurchaseReceiptRequest request)
        {
            if (request.ReceiptTypeId == 0)
                throw new InvalidOperationException("Receipt type is required.");

            if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
                throw new InvalidOperationException("Invoice number is required.");

            if (string.IsNullOrWhiteSpace(request.Category))
                throw new InvalidOperationException("Category is required.");
        }

        private static PurchaseReceiptDto MapToDto(PurchaseReceipt receipt) => new()
        {
            ReceiptId = receipt.ReceiptId,
            ReceiptTypeId = receipt.ReceiptTypeId,
            ReceiptTypeName = receipt.ReceiptType?.Name ?? string.Empty,
            SupplierId = receipt.SupplierId,
            SupplierName = receipt.Supplier?.CompanyName,
            InvoiceNumber = receipt.InvoiceNumber,
            InvoiceDate = receipt.InvoiceDate,
            Category = receipt.Category,
            Description = receipt.Description,
            Subtotal = receipt.Subtotal,
            VatRate = receipt.VatRate,
            VatAmount = receipt.VatAmount,
            GrandTotal = receipt.GrandTotal,
            Notes = receipt.Notes,
            ImagePath = receipt.ImagePath,
            CreatedBy = receipt.CreatedBy,
            CreatedByName = receipt.CreatedByUser?.FullName,
            CreatedAt = receipt.CreatedAt
        };
    }
}
