namespace BLL.DTOs;

public sealed class TopProductAggregateDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal TotalSales { get; init; }
    public int Quantity { get; init; }
}
