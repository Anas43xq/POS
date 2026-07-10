namespace BLL.DTOs;

/// <summary>
/// Projection of a tax rate for display and selection.
/// </summary>
public sealed class TaxRateDto
{
    public int TaxRateId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Rate { get; init; }

    public override string ToString() => $"{Name} ({Rate:P0})";
}