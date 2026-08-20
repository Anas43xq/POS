using BLL.Interfaces;

namespace BLL.Services
{
    internal sealed class ManagerSessionCache : IManagerSessionCache
    {
        private readonly Dictionary<int, string?> _pinHashes = new();
        private readonly object _lock = new();

        public void StorePinHash(int userId, string? pinHash)
        {
            lock (_lock)
                _pinHashes[userId] = pinHash;
        }

        public string? GetPinHash(int userId)
        {
            lock (_lock)
                return _pinHashes.TryGetValue(userId, out var hash) ? hash : null;
        }

        public bool TryGetPinHash(int userId, out string? pinHash)
        {
            lock (_lock)
            {
                if (_pinHashes.TryGetValue(userId, out pinHash))
                    return true;          // present — null means confirmed no-PIN

                pinHash = null;
                return false;             // absent — never loaded
            }
        }

        public void Invalidate(int userId)
        {
            lock (_lock)
                _pinHashes.Remove(userId);
        }
    }
}
