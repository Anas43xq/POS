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

                if (group.IsRequired && count == 0)
                {
                    errors.Add($"{group.Name} is required");
                    continue;
                }

                if (group.GroupType == 3)
                    continue;

                if (count < group.MinSelections)
                    errors.Add($"{group.Name}: select at least {group.MinSelections}");

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

            if (group.GroupType == 1)
            {
                modifiers.RemoveAll(m => m.ModifierGroupId == group.ModifierGroupId);
            }
            else
            {
                var existing = modifiers.FirstOrDefault(m =>
                    m.ModifierOptionId == option.ModifierOptionId);

                if (existing != null)
                {
                    if (group.GroupType == 2)
                    {
                        modifiers.Remove(existing);
                        return;
                    }
                    else
                    {
                        existing.Quantity += quantity;
                        return;
                    }
                }
            }

            modifiers.Add(new CartItemModifier
            {
                ModifierGroupId = group.ModifierGroupId,
                GroupName = group.Name,
                EnglishGroupName = string.IsNullOrWhiteSpace(group.EnglishName) ? group.Name : group.EnglishName,
                ModifierOptionId = option.ModifierOptionId,
                OptionName = option.Name,
                EnglishOptionName = string.IsNullOrWhiteSpace(option.EnglishName) ? option.Name : option.EnglishName,
                Quantity = quantity,
                PriceAdd = option.PriceAdd,
                GroupType = group.GroupType,
                IsDefault = option.IsDefault
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

        public decimal CalculateEffectiveUnitPrice(decimal baseUnitPrice, List<CartItemModifier> modifiers)
        {
            return baseUnitPrice + CalculateModifierTotal(modifiers);
        }

        public string BuildModifierSummary(List<CartItemModifier> modifiers)
        {

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
