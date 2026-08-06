using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces;

/// <summary>
/// Manager-side CRUD service for modifier groups, options,
/// and their assignment to categories and products.
/// Separate from <see cref="IModifierService"/> which is the
/// read-only cashier pipeline.
/// </summary>
public interface IModifierManagementService
{
    // ── Group CRUD ──────────────────────────────────────

    Task<Result<List<ModifierGroupSummaryDto>>> GetAllGroupsAsync();
    Task<Result<List<ModifierGroupSummaryDto>>> GetAllGroupsAsync(string languageCode);
    Task<Result<ModifierGroupDetailDto>> GetGroupDetailAsync(int groupId);
    Task<Result<ModifierGroupDetailDto>> GetGroupDetailAsync(int groupId, string languageCode);
    Task<Result<int>> CreateGroupAsync(ModifierGroupWriteDto dto);
    Task<Result<bool>> UpdateGroupAsync(ModifierGroupWriteDto dto);
    Task<Result<bool>> DeleteGroupAsync(int groupId);
    Task<Result<bool>> ToggleGroupActiveAsync(int groupId, bool isActive);

    // ── Option CRUD ─────────────────────────────────────

    Task<Result<int>> CreateOptionAsync(ModifierOptionWriteDto dto);
    Task<Result<bool>> UpdateOptionAsync(ModifierOptionWriteDto dto);
    Task<Result<bool>> DeleteOptionAsync(int optionId);

    // ── Category ↔ Group assignment ─────────────────────

    Task<Result<List<ModifierGroupSummaryDto>>> GetCategoryAssignedGroupsAsync(int categoryId);
    Task<Result<bool>> AssignGroupToCategoryAsync(int categoryId, int modifierGroupId);
    Task<Result<bool>> RemoveGroupFromCategoryAsync(int categoryId, int modifierGroupId);

    // ── Product ↔ Group assignment ──────────────────────

    Task<Result<List<ModifierGroupSummaryDto>>> GetProductAssignedGroupsAsync(int productId);
    Task<Result<List<ModifierGroupSummaryDto>>> GetProductInheritedGroupsAsync(int productId, int categoryId);
    Task<Result<bool>> AssignGroupToProductAsync(int productId, int modifierGroupId);
    Task<Result<bool>> RemoveGroupFromProductAsync(int productId, int modifierGroupId);
}