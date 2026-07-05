namespace POS.Contracts.Receipts;

public class SupplierDto
{
    public int SupplierId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? TRN { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
