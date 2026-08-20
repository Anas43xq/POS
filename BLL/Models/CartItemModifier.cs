namespace BLL.Models
{
    /// <summary>
    /// Represents a modifier selection applied to a cart item.
    /// Operates purely on DTOs — knows nothing about EF entities.
    /// </summary>
    public class CartItemModifier
    {
        public int ModifierGroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// Canonical (English) group name. Used when persisting the
        /// selection (transaction/receipt data) so receipts always print
        /// English regardless of the active cashier UI language, mirroring
        /// how <c>CartItem.ProductName</c> is stamped from the product's
        /// EnglishDisplayName.
        /// </summary>
        public string EnglishGroupName { get; set; } = string.Empty;

        public int ModifierOptionId { get; set; }

        public string OptionName { get; set; } = string.Empty;

        /// <summary>
        /// Canonical (English) option name. Used when persisting the
        /// selection (transaction/receipt data) so receipts always print
        /// English regardless of the active cashier UI language.
        /// </summary>
        public string EnglishOptionName { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        public decimal PriceAdd { get; set; }

        public decimal LineTotal => Math.Round(PriceAdd * Quantity, 2, MidpointRounding.AwayFromZero);

        public int GroupType { get; set; }

        /// <summary>
        /// Whether this option is the default selection for its group
        /// (e.g. "Regular Dough" for "Dough Thickness").
        /// Default options are excluded from receipt display.
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
