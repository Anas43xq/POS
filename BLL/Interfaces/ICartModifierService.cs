using BLL.DTOs;
using BLL.Models;
using System.Collections.Generic;

namespace BLL.Interfaces
{
    /// <summary>
    /// Cart-scoped service for applying/removing/editing modifier selections
    /// on a single cart line item. Operates purely on DTOs and cart models —
    /// knows nothing about EF entities.
    /// </summary>
    public interface ICartModifierService
    {
        /// <summary>
        /// Validates that all required groups are satisfied and selections
        /// fall within MinSelections/MaxSelections for each group.
        /// Returns a list of validation error messages (empty = valid).
        /// </summary>
        List<string> ValidateModifierSelections(
            List<ModifierGroupDto> groups,
            List<CartItemModifier> selectedModifiers);

        /// <summary>
        /// Applies a modifier selection: replaces previous selection for the
        /// same group (single-select) or adds to existing (multi-select/quantity).
        /// </summary>
        void ApplyModifier(
            ModifierGroupDto group,
            ModifierOptionDto option,
            int quantity,
            List<CartItemModifier> modifiers);

        /// <summary>
        /// Removes a modifier selection from the cart item.
        /// </summary>
        void RemoveModifier(int modifierOptionId, List<CartItemModifier> modifiers);

        /// <summary>
        /// Calculates the total modifier price for all selected modifiers.
        /// </summary>
        decimal CalculateModifierTotal(List<CartItemModifier> modifiers);

        /// <summary>
        /// Builds a compact summary string for display in the cart UI
        /// (e.g. "Whole Milk, Extra Shot ×2").
        /// </summary>
        string BuildModifierSummary(List<CartItemModifier> modifiers);
    }
}