using System.Threading.Tasks;

namespace UI.Services
{
    /// <summary>
    /// Abstraction over the Windows Registry for persisting
    /// per-user UI preferences (e.g. the last successfully-used
    /// manager username).
    ///
    /// All operations are asynchronous and never throw — registry
    /// access can fail in locked-down environments (corporate
    /// GPOs, terminal-services profiles) and we never want that
    /// to break the login flow.
    /// </summary>
    public interface IRegistryService
    {
        /// <summary>Persists <paramref name="username"/> so the next login can pre-fill it.</summary>
        Task SaveRememberedUsernameAsync(string username);

        /// <summary>Reads the previously-saved username, or <c>null</c> if none / unavailable.</summary>
        Task<string?> GetRememberedUsernameAsync();

        /// <summary>Removes the remembered username. Reserved for future "Not you?" flows.</summary>
        Task ClearRememberedUsernameAsync();
    }
}
