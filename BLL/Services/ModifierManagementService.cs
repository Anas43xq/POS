using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>
/// Manager-side CRUD for modifier groups, options, and
/// category/product assignments. Uses EF Core via repositories.
/// </summary>
public class ModifierManagementService : IModifierManagementService
{
    private readonly IModifierGroupRepository _groupRepo;
    private readonly IModifierOptionRepository _optionRepo;
    private readonly ICategoryModifierGroupRepository _categoryGroupRepo;
    private readonly IProductModifierGroupRepository _productGroupRepo;
    private readonly ILogger<ModifierManagementService> _logger;

    public ModifierManagementService(
        IModifierGroupRepository groupRepo,
        IModifierOptionRepository optionRepo,
        ICategoryModifierGroupRepository categoryGroupRepo,
        IProductModifierGroupRepository productGroupRepo,
        ILogger<ModifierManagementService> logger)
    {
        _groupRepo = groupRepo;
        _optionRepo = optionRepo;
        _categoryGroupRepo = categoryGroupRepo;
        _productGroupRepo = productGroupRepo;
        _logger = logger;
    }

    // ── Group CRUD ──────────────────────────────────────

    public async Task<Result<List<ModifierGroupSummaryDto>>> GetAllGroupsAsync()
    {
        try
        {
            var groups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
            var summaries = groups
                .OrderBy(g => g.SortOrder)
                .ThenBy(g => g.Name)
                .Select(MapToSummary)
                .ToList();
            return Result<List<ModifierGroupSummaryDto>>.Success(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load all modifier groups");
            return Result<List<ModifierGroupSummaryDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<ModifierGroupDetailDto>> GetGroupDetailAsync(int groupId)
    {
        try
        {
            var groups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
            var group = groups.FirstOrDefault(g => g.ModifierGroupId == groupId);
            if (group is null)
                return Result<ModifierGroupDetailDto>.Failure($"Modifier group {groupId} not found.");

            return Result<ModifierGroupDetailDto>.Success(MapToDetail(group));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load modifier group detail for {GroupId}", groupId);
            return Result<ModifierGroupDetailDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<int>> CreateGroupAsync(ModifierGroupWriteDto dto)
    {
        try
        {
            var entity = new ModifierGroup
            {
                Name = dto.Name.Trim(),
                GroupType = dto.GroupType,
                IsRequired = dto.IsRequired,
                IsActive = dto.IsActive,
                MinSelections = dto.MinSelections,
                MaxSelections = dto.MaxSelections,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _groupRepo.AddAsync(entity);
            return Result<int>.Success(entity.ModifierGroupId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create modifier group");
            return Result<int>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> UpdateGroupAsync(ModifierGroupWriteDto dto)
    {
        try
        {
            if (!dto.ModifierGroupId.HasValue)
                return Result<bool>.Failure("ModifierGroupId is required for update.");

            var entity = await _groupRepo.GetByIdAsync(dto.ModifierGroupId.Value);
            if (entity is null)
                return Result<bool>.Failure($"Modifier group {dto.ModifierGroupId} not found.");

            entity.Name = dto.Name.Trim();
            entity.GroupType = dto.GroupType;
            entity.IsRequired = dto.IsRequired;
            entity.IsActive = dto.IsActive;
            entity.MinSelections = dto.MinSelections;
            entity.MaxSelections = dto.MaxSelections;
            entity.SortOrder = dto.SortOrder;
            entity.UpdatedAt = DateTime.UtcNow;

            await _groupRepo.UpdateAsync(entity);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update modifier group {GroupId}", dto.ModifierGroupId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteGroupAsync(int groupId)
    {
        try
        {
            // Check for category assignments
            var allGroups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
            var group = allGroups.FirstOrDefault(g => g.ModifierGroupId == groupId);
            if (group is null)
                return Result<bool>.Failure($"Modifier group {groupId} not found.");

            if (group.CategoryModifierGroups.Any())
                return Result<bool>.Failure("Cannot delete group: it is assigned to one or more categories. Remove assignments first.");

            if (group.ProductModifierGroups.Any())
                return Result<bool>.Failure("Cannot delete group: it is assigned to one or more products. Remove assignments first.");

            await _groupRepo.DeleteAsync(groupId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete modifier group {GroupId}", groupId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> ToggleGroupActiveAsync(int groupId, bool isActive)
    {
        try
        {
            var entity = await _groupRepo.GetByIdAsync(groupId);
            if (entity is null)
                return Result<bool>.Failure($"Modifier group {groupId} not found.");

            entity.IsActive = isActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _groupRepo.UpdateAsync(entity);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle modifier group {GroupId} active state", groupId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    // ── Option CRUD ─────────────────────────────────────

    public async Task<Result<int>> CreateOptionAsync(ModifierOptionWriteDto dto)
    {
        try
        {
            // Validate single-select default constraint
            if (dto.IsDefault)
            {
                var existingOptions = await _optionRepo.GetByGroupIdAsync(dto.ModifierGroupId);
                if (existingOptions.Any(o => o.IsDefault))
                {
                    // Clear existing defaults for single-select groups
                    var groups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
                    var parentGroup = groups.FirstOrDefault(g => g.ModifierGroupId == dto.ModifierGroupId);
                    if (parentGroup?.GroupType == 1) // SingleSelect
                    {
                        foreach (var existing in existingOptions.Where(o => o.IsDefault))
                        {
                            existing.IsDefault = false;
                            existing.UpdatedAt = DateTime.UtcNow;
                            await _optionRepo.UpdateAsync(existing);
                        }
                    }
                }
            }

            var entity = new ModifierOption
            {
                ModifierGroupId = dto.ModifierGroupId,
                Name = dto.Name.Trim(),
                PriceAdd = dto.PriceAdd,
                AllowQuantity = dto.AllowQuantity,
                IsDefault = dto.IsDefault,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _optionRepo.AddAsync(entity);
            return Result<int>.Success(entity.ModifierOptionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create modifier option");
            return Result<int>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> UpdateOptionAsync(ModifierOptionWriteDto dto)
    {
        try
        {
            if (!dto.ModifierOptionId.HasValue)
                return Result<bool>.Failure("ModifierOptionId is required for update.");

            var entity = await _optionRepo.GetByIdAsync(dto.ModifierOptionId.Value);
            if (entity is null)
                return Result<bool>.Failure($"Modifier option {dto.ModifierOptionId} not found.");

            // Validate single-select default constraint
            if (dto.IsDefault && !entity.IsDefault)
            {
                var groups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
                var parentGroup = groups.FirstOrDefault(g => g.ModifierGroupId == entity.ModifierGroupId);
                if (parentGroup?.GroupType == 1) // SingleSelect
                {
                    var siblings = await _optionRepo.GetByGroupIdAsync(entity.ModifierGroupId);
                    foreach (var sibling in siblings.Where(o => o.IsDefault && o.ModifierOptionId != entity.ModifierOptionId))
                    {
                        sibling.IsDefault = false;
                        sibling.UpdatedAt = DateTime.UtcNow;
                        await _optionRepo.UpdateAsync(sibling);
                    }
                }
            }

            entity.Name = dto.Name.Trim();
            entity.PriceAdd = dto.PriceAdd;
            entity.AllowQuantity = dto.AllowQuantity;
            entity.IsDefault = dto.IsDefault;
            entity.IsActive = dto.IsActive;
            entity.SortOrder = dto.SortOrder;
            entity.UpdatedAt = DateTime.UtcNow;

            await _optionRepo.UpdateAsync(entity);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update modifier option {OptionId}", dto.ModifierOptionId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteOptionAsync(int optionId)
    {
        try
        {
            await _optionRepo.DeleteAsync(optionId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete modifier option {OptionId}", optionId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    // ── Category ↔ Group assignment ─────────────────────

    public async Task<Result<List<ModifierGroupSummaryDto>>> GetCategoryAssignedGroupsAsync(int categoryId)
    {
        try
        {
            var assignments = await _categoryGroupRepo.GetByCategoryIdAsync(categoryId);
            var groupIds = assignments.Select(a => a.ModifierGroupId).ToHashSet();

            var allGroups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
            var assigned = allGroups
                .Where(g => groupIds.Contains(g.ModifierGroupId))
                .OrderBy(g => g.SortOrder)
                .Select(MapToSummary)
                .ToList();

            return Result<List<ModifierGroupSummaryDto>>.Success(assigned);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load category modifier groups for {CategoryId}", categoryId);
            return Result<List<ModifierGroupSummaryDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> AssignGroupToCategoryAsync(int categoryId, int modifierGroupId)
    {
        try
        {
            // Check for duplicate
            var existing = await _categoryGroupRepo.GetByCategoryIdAsync(categoryId);
            if (existing.Any(a => a.ModifierGroupId == modifierGroupId))
                return Result<bool>.Failure("This group is already assigned to the category.");

            var entity = new CategoryModifierGroup
            {
                CategoryId = categoryId,
                ModifierGroupId = modifierGroupId
            };

            await _categoryGroupRepo.AddAsync(entity);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign group {GroupId} to category {CategoryId}", modifierGroupId, categoryId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> RemoveGroupFromCategoryAsync(int categoryId, int modifierGroupId)
    {
        try
        {
            var existing = await _categoryGroupRepo.GetByCategoryIdAsync(categoryId);
            var assignment = existing.FirstOrDefault(a => a.ModifierGroupId == modifierGroupId);
            if (assignment is null)
                return Result<bool>.Failure("This group is not assigned to the category.");

            await _categoryGroupRepo.DeleteByCompositeKeyAsync(assignment.CategoryId, assignment.ModifierGroupId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove group {GroupId} from category {CategoryId}", modifierGroupId, categoryId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    // ── Product ↔ Group assignment ──────────────────────

    public async Task<Result<List<ModifierGroupSummaryDto>>> GetProductAssignedGroupsAsync(int productId)
    {
        try
        {
            var assignments = await _productGroupRepo.GetByProductIdAsync(productId);
            var groupIds = assignments.Select(a => a.ModifierGroupId).ToHashSet();

            var allGroups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
            var assigned = allGroups
                .Where(g => groupIds.Contains(g.ModifierGroupId))
                .OrderBy(g => g.SortOrder)
                .Select(MapToSummary)
                .ToList();

            return Result<List<ModifierGroupSummaryDto>>.Success(assigned);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load product modifier groups for {ProductId}", productId);
            return Result<List<ModifierGroupSummaryDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<ModifierGroupSummaryDto>>> GetProductInheritedGroupsAsync(int productId, int categoryId)
    {
        try
        {
            var categoryAssignments = await _categoryGroupRepo.GetByCategoryIdAsync(categoryId);
            var groupIds = categoryAssignments.Select(a => a.ModifierGroupId).ToHashSet();

            var allGroups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
            var inherited = allGroups
                .Where(g => groupIds.Contains(g.ModifierGroupId))
                .OrderBy(g => g.SortOrder)
                .Select(MapToSummary)
                .ToList();

            return Result<List<ModifierGroupSummaryDto>>.Success(inherited);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load inherited groups for product {ProductId}", productId);
            return Result<List<ModifierGroupSummaryDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> AssignGroupToProductAsync(int productId, int modifierGroupId)
    {
        try
        {
            var existing = await _productGroupRepo.GetByProductIdAsync(productId);
            if (existing.Any(a => a.ModifierGroupId == modifierGroupId))
                return Result<bool>.Failure("This group is already assigned to the product.");

            var entity = new ProductModifierGroup
            {
                ProductId = productId,
                ModifierGroupId = modifierGroupId
            };

            await _productGroupRepo.AddAsync(entity);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign group {GroupId} to product {ProductId}", modifierGroupId, productId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> RemoveGroupFromProductAsync(int productId, int modifierGroupId)
    {
        try
        {
            var existing = await _productGroupRepo.GetByProductIdAsync(productId);
            var assignment = existing.FirstOrDefault(a => a.ModifierGroupId == modifierGroupId);
            if (assignment is null)
                return Result<bool>.Failure("This group is not assigned to the product.");

            await _productGroupRepo.DeleteByCompositeKeyAsync(assignment.ProductId, assignment.ModifierGroupId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove group {GroupId} from product {ProductId}", modifierGroupId, productId);
            return Result<bool>.Failure(ex.Message);
        }
    }

    // ── Localized overloads ─────────────────────────────

    public async Task<Result<List<ModifierGroupSummaryDto>>> GetAllGroupsAsync(string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode) || languageCode == "en")
            return await GetAllGroupsAsync();

        try
        {
            var groups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
            var summaries = groups
                .OrderBy(g => g.SortOrder)
                .ThenBy(g => g.Name)
                .Select(g => MapToSummaryLocalized(g, languageCode))
                .ToList();
            return Result<List<ModifierGroupSummaryDto>>.Success(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load all modifier groups in language {Language}", languageCode);
            return Result<List<ModifierGroupSummaryDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<ModifierGroupDetailDto>> GetGroupDetailAsync(int groupId, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode) || languageCode == "en")
            return await GetGroupDetailAsync(groupId);

        try
        {
            var groups = await _groupRepo.GetAllWithOptionsAndTranslationsAsync();
            var group = groups.FirstOrDefault(g => g.ModifierGroupId == groupId);
            if (group is null)
                return Result<ModifierGroupDetailDto>.Failure($"Modifier group {groupId} not found.");

            return Result<ModifierGroupDetailDto>.Success(MapToDetailLocalized(group, languageCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load modifier group detail for {GroupId} in language {Language}", groupId, languageCode);
            return Result<ModifierGroupDetailDto>.Failure(ex.Message);
        }
    }

    // ── Mapping helpers ─────────────────────────────────

    private static ModifierGroupSummaryDto MapToSummary(ModifierGroup g) => new()
    {
        ModifierGroupId = g.ModifierGroupId,
        Name = g.Name,
        GroupType = g.GroupType,
        GroupTypeDisplay = g.GroupType switch
        {
            1 => "Single Select",
            2 => "Multi Select",
            3 => "Quantity",
            _ => "Unknown"
        },
        IsRequired = g.IsRequired,
        IsActive = g.IsActive,
        OptionCount = g.ModifierOptions.Count,
        SortOrder = g.SortOrder
    };

    private static ModifierGroupSummaryDto MapToSummaryLocalized(ModifierGroup g, string languageCode)
    {
        var translation = g.ModifierGroupTranslations
            .FirstOrDefault(t => t.LanguageCode == languageCode);

        return new ModifierGroupSummaryDto
        {
            ModifierGroupId = g.ModifierGroupId,
            Name = translation?.Name ?? g.Name,
            GroupType = g.GroupType,
            GroupTypeDisplay = g.GroupType switch
            {
                1 => "Single Select",
                2 => "Multi Select",
                3 => "Quantity",
                _ => "Unknown"
            },
            IsRequired = g.IsRequired,
            IsActive = g.IsActive,
            OptionCount = g.ModifierOptions.Count,
            SortOrder = g.SortOrder
        };
    }

    private static ModifierGroupDetailDto MapToDetail(ModifierGroup g) => new()
    {
        ModifierGroupId = g.ModifierGroupId,
        Name = g.Name,
        GroupType = g.GroupType,
        IsRequired = g.IsRequired,
        IsActive = g.IsActive,
        MinSelections = g.MinSelections,
        MaxSelections = g.MaxSelections,
        SortOrder = g.SortOrder,
        Options = g.ModifierOptions
            .OrderBy(o => o.SortOrder)
            .Select(o => new ModifierOptionDetailDto
            {
                ModifierOptionId = o.ModifierOptionId,
                Name = o.Name,
                PriceAdd = o.PriceAdd,
                AllowQuantity = o.AllowQuantity,
                IsDefault = o.IsDefault,
                IsActive = o.IsActive,
                SortOrder = o.SortOrder
            })
            .ToList()
    };

    private static ModifierGroupDetailDto MapToDetailLocalized(ModifierGroup g, string languageCode)
    {
        var translation = g.ModifierGroupTranslations
            .FirstOrDefault(t => t.LanguageCode == languageCode);

        return new ModifierGroupDetailDto
        {
            ModifierGroupId = g.ModifierGroupId,
            Name = translation?.Name ?? g.Name,
            GroupType = g.GroupType,
            IsRequired = g.IsRequired,
            IsActive = g.IsActive,
            MinSelections = g.MinSelections,
            MaxSelections = g.MaxSelections,
            SortOrder = g.SortOrder,
            Options = g.ModifierOptions
                .OrderBy(o => o.SortOrder)
                .Select(o =>
                {
                    var optTranslation = o.ModifierOptionTranslations
                        .FirstOrDefault(t => t.LanguageCode == languageCode);
                    return new ModifierOptionDetailDto
                    {
                        ModifierOptionId = o.ModifierOptionId,
                        Name = optTranslation?.Name ?? o.Name,
                        PriceAdd = o.PriceAdd,
                        AllowQuantity = o.AllowQuantity,
                        IsDefault = o.IsDefault,
                        IsActive = o.IsActive,
                        SortOrder = o.SortOrder
                    };
                })
                .ToList()
        };
    }
}