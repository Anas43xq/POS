namespace BLL.DTOs;

public sealed class SalesChartBucketDto
{
    public string Label { get; init; } = string.Empty;
    public decimal TotalSales { get; init; }
}
