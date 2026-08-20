namespace Contracts.Sales
{
    public class CategoryTopProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int Quantity { get; set; }
    }
}
