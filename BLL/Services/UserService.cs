using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IAuditLogService _auditLogService;
        private readonly ISessionService _sessionService;

        public UserService(IUserRepository userRepo, IAuditLogService auditLogService, ISessionService sessionService)
        {
            _userRepo = userRepo;
            _auditLogService = auditLogService;
            _sessionService = sessionService;
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

        public async Task AddUserAsync(UserDto user)
        {
            var entity = MapToEntity(user);
            await _userRepo.AddAsync(entity);
            await _auditLogService.LogAsync("Create", "User", entity.UserId, _sessionService.CurrentUser?.UserId,
                oldValue: null, newValue: MapToDto(entity));
        }

        public async Task UpdateUserAsync(UserDto user)
        {
            var before = await _userRepo.GetByIdAsync(user.UserId);
            await _userRepo.UpdateAsync(MapToEntity(user));
            await _auditLogService.LogAsync("Update", "User", user.UserId, _sessionService.CurrentUser?.UserId,
                oldValue: before is null ? null : MapToDto(before), newValue: user);
        }

        public async Task DeleteUserAsync(int id)
        {
            var before = await _userRepo.GetByIdAsync(id);
            await _userRepo.DeleteAsync(id);
            await _auditLogService.LogAsync("Delete", "User", id, _sessionService.CurrentUser?.UserId,
                oldValue: before is null ? null : MapToDto(before), newValue: null);
        }

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