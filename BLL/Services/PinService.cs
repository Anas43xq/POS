using BLL.Interfaces;
using BLL.Models;
using DAL.Interfaces;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BLL.Services
{
    internal sealed class PinService : IPinService
    {
        // Argon2id parameters — tuned for fast interactive verify (~50–80 ms)
        // while still being meaningfully resistant to brute force on the DB.
        // The UI rate-limits attempts, so speed here is fine for a local POS.
        private const int Iterations   = 2;
        private const int MemorySize   = 65536; // 64 MB
        private const int DegreeOfParallelism = 1;
        private const int HashLength   = 32;
        private const int SaltLength   = 16;

        private static readonly Regex PinFormat = new(@"^\d{4}$", RegexOptions.Compiled);

        private readonly IUserRepository _userRepository;
        private readonly IManagerSessionCache _cache;

        public PinService(IUserRepository userRepository, IManagerSessionCache cache)
        {
            _userRepository = userRepository;
            _cache = cache;
        }

        public async Task<Result<bool>> SetPinAsync(int userId, string pin)
        {
            if (string.IsNullOrWhiteSpace(pin) || !PinFormat.IsMatch(pin))
                return Result<bool>.Failure("PIN must be exactly 4 numeric digits.");

            var hash = await Task.Run(() => HashPin(pin));
            await _userRepository.UpdatePinHashAsync(userId, hash);
            _cache.StorePinHash(userId, hash);

            return Result<bool>.Success(true);
        }

        public async Task<bool> VerifyPinAsync(int userId, string pin)
        {
            if (string.IsNullOrWhiteSpace(pin)) return false;

            if (_cache.TryGetPinHash(userId, out var hash))
            {
                // Cache hit — null means confirmed no PIN; non-null means verify against it
                if (hash is null) return false;
                return await Task.Run(() => VerifyPin(pin, hash));
            }

            // Cache miss — load from DB and store result (null = no PIN, string = hash)
            hash = await _userRepository.GetPinHashAsync(userId);
            _cache.StorePinHash(userId, hash);     // stores null for no-PIN — future calls short-circuit
            if (hash is null) return false;
            return await Task.Run(() => VerifyPin(pin, hash));
        }

        public async Task HydrateCacheAsync(int userId)
        {
            var hash = await _userRepository.GetPinHashAsync(userId);
            _cache.StorePinHash(userId, hash);
        }

        public async Task<bool> HasPinAsync(int userId)
        {
            if (_cache.TryGetPinHash(userId, out var hash))
                return hash is not null;

            var dbHash = await _userRepository.GetPinHashAsync(userId);
            _cache.StorePinHash(userId, dbHash);  // cache result so re-opens don't re-query
            return dbHash is not null;
        }

        // ── Argon2id helpers ──────────────────────────────────────────────

        private static string HashPin(string pin)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltLength);
            var hash = ComputeArgon2id(Encoding.UTF8.GetBytes(pin), salt);

            // Format: base64(salt):base64(hash)
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPin(string pin, string stored)
        {
            var parts = stored.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt, expectedHash;
            try
            {
                salt         = Convert.FromBase64String(parts[0]);
                expectedHash = Convert.FromBase64String(parts[1]);
            }
            catch
            {
                return false;
            }

            var actualHash = ComputeArgon2id(Encoding.UTF8.GetBytes(pin), salt);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        private static byte[] ComputeArgon2id(byte[] password, byte[] salt)
        {
            using var argon2 = new Argon2id(password)
            {
                Salt                  = salt,
                Iterations            = Iterations,
                MemorySize            = MemorySize,
                DegreeOfParallelism   = DegreeOfParallelism
            };
            return argon2.GetBytes(HashLength);
        }
    }
}
