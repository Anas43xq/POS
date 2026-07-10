using BLL.DTOs;

namespace BLL.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();

    Task<RoleDto?> GetRoleByIdAsync(int id);

    Task AddRoleAsync(RoleDto role);

    Task UpdateRoleAsync(RoleDto role);

    Task DeleteRoleAsync(int id);
}