namespace BLL.DTOs
{
    /// <summary>
    /// Cart-level rollup totals computed by <see cref="Interfaces.ICartPricingService"/>
    /// from the individual cart lines. Read-only from the UI's perspective —
    /// the ViewModel displays these, it does not compute them.
    /// </summary>
    public class CartTotalsDto
    {
        public decimal Subtotal { get; set; }

        public decimal Tax { get; set; }

        public decimal Total { get; set; }
    }
}
