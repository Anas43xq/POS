using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class ModifierService : IModifierService
    {
        private readonly IModifierGroupRepository _modifierGroupRepo;
        private readonly ICategoryModifierGroupRepository _categoryModifierGroupRepo;
        private readonly IProductModifierGroupRepository _productModifierGroupRepo;
        private readonly ILogger<ModifierService> _logger;

        public ModifierService(
            IModifierGroupRepository modifierGroupRepo,
            ICategoryModifierGroupRepository categoryModifierGroupRepo,
            IProductModifierGroupRepository productModifierGroupRepo,
            ILogger<ModifierService> logger)
        {
            _modifierGroupRepo = modifierGroupRepo;
            _categoryModifierGroupRepo = categoryModifierGroupRepo;
            _productModifierGroupRepo = productModifierGroupRepo;
            _logger = logger;
        }

        public async Task<Result<List<ModifierGroupDto>>> GetModifierGroupsForProductAsync(int productId, int? categoryId)
        {
            try
            {
                var mergedIds = await LoadAndMergeGroupIdsAsync(productId, categoryId);
                var groups = await HydrateGroupsWithOptionsAsync(mergedIds);
                return Result<List<ModifierGroupDto>>.Success(
                    groups.Select(MapToDto).OrderBy(g => g.SortOrder).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load modifier groups for product {ProductId}", productId);
                return Result<List<ModifierGroupDto>>.Failure(ex.Message);
            }
        }

        public async Task<Result<List<ModifierGroupDto>>> GetModifierGroupsForProductAsync(int productId, int? categoryId, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode) || languageCode == "en")
                return await GetModifierGroupsForProductAsync(productId, categoryId);

            try
            {
                var mergedIds = await LoadAndMergeGroupIdsAsync(productId, categoryId);
                var groups = await HydrateGroupsWithOptionsAsync(mergedIds);
                return Result<List<ModifierGroupDto>>.Success(
                    groups.Select(g => MapToDtoLocalized(g, languageCode)).OrderBy(g => g.SortOrder).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load modifier groups for product {ProductId} in language {Language}", productId, languageCode);
                return Result<List<ModifierGroupDto>>.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Loading pipeline step 1-3: load category modifier groups,
        /// load product modifier groups, merge deduplicated group IDs.
        /// </summary>
        private async Task<HashSet<int>> LoadAndMergeGroupIdsAsync(int productId, int? categoryId)
        {
            var groupIds = new HashSet<int>();

            // Step 1: Load category modifier groups
            if (categoryId.HasValue)
            {
                var categoryAssignments = await _categoryModifierGroupRepo.GetByCategoryIdAsync(categoryId.Value);
                foreach (var assignment in categoryAssignments)
                    groupIds.Add(assignment.ModifierGroupId);
            }

            // Step 2: Load product modifier groups
            var productAssignments = await _productModifierGroupRepo.GetByProductIdAsync(productId);
            foreach (var assignment in productAssignments)
                groupIds.Add(assignment.ModifierGroupId);

            return groupIds;
        }

        /// <summary>
        /// Loading pipeline step 4-5: load modifier group entities with
        /// options and translations for the merged group IDs.
        /// </summary>
        private async Task<List<DAL.Entities.ModifierGroup>> HydrateGroupsWithOptionsAsync(HashSet<int> groupIds)
        {
            var allGroups = await _modifierGroupRepo.GetAllWithOptionsAndTranslationsAsync();

            return allGroups
                .Where(g => groupIds.Contains(g.ModifierGroupId))
                .ToList();
        }

        private static ModifierGroupDto MapToDto(DAL.Entities.ModifierGroup e) => new()
        {
            ModifierGroupId = e.ModifierGroupId,
            Name = e.Name,
            GroupType = e.GroupType,
            IsRequired = e.IsRequired,
            MinSelections = e.MinSelections,
            MaxSelections = e.MaxSelections,
            SortOrder = e.SortOrder,
            Options = e.ModifierOptions
                .Where(o => o.IsActive)
                .OrderBy(o => o.SortOrder)
                .Select(o => new ModifierOptionDto
                {
                    ModifierOptionId = o.ModifierOptionId,
                    Name = o.Name,
                    PriceAdd = o.PriceAdd,
                    AllowQuantity = o.AllowQuantity,
                    IsDefault = o.IsDefault,
                    SortOrder = o.SortOrder
                })
                .ToList()
        };

        private static ModifierGroupDto MapToDtoLocalized(DAL.Entities.ModifierGroup e, string languageCode)
        {
            var translation = e.ModifierGroupTranslations
                .FirstOrDefault(t => t.LanguageCode == languageCode);

            return new ModifierGroupDto
            {
                ModifierGroupId = e.ModifierGroupId,
                Name = translation?.Name ?? e.Name,
                GroupType = e.GroupType,
                IsRequired = e.IsRequired,
                MinSelections = e.MinSelections,
                MaxSelections = e.MaxSelections,
                SortOrder = e.SortOrder,
                Options = e.ModifierOptions
                    .Where(o => o.IsActive)
                    .OrderBy(o => o.SortOrder)
                    .Select(o =>
                    {
                        var optTranslation = o.ModifierOptionTranslations
                            .FirstOrDefault(t => t.LanguageCode == languageCode);

                        return new ModifierOptionDto
                        {
                            ModifierOptionId = o.ModifierOptionId,
                            Name = optTranslation?.Name ?? o.Name,
                            PriceAdd = o.PriceAdd,
                            AllowQuantity = o.AllowQuantity,
                            IsDefault = o.IsDefault,
                            SortOrder = o.SortOrder
                        };
                    })
                    .ToList()
            };
        }
    }
}