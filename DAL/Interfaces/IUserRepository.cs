using DAL.Entities;

namespace DAL.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool?> IsActiveUser(int id = -1);

        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Default cashier account (currently the first active user
        /// whose role is named <c>"Cashier"</c>). The name leaves
        /// room for the resolution policy to evolve (e.g. a
        /// dedicated <c>IsDefaultCashier</c> flag) without breaking
        /// callers.
        /// </summary>
        Task<User?> GetDefaultCashierAsync();
    }
}
