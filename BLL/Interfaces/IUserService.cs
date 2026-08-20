using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();

        Task<UserDto?> GetUserByIdAsync(int id);

        Task AddUserAsync(UserDto User);

        Task UpdateUserAsync(UserDto User);

        Task DeleteUserAsync(int id);

        Task<bool?> IsActiveUser(int id = -1);

        /// <summary>
        /// Returns the default cashier account used by the Login-As
        /// window's cashier flow (no password required).
        /// </summary>
        Task<UserDto?> GetDefaultCashierAsync();
    }
}