using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using DAL.Interfaces;
using DAL.Entities;

namespace BLL.Services
{
    internal class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;

        public AuthService(IUserRepository userRepository, ISessionRepository sessionRepository)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<Result<UserDto>> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Result<UserDto>.Failure("Username or Password is Empty");
            }

            User? user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return Result<UserDto>.Failure("Invalid password/UserName");
            }

            if (!user.IsActive)
            {
                return Result<UserDto>.Failure("Account is deactivated");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return Result<UserDto>.Failure("Invalid password/UserName");
            }

            Session session = new Session
            {
                UserId = user.UserId,
                LoginAt = DateTime.Now
            };

            await _sessionRepository.AddAsync(session);

            var dto = new UserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Username = user.Username,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? string.Empty,
                IsActive = user.IsActive
            };

            return Result<UserDto>.Success(dto);
        }
    }
}