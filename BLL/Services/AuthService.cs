using System.Security.Cryptography;
using BLL.DTOs;
using BLL.Diagnostics;
using BLL.Interfaces;
using BLL.Models;
using DAL.Interfaces;
using DAL.Entities;
using Microsoft.Extensions.Options;

namespace BLL.Services
{
    internal class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly RefreshTokenSettings _refreshTokenSettings;
        private readonly IPinService _pinService;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IOptions<RefreshTokenSettings> refreshTokenSettings,
            IPinService pinService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenSettings = refreshTokenSettings.Value;
            _pinService = pinService;
        }

        public async Task<Result<UserDto>> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Result<UserDto>.Failure("Username or Password is Empty");
            }

            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var userLookupStopwatch = System.Diagnostics.Stopwatch.StartNew();
            User? user = await _userRepository.GetByUsernameAsync(username);
            TxpTrace.WriteLine(
                $"[TXP] - Auth user lookup completed in {userLookupStopwatch.ElapsedMilliseconds} ms for {username}");
            if (user == null)
            {
                TxpTrace.WriteLine(
                    $"[TXP] - Auth total completed in {totalStopwatch.ElapsedMilliseconds} ms for {username} (user not found)");
                return Result<UserDto>.Failure("Invalid password/UserName");
            }

            if (!user.IsActive)
            {
                TxpTrace.WriteLine(
                    $"[TXP] - Auth total completed in {totalStopwatch.ElapsedMilliseconds} ms for {username} (inactive user)");
                return Result<UserDto>.Failure("Account is deactivated");
            }

            var verifyStopwatch = System.Diagnostics.Stopwatch.StartNew();
            if (!await Task.Run(() => BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)))
            {
                TxpTrace.WriteLine(
                    $"[TXP] - Auth bcrypt verify completed in {verifyStopwatch.ElapsedMilliseconds} ms for {username} (invalid)");
                TxpTrace.WriteLine(
                    $"[TXP] - Auth total completed in {totalStopwatch.ElapsedMilliseconds} ms for {username} (invalid password)");
                return Result<UserDto>.Failure("Invalid password/UserName");
            }

            TxpTrace.WriteLine(
                $"[TXP] - Auth bcrypt verify completed in {verifyStopwatch.ElapsedMilliseconds} ms for {username}");
            TxpTrace.WriteLine(
                $"[TXP] - Auth total completed in {totalStopwatch.ElapsedMilliseconds} ms for {username}");

            // Pre-load PIN hash into cache so override prompts never hit the DB
            _ = _pinService.HydrateCacheAsync(user.UserId);

            return Result<UserDto>.Success(MapToDto(user));
        }

        public async Task<string> IssueRefreshTokenAsync(int userId)
        {
            var rawToken = GenerateSecureToken();

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                TokenHash = HashToken(rawToken),
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenSettings.ExpiryDays)
            };

            await _refreshTokenRepository.AddAsync(refreshToken);

            return rawToken;
        }

        public async Task<Result<UserDto>> RefreshAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Result<UserDto>.Failure("Refresh token is required");
            }

            var stored = await _refreshTokenRepository.GetActiveByTokenHashAsync(HashToken(refreshToken));
            if (stored is null)
            {
                return Result<UserDto>.Failure("Refresh token is invalid or expired");
            }

            User? user = await _userRepository.GetByIdWithRoleAsync(stored.UserId);
            if (user is null || !user.IsActive)
            {
                return Result<UserDto>.Failure("Account is no longer active");
            }

            return Result<UserDto>.Success(MapToDto(user));
        }

        private static UserDto MapToDto(User user) => new()
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Username = user.Username,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName ?? string.Empty,
            IsActive = user.IsActive
        };

        private static string GenerateSecureToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

        private static string HashToken(string token) =>
            Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
    }
}
