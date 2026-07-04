namespace Contracts.Sales
{
    public class KpiDto
    {
        public decimal TotalSales { get; set; }
        public decimal AverageSale { get; set; }
        public decimal TotalCash { get; set; }
        public decimal TotalCard { get; set; }
        public int TotalOrders { get; set; }
    }
}
