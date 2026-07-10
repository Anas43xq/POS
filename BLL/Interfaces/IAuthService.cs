using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        Task<Result<UserDto>> LoginAsync(string username, string password);
    }
}