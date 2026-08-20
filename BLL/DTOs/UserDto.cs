namespace BLL.DTOs;

/// <summary>
/// Lightweight projection of a user, excluding sensitive fields like password hash.
/// </summary>
public sealed class UserDto
{
    public int UserId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Username { get; init; } = string.Empty;

    public int RoleId { get; init; }

    public string RoleName { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}