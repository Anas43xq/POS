using Contracts.Enum;

namespace BLL.DTOs;

/// <summary>
/// Projection of a cashier shift (open/close).
/// </summary>
public sealed class ShiftDto
{
    public int ShiftId { get; init; }

    public int UserId { get; init; }

    public DateTime OpenedAt { get; init; }

    public DateTime? ClosedAt { get; init; }

    public decimal OpeningCash { get; init; }

    public decimal? ClosingCash { get; init; }

    public decimal? ExpectedCash { get; init; }

    public decimal? CashDifference { get; init; }

    public ShiftStatus Status { get; init; }
}