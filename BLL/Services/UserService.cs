using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var entities = await _userRepo.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var entity = await _userRepo.GetByIdAsync(id);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task AddUserAsync(UserDto user) =>
            await _userRepo.AddAsync(MapToEntity(user));

        public async Task UpdateUserAsync(UserDto user) =>
            await _userRepo.UpdateAsync(MapToEntity(user));

        public async Task DeleteUserAsync(int id) =>
            await _userRepo.DeleteAsync(id);

        public async Task<bool?> IsActiveUser(int id) =>
            await _userRepo.IsActiveUser(id);

        public async Task<UserDto?> GetDefaultCashierAsync()
        {
            var entity = await _userRepo.GetDefaultCashierAsync();
            return entity is null ? null : MapToDto(entity);
        }

        private static UserDto MapToDto(User e) => new()
        {
            UserId = e.UserId,
            FullName = e.FullName,
            Username = e.Username,
            RoleId = e.RoleId,
            RoleName = e.Role?.RoleName ?? string.Empty,
            IsActive = e.IsActive
        };

        private static User MapToEntity(UserDto d) => new()
        {
            UserId = d.UserId,
            FullName = d.FullName,
            Username = d.Username,
            RoleId = d.RoleId,
            IsActive = d.IsActive
        };
    }
}