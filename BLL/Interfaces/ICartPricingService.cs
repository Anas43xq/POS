using BLL.DTOs;
using BLL.Models;
using System.Collections.Generic;

namespace BLL.Interfaces
{
    /// <summary>
    /// Cart-scoped service owning the two pieces of cart business logic
    /// that previously lived in <c>CashierDashboardViewModel</c>:
    /// the Subtotal/Tax/Total rollup, and the "does this addition merge
    /// with an existing line" decision. The ViewModel displays what this
    /// service computes; it does not compute totals or merge decisions
    /// itself.
    /// </summary>
    public interface ICartPricingService
    {
        /// <summary>
        /// Rolls up Subtotal/Tax/Total across all cart lines. Each line's
        /// own LineSubtotal/LineTax already account for its Quantity and
        /// TaxRate — this only sums across lines.
        /// </summary>
        CartTotalsDto CalculateTotals(IEnumerable<CartItem> items);

        /// <summary>
        /// Finds an existing cart line that a new addition of the same
        /// product + modifier selection should merge into (quantity
        /// increment) rather than becoming a separate line. Two lines are
        /// the same "configuration" when they share the same VariantId
        /// and an identical modifier selection (same option IDs and
        /// quantities) — not merely the same computed UnitPrice, which
        /// two genuinely different configurations can coincidentally
        /// share. Returns null when no existing line matches.
        /// </summary>
        CartItem? FindMergeableLine(
            IEnumerable<CartItem> items,
            int variantId,
            IReadOnlyList<CartItemModifier> modifiers);
    }
}
