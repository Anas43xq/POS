namespace Contracts.Sales
{
    public class SalesSummaryResponse
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public List<SalesSummaryBucketResponse> Chart { get; set; } = new();
    }

    public class SalesSummaryBucketResponse
    {
        public string Label { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
    }
}
