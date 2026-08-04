namespace DAL.Entities
{
    public class SalesAnalysisEntity
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int SizeId { get; set; }
        public string SizeName { get; set; } = string.Empty;
        public int SizeDisplayOrder { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
