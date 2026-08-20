using BLL.Models;

namespace BLL.Interfaces
{
    /// <summary>
    /// Manages the manager's 4-digit override PIN —
    /// hashing, persistence, and verification via the in-memory cache.
    /// </summary>
    public interface IPinService
    {
        /// <summary>
        /// Hashes <paramref name="pin"/> with Argon2id, saves it to the DB,
        /// and refreshes the in-memory cache for <paramref name="userId"/>.
        /// Returns a failure result if <paramref name="pin"/> is not exactly
        /// 4 numeric digits.
        /// </summary>
        Task<Result<bool>> SetPinAsync(int userId, string pin);

        /// <summary>
        /// Verifies <paramref name="pin"/> against the cached (or DB-fetched)
        /// Argon2id hash for <paramref name="userId"/>.
        /// Returns false if no PIN has been set.
        /// </summary>
        Task<bool> VerifyPinAsync(int userId, string pin);

        /// <summary>
        /// Loads the PIN hash from the DB into the in-memory cache.
        /// Called once after a successful manager login so subsequent
        /// override prompts never hit the database.
        /// </summary>
        Task HydrateCacheAsync(int userId);

        /// <summary>Returns true if the user currently has a PIN set.</summary>
        Task<bool> HasPinAsync(int userId);
    }
}
