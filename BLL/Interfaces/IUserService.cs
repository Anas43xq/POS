using DAL.Entities;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> GetUserByIdAsync(int id);

        Task AddUserAsync(User User);

        Task UpdateUserAsync(User User);

        Task DeleteUserAsync(int id);

        Task<bool?> IsActiveUser(int id = -1);

        /// <summary>
        /// Returns the default cashier account used by the Login-As
        /// window's cashier flow (no password required). Currently
        /// resolves to the first active user with role
        /// <c>"Cashier"</c>; the name is intentionally
        /// forward-compatible so the resolution policy can be
        /// extended later without breaking callers.
        /// </summary>
        Task<User?> GetDefaultCashierAsync();
    }
}
