namespace Contracts.Sales
{
    public class TopCategoryResponse
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int Quantity { get; set; }
    }
}
