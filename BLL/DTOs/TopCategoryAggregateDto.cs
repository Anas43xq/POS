namespace BLL.DTOs;

public sealed class TopCategoryAggregateDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal TotalSales { get; init; }
    public int Quantity { get; init; }
}
