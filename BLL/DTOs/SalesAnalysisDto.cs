namespace BLL.DTOs;

/// <summary>
/// Flat row in a sales analysis report (Category / Product / Size aggregate).
/// Grouping into the Category -> Product -> Size hierarchy happens during
/// report generation (Excel export), not in this DTO.
/// </summary>
public sealed class SalesAnalysisDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;

    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;

    public int SizeId { get; init; }
    public string SizeName { get; init; } = string.Empty;
    public int SizeDisplayOrder { get; init; }

    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
