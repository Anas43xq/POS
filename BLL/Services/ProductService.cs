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
        private readonly IProductModifierGroupRepository _productModifierGroupRepo;
        private readonly ICategoryModifierGroupRepository _categoryModifierGroupRepo;

        public ProductService(
            IProductRepository ProductRepo,
            IProductTranslationService productTranslationService,
            ISizeTranslationService sizeTranslationService,
            IProductModifierGroupRepository productModifierGroupRepo,
            ICategoryModifierGroupRepository categoryModifierGroupRepo)
        {
            _productrepo = ProductRepo;
            _productTranslationService = productTranslationService;
            _sizeTranslationService = sizeTranslationService;
            _productModifierGroupRepo = productModifierGroupRepo;
            _categoryModifierGroupRepo = categoryModifierGroupRepo;
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

        public async Task<Result<List<ProductSummaryDto>>> GetProductSummariesAsync(string? languageCode = null)
        {
            try
            {
                var products = await _productrepo.GetProductSummariesAsync();

                if (string.IsNullOrWhiteSpace(languageCode) || languageCode == "en")
                {
                    var mapped = products.Select(p => new ProductSummaryDto
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        IsActive = p.IsActive
                    }).ToList();

                    return Result<List<ProductSummaryDto>>.Success(mapped);
                }

                var translations = (await _productTranslationService
                    .GetAllByLanguageCodeAsync(languageCode))
                    .ToDictionary(t => t.ProductId, t => t.TranslatedName);

                var mappedLocalized = products.Select(p =>
                    {
                        var localizedName = translations.TryGetValue(p.ProductId, out var tName)
                            && !string.IsNullOrWhiteSpace(tName)
                            ? tName
                            : p.Name;

                        return new ProductSummaryDto
                        {
                            ProductId = p.ProductId,
                            Name = localizedName,
                            IsActive = p.IsActive
                        };
                    }).ToList();

                return Result<List<ProductSummaryDto>>.Success(mappedLocalized);
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

                var modifierProductIds = await BuildModifierProductIdSetAsync(GetActiveProductsFromVariants(variants));

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
                        IsActive = v.IsActive,
                        HasModifiers = modifierProductIds.Contains(v.ProductId)
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

                var productTranslations = (await _productTranslationService
                    .GetAllByLanguageCodeAsync(languageCode))
                    .ToDictionary(t => t.ProductId, t => t.TranslatedName);

                var sizeTranslations = (await _sizeTranslationService
                    .GetAllByLanguageCodeAsync(languageCode))
                    .ToDictionary(t => t.SizeId, t => t.TranslatedName);

                var modifierProductIds = await BuildModifierProductIdSetAsync(GetActiveProductsFromVariants(variants));

                var result = variants.Select(v =>
                {
                    var englishProductName = v.Product?.Name ?? string.Empty;
                    var englishSizeName = v.Size?.Name;
                    var englishDisplayName = ProductNameFormatter.Format(englishProductName, englishSizeName);

                    var localizedProductName = productTranslations.TryGetValue(v.ProductId, out var ptName)
                        ? ptName
                        : englishProductName;

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
                        IsActive = v.IsActive,
                        HasModifiers = modifierProductIds.Contains(v.ProductId)
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

        private async Task<HashSet<int>> BuildModifierProductIdSetAsync(IEnumerable<Product> activeProducts)
        {
            // NOTE: The two lookups below are independent of one another
            // (direct product->modifier-group assignments vs.
            // category->modifier-group assignments), so they're fired
            // concurrently instead of as two sequential round trips. The
            // third step (resolving which products fall under a
            // modifier-flagged category) genuinely depends on the result of
            // the second, so it can't join the same Task.WhenAll batch —
            // but it no longer needs its own DB round trip at all: callers
            // already have the full Product set loaded (from the variants
            // query), so it's resolved from that in-memory data instead of
            // re-querying Products. See login-performance-analysis.md §9/§11
            // for the (deferred) idea of collapsing all of this into a
            // single SQL view/UNION, which would require a DB-side change.
            var productAssignmentsTask = _productModifierGroupRepo.GetAllAsync();
            var categoryAssignmentsTask = _categoryModifierGroupRepo.GetAllAsync();

            await Task.WhenAll(productAssignmentsTask, categoryAssignmentsTask);

            var productIds = new HashSet<int>();
            foreach (var a in productAssignmentsTask.Result)
                productIds.Add(a.ProductId);

            var categoryIds = categoryAssignmentsTask.Result.Select(a => a.CategoryId).ToHashSet();

            // For category-level modifiers, resolve which products belong to those categories
            if (categoryIds.Count > 0)
            {
                foreach (var p in activeProducts.Where(p => categoryIds.Contains(p.CategoryId)))
                    productIds.Add(p.ProductId);
            }

            return productIds;
        }

        /// <summary>
        /// Extracts the distinct, active Product entities already attached
        /// to a variants result (loaded via ProductRepository.GetAllVariantsAsync's
        /// .Include(v => v.Product)) so BuildModifierProductIdSetAsync can
        /// resolve category membership from in-memory data instead of
        /// re-querying Products. Explicitly re-applies the IsActive filter
        /// that GetAllProductsWithTaxRateAsync used to apply on its own —
        /// the variants query only filters on variant-level IsActive, not
        /// the parent Product's, so this keeps prior behavior unchanged.
        /// </summary>
        private static IEnumerable<Product> GetActiveProductsFromVariants(IEnumerable<ProductVariant> variants) =>
            variants
                .Select(v => v.Product)
                .Where(p => p != null && p.IsActive)
                .DistinctBy(p => p!.ProductId)!;

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