namespace BLL.DTOs;

/// <summary>
/// Read/write model for a size (shared master data).
/// Used by the Size management page.
/// </summary>
public sealed class SizeDto
{
    public int SizeId { get; init; }

    public string Name { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public bool IsActive { get; init; } = true;
}