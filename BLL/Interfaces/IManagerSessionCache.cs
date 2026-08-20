namespace BLL.Interfaces
{
    /// <summary>
    /// In-memory singleton that holds the currently-logged-in manager's PIN hash.
    /// Avoids a DB round-trip on every override prompt — the hash is loaded once
    /// at login and kept until the session ends or the PIN is changed.
    /// </summary>
    public interface IManagerSessionCache
    {
        /// <summary>Stores the Argon2id PIN hash for <paramref name="userId"/> after a successful login.</summary>
        void StorePinHash(int userId, string? pinHash);

        /// <summary>Returns the cached PIN hash, or null if none is stored for <paramref name="userId"/>.</summary>
        string? GetPinHash(int userId);

        /// <summary>
        /// Returns true if an entry exists for <paramref name="userId"/> (even if the stored
        /// hash is null, meaning the user is confirmed to have no PIN).
        /// Returns false when the user's state has never been loaded into the cache.
        /// </summary>
        bool TryGetPinHash(int userId, out string? pinHash);

        /// <summary>Removes all cached state for <paramref name="userId"/> (called on logout).</summary>
        void Invalidate(int userId);
    }
}
