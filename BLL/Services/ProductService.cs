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
        private readonly IProductVariantRepository _productVariantRepo;
        private readonly IProductTranslationService _productTranslationService;
        private readonly ISizeTranslationService _sizeTranslationService;
        private readonly IProductModifierGroupRepository _productModifierGroupRepo;
        private readonly ICategoryModifierGroupRepository _categoryModifierGroupRepo;
        private readonly IAuditLogService _auditLogService;
        private readonly ISessionService _sessionService;

        public ProductService(
            IProductRepository ProductRepo,
            IProductVariantRepository productVariantRepo,
            IProductTranslationService productTranslationService,
            ISizeTranslationService sizeTranslationService,
            IProductModifierGroupRepository productModifierGroupRepo,
            ICategoryModifierGroupRepository categoryModifierGroupRepo,
            IAuditLogService auditLogService,
            ISessionService sessionService)
        {
            _productrepo = ProductRepo;
            _productVariantRepo = productVariantRepo;
            _productTranslationService = productTranslationService;
            _sizeTranslationService = sizeTranslationService;
            _productModifierGroupRepo = productModifierGroupRepo;
            _categoryModifierGroupRepo = categoryModifierGroupRepo;
            _auditLogService = auditLogService;
            _sessionService = sessionService;
        }

        public async Task<Result<List<ProductSummaryDto>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _productrepo.GetAllProductsWithVariantsAsync();
                return Result<List<ProductSummaryDto>>.Success(
                    products.Select(p => MapToSummaryDto(p, p.Name)).ToList());
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
                var products = await _productrepo.GetAllProductsWithVariantsAsync();

                var translations = (await _productTranslationService
                    .GetAllByLanguageCodeAsync(languageCode))
                    .ToDictionary(t => t.ProductId, t => t.TranslatedName);

                return Result<List<ProductSummaryDto>>.Success(
                    products.Select(p =>
                    {
                        var localizedName = translations.TryGetValue(p.ProductId, out var tName)
                            && !string.IsNullOrWhiteSpace(tName)
                            ? tName
                            : p.Name;

                        return MapToSummaryDto(p, localizedName);
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

        public async Task<Result<List<ProductWithVariantsDto>>> GetAllProductsWithVariantsAsync()
        {
            try
            {
                var products = await _productrepo.GetAllProductsWithVariantsAsync();

                var result = products.Select(p => new ProductWithVariantsDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? string.Empty,
                    TaxRateId = p.TaxRateId,
                    TaxRateName = p.TaxRate?.Name ?? string.Empty,
                    TaxRatePercentage = p.TaxRate?.Rate ?? 0,
                    IsActive = p.IsActive,
                    Variants = (p.ProductVariants ?? new List<ProductVariant>())
                        .Where(v => v.IsActive)
                        .Select(v => new ProductVariantDto
                        {
                            VariantId = v.VariantId,
                            ProductId = v.ProductId,
                            SizeId = v.SizeId,
                            SizeName = v.Size?.Name ?? string.Empty,
                            UnitPrice = v.UnitPrice,
                            IsActive = v.IsActive
                        }).ToList()
                }).ToList();

                return Result<List<ProductWithVariantsDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<List<ProductWithVariantsDto>>.Failure(ex.Message);
            }
        }

        public async Task<ProductWriteDto?> GetProductByIdAsync(int id)
        {
            var entity = await _productrepo.GetByIdWithVariantsAsync(id);
            return entity is null ? null : new ProductWriteDto
            {
                ProductId = entity.ProductId,
                Name = entity.Name,
                CategoryId = entity.CategoryId,
                TaxRateId = entity.TaxRateId,
                IsActive = entity.IsActive,
                Description = entity.Description,
                Variants = entity.ProductVariants.Select(v => new ProductVariantWriteDto
                {
                    VariantId = v.VariantId,
                    SizeId = v.SizeId,
                    SizeName = v.Size?.Name ?? string.Empty,
                    UnitPrice = v.UnitPrice,
                    IsActive = v.IsActive
                }).ToList()
            };
        }

        public async Task AddProductAsync(ProductWriteDto product)
        {
            ValidateVariants(product.Variants);

            var entity = MapToEntity(product);
            await _productrepo.AddAsync(entity);

            foreach (var variant in product.Variants)
            {
                await _productVariantRepo.AddAsync(new ProductVariant
                {
                    ProductId = entity.ProductId,
                    SizeId = variant.SizeId,
                    UnitPrice = variant.UnitPrice,
                    IsActive = variant.IsActive
                });
            }

            await _auditLogService.LogAsync("Create", "Product", entity.ProductId, _sessionService.CurrentUser?.UserId,
                oldValue: null, newValue: product);
        }

        public async Task<List<string>> UpdateProductAsync(ProductWriteDto product)
        {
            ValidateVariants(product.Variants);

            var before = await GetProductByIdAsync(product.ProductId);
            await _productrepo.UpdateAsync(MapToEntity(product));
            var deactivated = await ReconcileVariantsAsync(product.ProductId, product.Variants);

            await _auditLogService.LogAsync("Update", "Product", product.ProductId, _sessionService.CurrentUser?.UserId,
                oldValue: before, newValue: product);

            return deactivated;
        }

        public async Task DeleteProductAsync(int id)
        {
            var before = await GetProductByIdAsync(id);
            await _productrepo.DeleteAsync(id);
            await _auditLogService.LogAsync("Delete", "Product", id, _sessionService.CurrentUser?.UserId,
                oldValue: before, newValue: null);
        }

        private static void ValidateVariants(List<ProductVariantWriteDto> variants)
        {
            if (variants == null || variants.Count == 0)
                throw new ArgumentException("A product needs at least one size and price.");

            if (variants.Any(v => v.UnitPrice <= 0))
                throw new ArgumentException("Every size needs a price greater than 0.");

            if (variants.Select(v => v.SizeId).Distinct().Count() != variants.Count)
                throw new ArgumentException("Each size can only be used once per product.");
        }

        private async Task<List<string>> ReconcileVariantsAsync(int productId, List<ProductVariantWriteDto> variants)
        {
            var deactivatedSizeNames = new List<string>();
            var existing = (await _productVariantRepo.GetByProductIdAsync(productId)).ToList();
            var incomingIds = variants.Where(v => v.VariantId != 0).Select(v => v.VariantId).ToHashSet();

            foreach (var stale in existing.Where(e => !incomingIds.Contains(e.VariantId)))
            {
                var outcome = await _productVariantRepo.TryDeleteAsync(stale.VariantId);
                if (outcome == VariantDeleteOutcome.Deactivated)
                {
                    deactivatedSizeNames.Add(stale.Size?.Name ?? stale.VariantId.ToString());
                }
            }

            foreach (var v in variants)
            {
                if (v.VariantId == 0)
                {
                    await _productVariantRepo.AddAsync(new ProductVariant
                    {
                        ProductId = productId,
                        SizeId = v.SizeId,
                        UnitPrice = v.UnitPrice,
                        IsActive = v.IsActive
                    });
                }
                else
                {
                    await _productVariantRepo.UpdateAsync(new ProductVariant
                    {
                        VariantId = v.VariantId,
                        ProductId = productId,
                        SizeId = v.SizeId,
                        UnitPrice = v.UnitPrice,
                        IsActive = v.IsActive
                    });
                }
            }

            return deactivatedSizeNames;
        }

        private static ProductSummaryDto MapToSummaryDto(Product p, string name)
        {
            var activeVariants = p.ProductVariants?.Where(v => v.IsActive).ToList() ?? new List<ProductVariant>();

            return new ProductSummaryDto
            {
                ProductId = p.ProductId,
                Name = name,
                CategoryId = p.CategoryId,
                MinUnitPrice = activeVariants.Count > 0 ? activeVariants.Min(v => v.UnitPrice) : null,
                MaxUnitPrice = activeVariants.Count > 0 ? activeVariants.Max(v => v.UnitPrice) : null,
                VariantCount = activeVariants.Count,
                TaxRateId = p.TaxRateId,
                TaxRateName = p.TaxRate?.Name ?? string.Empty,
                IsActive = p.IsActive
            };
        }

        private async Task<HashSet<int>> BuildModifierProductIdSetAsync(IEnumerable<Product> activeProducts)
        {
            var productAssignmentsTask = _productModifierGroupRepo.GetAllAsync();
            var categoryAssignmentsTask = _categoryModifierGroupRepo.GetAllAsync();

            await Task.WhenAll(productAssignmentsTask, categoryAssignmentsTask);

            var productIds = new HashSet<int>();
            foreach (var a in productAssignmentsTask.Result)
                productIds.Add(a.ProductId);

            var categoryIds = categoryAssignmentsTask.Result.Select(a => a.CategoryId).ToHashSet();

            if (categoryIds.Count > 0)
            {
                foreach (var p in activeProducts.Where(p => categoryIds.Contains(p.CategoryId)))
                    productIds.Add(p.ProductId);
            }

            return productIds;
        }

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
            TaxRateId = d.TaxRateId,
            IsActive = d.IsActive,
            Description = d.Description
        };
    }
}
