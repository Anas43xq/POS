using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using System.Collections.Generic;
using System.Linq;

namespace BLL.Services
{
    public class CartPricingService : ICartPricingService
    {
        public CartTotalsDto CalculateTotals(IEnumerable<CartItem> items)
        {
            var list = items as ICollection<CartItem> ?? items.ToList();

            decimal subtotal = list.Sum(i => i.LineSubtotal);
            decimal tax = list.Sum(i => i.LineTax);

            return new CartTotalsDto
            {
                Subtotal = subtotal,
                Tax = tax,
                Total = subtotal + tax
            };
        }

        public CartItem? FindMergeableLine(
            IEnumerable<CartItem> items,
            int variantId,
            IReadOnlyList<CartItemModifier> modifiers)
        {
            var targetSignature = BuildSignature(modifiers);

            return items.FirstOrDefault(item =>
                item.VariantId == variantId &&
                SignaturesEqual(BuildSignature(item.Modifiers), targetSignature));
        }

        /// <summary>
        /// Reduces a modifier selection to (ModifierOptionId -> total
        /// Quantity), order-independent. This is what "same modifier
        /// configuration" actually means for merge purposes — not the
        /// list order selections happened to be applied in.
        /// </summary>
        private static Dictionary<int, int> BuildSignature(IReadOnlyList<CartItemModifier>? modifiers)
        {
            var signature = new Dictionary<int, int>();

            if (modifiers == null)
                return signature;

            foreach (var modifier in modifiers)
            {
                signature.TryGetValue(modifier.ModifierOptionId, out int existingQuantity);
                signature[modifier.ModifierOptionId] = existingQuantity + modifier.Quantity;
            }

            return signature;
        }

        private static bool SignaturesEqual(Dictionary<int, int> a, Dictionary<int, int> b)
        {
            if (a.Count != b.Count)
                return false;

            foreach (var (optionId, quantity) in a)
            {
                if (!b.TryGetValue(optionId, out int otherQuantity) || otherQuantity != quantity)
                    return false;
            }

            return true;
        }
    }
}
