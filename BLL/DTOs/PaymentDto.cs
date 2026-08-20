namespace BLL.DTOs;

public sealed class PaymentDto
{
    public int PaymentId { get; init; }

    public int TransactionId { get; init; }

    public string PaymentMethod { get; init; } = string.Empty;

    public decimal AmountTendered { get; init; }

    public decimal ChangeGiven { get; init; }

    public string? ReferenceNumber { get; init; }

    public DateTime PaidAt { get; init; }
}