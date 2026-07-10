namespace BLL.DTOs;

/// <summary>
/// Projection of an audit log entry.
/// </summary>
public sealed class AuditLogDto
{
    public int AuditLogId { get; init; }

    public int? UserId { get; init; }

    public string ActionType { get; init; } = string.Empty;

    public string? EntityName { get; init; }

    public int? EntityId { get; init; }

    public string? OldValue { get; init; }

    public string? NewValue { get; init; }

    public DateTime OccurredAt { get; init; }
}