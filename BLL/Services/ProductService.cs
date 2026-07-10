using BLL.DTOs;
using BLL.Helpers;
using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productrepo;

        public ProductService(IProductRepository ProductRepo)
        {
            _productrepo = ProductRepo;
        }

        public async Task<Result<List<ProductSummaryDto>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productrepo.GetAllProductsWithTaxRateAsync();
                return Result<List<ProductSummaryDto>>.Success(
                    products.Select(p => new ProductSummaryDto
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        CategoryId = p.CategoryId,
                        UnitPrice = p.UnitPrice,
                        TaxRateId = p.TaxRateId,
                        TaxRateName = p.TaxRate?.Name ?? string.Empty,
                        IsActive = p.IsActive
                    }).ToList());
            }
            catch (Exception ex)
            {
                return Result<List<ProductSummaryDto>>.Failure(ex.Message);
            }
        }

        public async Task<Result<List<ProductDto>>> GetAllVariantsAsync()
        {
            try
            {
                var variants = await _productrepo.GetAllVariantsAsync();

                if (variants is null || !variants.Any())
                    return Result<List<ProductDto>>.Success([]);

                var result = variants.Select(v => new ProductDto
                {
                    ProductId = v.ProductId,
                    VariantId = v.VariantId,
                    Name = v.Product?.Name ?? string.Empty,
                    DisplayName = ProductNameFormatter.Format(
                        v.Product?.Name ?? string.Empty,
                        v.Size?.Name),
                    UnitPrice = v.UnitPrice,
                    TaxRate = v.Product?.TaxRate?.Rate ?? 0,
                    CategoryId = v.Product?.CategoryId ?? 0,
                    IsActive = v.IsActive
                }).ToList();

                return Result<List<ProductDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<ProductDto>>.Failure(ex.Message);
            }
        }

        public async Task<ProductWriteDto?> GetProductByIdAsync(int id)
        {
            var entity = await _productrepo.GetByIdAsync(id);
            return entity is null ? null : new ProductWriteDto
            {
                ProductId = entity.ProductId,
                Name = entity.Name,
                CategoryId = entity.CategoryId,
                UnitPrice = entity.UnitPrice,
                TaxRateId = entity.TaxRateId,
                IsActive = entity.IsActive,
                Description = entity.Description
            };
        }

        public async Task AddProductAsync(ProductWriteDto product) =>
            await _productrepo.AddAsync(MapToEntity(product));

        public async Task UpdateProductAsync(ProductWriteDto product) =>
            await _productrepo.UpdateAsync(MapToEntity(product));

        public async Task DeleteProductAsync(int id) =>
            await _productrepo.DeleteAsync(id);

        private static Product MapToEntity(ProductWriteDto d) => new()
        {
            ProductId = d.ProductId,
            Name = d.Name,
            CategoryId = d.CategoryId,
            UnitPrice = d.UnitPrice,
            TaxRateId = d.TaxRateId,
            IsActive = d.IsActive,
            Description = d.Description
        };
    }
}