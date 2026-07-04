using BLL.Interfaces;
using BLL.Models;
using DAL.Interfaces;
using DAL.Entities;
using Microsoft.Extensions.DependencyInjection;

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
        public async Task<Result<User>> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return Result<User>.Failure("Username or Password is Empty");
            }

            User? user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
            {
                return Result<User>.Failure("Invalid password/UserName");
            }

            if (!user.IsActive)
            {
                return Result<User>.Failure("Account is deactivated");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return Result<User>.Failure("Invalid password/UserName");
            }

            Session session = new Session
            {
                UserId = user.UserId,
                LoginAt = DateTime.Now
            };

            await _sessionRepository.AddAsync(session);
            return Result<User>.Success(user);
        }
    }
}
