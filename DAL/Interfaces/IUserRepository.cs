using DAL.Entities;

namespace DAL.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool?> IsActiveUser(int id = -1);

        Task<User?> GetByUsernameAsync(string username);

        /// <summary>Same shape as <see cref="GetByUsernameAsync"/> but keyed by id, with <c>Role</c> included.</summary>
        Task<User?> GetByIdWithRoleAsync(int id);

        /// <summary>
        /// Default cashier account (currently the first active user
        /// whose role is named <c>"Cashier"</c>). The name leaves
        /// room for the resolution policy to evolve (e.g. a
        /// dedicated <c>IsDefaultCashier</c> flag) without breaking
        /// callers.
        /// </summary>
        Task<User?> GetDefaultCashierAsync();

        /// <summary>Returns the stored Argon2id PIN hash for the given user, or null if no PIN has been set.</summary>
        Task<string?> GetPinHashAsync(int userId);

        /// <summary>Persists a new Argon2id PIN hash for the given user.</summary>
        Task UpdatePinHashAsync(int userId, string pinHash);
    }
}
