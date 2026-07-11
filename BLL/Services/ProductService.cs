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
        private readonly IProductTranslationService _productTranslationService;
        private readonly ISizeTranslationService _sizeTranslationService;

        public ProductService(
            IProductRepository ProductRepo,
            IProductTranslationService productTranslationService,
            ISizeTranslationService sizeTranslationService)
        {
            _productrepo = ProductRepo;
            _productTranslationService = productTranslationService;
            _sizeTranslationService = sizeTranslationService;
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

        public async Task<Result<List<ProductSummaryDto>>> GetAllProductsAsync(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode) || languageCode == "en")
                return await GetAllProductsAsync();

            try
            {
                var products = await _productrepo.GetAllProductsWithTaxRateAsync();

                // Batch-load all product translations for the requested language (avoids N+1).
                var translations = (await _productTranslationService
                    .GetAllByLanguageCodeAsync(languageCode))
                    .ToDictionary(t => t.ProductId, t => t.TranslatedName);

                return Result<List<ProductSummaryDto>>.Success(
                    products.Select(p =>
                    {
                        var englishName = p.Name;
                        var localizedName = translations.TryGetValue(p.ProductId, out var tName)
                            && !string.IsNullOrWhiteSpace(tName)
                            ? tName
                            : englishName;

                        return new ProductSummaryDto
                        {
                            ProductId = p.ProductId,
                            Name = localizedName,
                            CategoryId = p.CategoryId,
                            UnitPrice = p.UnitPrice,
                            TaxRateId = p.TaxRateId,
                            TaxRateName = p.TaxRate?.Name ?? string.Empty,
                            IsActive = p.IsActive
                        };
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

                var result = variants.Select(v =>
                {
                    var productName = v.Product?.Name ?? string.Empty;
                    var sizeName = v.Size?.Name;
                    var displayName = ProductNameFormatter.Format(productName, sizeName);
                    return new ProductDto
                    {
                        ProductId = v.ProductId,
                        VariantId = v.VariantId,
                        Name = productName,
                        DisplayName = displayName,
                        EnglishName = productName,
                        EnglishDisplayName = displayName,
                        UnitPrice = v.UnitPrice,
                        TaxRate = v.Product?.TaxRate?.Rate ?? 0,
                        CategoryId = v.Product?.CategoryId ?? 0,
                        IsActive = v.IsActive
                    };
                }).ToList();

                return Result<List<ProductDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<ProductDto>>.Failure(ex.Message);
            }
        }

        public async Task<Result<List<ProductDto>>> GetAllVariantsAsync(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode) || languageCode == "en")
                return await GetAllVariantsAsync();

            try
            {
                var variants = await _productrepo.GetAllVariantsAsync();

                if (variants is null || !variants.Any())
                    return Result<List<ProductDto>>.Success([]);

                // Batch-load all translations for the requested language (avoids N+1).
                var productTranslations = (await _productTranslationService
                    .GetAllByLanguageCodeAsync(languageCode))
                    .ToDictionary(t => t.ProductId, t => t.TranslatedName);

                var sizeTranslations = (await _sizeTranslationService
                    .GetAllByLanguageCodeAsync(languageCode))
                    .ToDictionary(t => t.SizeId, t => t.TranslatedName);

                var result = variants.Select(v =>
                {
                    var englishProductName = v.Product?.Name ?? string.Empty;
                    var englishSizeName = v.Size?.Name;
                    var englishDisplayName = ProductNameFormatter.Format(englishProductName, englishSizeName);

                    // Localized product name: translation or fallback to English.
                    var localizedProductName = productTranslations.TryGetValue(v.ProductId, out var ptName)
                        ? ptName
                        : englishProductName;

                    // Localized size name: translation or fallback to English.
                    var localizedSizeName = (englishSizeName is not null &&
                        sizeTranslations.TryGetValue(v.SizeId, out var stName))
                        ? stName
                        : englishSizeName;

                    var localizedName = string.IsNullOrWhiteSpace(localizedProductName)
                        ? englishProductName
                        : localizedProductName;

                    var localizedDisplayName = ProductNameFormatter.Format(localizedName, localizedSizeName);

                    return new ProductDto
                    {
                        ProductId = v.ProductId,
                        VariantId = v.VariantId,
                        Name = localizedName,
                        DisplayName = localizedDisplayName,
                        EnglishName = englishProductName,
                        EnglishDisplayName = englishDisplayName,
                        UnitPrice = v.UnitPrice,
                        TaxRate = v.Product?.TaxRate?.Rate ?? 0,
                        CategoryId = v.Product?.CategoryId ?? 0,
                        IsActive = v.IsActive
                    };
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