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

        public int ModifierOptionId { get; set; }

        public string OptionName { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        public decimal PriceAdd { get; set; }

        public decimal LineTotal => Math.Round(PriceAdd * Quantity, 2, MidpointRounding.AwayFromZero);

        public int GroupType { get; set; }
    }
}