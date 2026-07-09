using System.Threading.Tasks;
using Microsoft.Win32;

namespace UI.Services
{
    /// <summary>
    /// Default <see cref="IRegistryService"/> implementation. Stores
    /// the remembered username under
    /// <c>HKCU\Software\PointOfSalePOS\RememberedUsername</c>.
    /// </summary>
    public sealed class RegistryService : IRegistryService
    {
        private const string KeyPath = @"Software\PointOfSalePOS";
        private const string ValueName = "RememberedUsername";

        public Task SaveRememberedUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Task.CompletedTask;

            // Registry calls are synchronous and very fast; running
            // them on the thread-pool keeps the UI responsive.
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.CurrentUser.CreateSubKey(KeyPath, writable: true);
                    key?.SetValue(ValueName, username.Trim(), RegistryValueKind.String);
                }
                catch
                {
                    // Swallow — registry persistence is best-effort.
                }
            });
        }

        public Task<string?> GetRememberedUsernameAsync()
        {
            return Task.Run<string?>(() =>
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(KeyPath, writable: false);
                    return key?.GetValue(ValueName) as string;
                }
                catch
                {
                    return null;
                }
            });
        }

        public Task ClearRememberedUsernameAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    using var key = Registry.CurrentUser.OpenSubKey(KeyPath, writable: true);
                    key?.DeleteValue(ValueName, throwOnMissingValue: false);
                }
                catch
                {
                    // Swallow — clearing is best-effort.
                }
            });
        }
    }
}
