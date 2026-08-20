using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        Task<Result<UserDto>> LoginAsync(string username, string password);

        /// <summary>Issues and persists a new refresh token for the given user, returning the raw (unhashed) value.</summary>
        Task<string> IssueRefreshTokenAsync(int userId);

        /// <summary>Validates a refresh token and returns the user it belongs to if it's still active.</summary>
        Task<Result<UserDto>> RefreshAsync(string refreshToken);
    }
}