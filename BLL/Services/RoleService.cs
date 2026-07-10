using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepo;

    public RoleService(IRoleRepository roleRepo)
    {
        _roleRepo = roleRepo;
    }

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var entities = await _roleRepo.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var entity = await _roleRepo.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task AddRoleAsync(RoleDto role) =>
        await _roleRepo.AddAsync(MapToEntity(role));

    public async Task UpdateRoleAsync(RoleDto role) =>
        await _roleRepo.UpdateAsync(MapToEntity(role));

    public async Task DeleteRoleAsync(int id) =>
        await _roleRepo.DeleteAsync(id);

    private static RoleDto MapToDto(Role e) => new()
    {
        RoleId = e.RoleId,
        RoleName = e.RoleName,
        Description = e.Description
    };

    private static Role MapToEntity(RoleDto d) => new()
    {
        RoleId = d.RoleId,
        RoleName = d.RoleName,
        Description = d.Description
    };
}