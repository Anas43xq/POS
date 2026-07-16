using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLL.Services
{
    public class CartModifierService : ICartModifierService
    {
        public List<string> ValidateModifierSelections(
            List<ModifierGroupDto> groups,
            List<CartItemModifier> selectedModifiers)
        {
            var errors = new List<string>();

            foreach (var group in groups)
            {
                var selections = selectedModifiers
                    .Where(m => m.ModifierGroupId == group.ModifierGroupId)
                    .ToList();

                int count = selections.Count;

                // Required groups must have at least one selection
                if (group.IsRequired && count == 0)
                {
                    errors.Add($"{group.Name} is required");
                    continue;
                }

                // Skip Min/Max validation for quantity groups (GroupType 3)
                // since they have a fixed set of options
                if (group.GroupType == 3)
                    continue;

                // Validate minimum selections
                if (count < group.MinSelections)
                    errors.Add($"{group.Name}: select at least {group.MinSelections}");

                // Validate maximum selections
                if (count > group.MaxSelections)
                    errors.Add($"{group.Name}: select at most {group.MaxSelections}");
            }

            return errors;
        }

        public void ApplyModifier(
            ModifierGroupDto group,
            ModifierOptionDto option,
            int quantity,
            List<CartItemModifier> modifiers)
        {
            if (quantity < 1)
                quantity = 1;

            // Single-select: remove existing selections for this group first
            if (group.GroupType == 1)
            {
                modifiers.RemoveAll(m => m.ModifierGroupId == group.ModifierGroupId);
            }
            // Multi-select, Quantity: check if option already exists
            else
            {
                var existing = modifiers.FirstOrDefault(m =>
                    m.ModifierOptionId == option.ModifierOptionId);

                if (existing != null)
                {
                    if (group.GroupType == 2)
                    {
                        // Multi-select: toggle off
                        modifiers.Remove(existing);
                        return;
                    }
                    else
                    {
                        // Quantity group: increment
                        existing.Quantity += quantity;
                        return;
                    }
                }
            }

            modifiers.Add(new CartItemModifier
            {
                ModifierGroupId = group.ModifierGroupId,
                GroupName = group.Name,
                ModifierOptionId = option.ModifierOptionId,
                OptionName = option.Name,
                Quantity = quantity,
                PriceAdd = option.PriceAdd,
                GroupType = group.GroupType
            });
        }

        public void RemoveModifier(int modifierOptionId, List<CartItemModifier> modifiers)
        {
            modifiers.RemoveAll(m => m.ModifierOptionId == modifierOptionId);
        }

        public decimal CalculateModifierTotal(List<CartItemModifier> modifiers)
        {
            if (modifiers.Count == 0)
                return 0;

            return modifiers.Sum(m => m.LineTotal);
        }

        public string BuildModifierSummary(List<CartItemModifier> modifiers)
        {
            if (modifiers.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var modifier in modifiers)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(modifier.OptionName);

                if (modifier.Quantity > 1)
                    sb.Append($" ×{modifier.Quantity}");
            }

            return sb.ToString();
        }
    }
}