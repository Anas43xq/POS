using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces
{
    /// <summary>
    /// Configuration service for modifier groups and options.
    /// Implements the loading pipeline: product → category groups →
    /// product groups → merge → load options → map DTOs.
    /// Knows nothing about the shopping cart.
    /// </summary>
    public interface IModifierService
    {
        /// <summary>
        /// Loads modifier groups for a product by merging category-level
        /// and product-level assignments, then hydrating with options.
        /// </summary>
        Task<Result<List<ModifierGroupDto>>> GetModifierGroupsForProductAsync(int productId, int? categoryId);

        /// <summary>
        /// Loads modifier groups for a product with localization fallback.
        /// </summary>
        Task<Result<List<ModifierGroupDto>>> GetModifierGroupsForProductAsync(int productId, int? categoryId, string languageCode);
    }
}