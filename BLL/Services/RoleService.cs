using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;

    public  class RoleService : IRoleService
    {
    private readonly IRoleRepository _roleRepo;

    public RoleService(IRoleRepository roleRepo)
    {
        _roleRepo = roleRepo;
    }
    public async Task<IEnumerable<Role>> GetAllRolesAsync() =>

        await _roleRepo.GetAllAsync();

    public async Task<Role?> GetRoleByIdAsync(int id) =>

        await _roleRepo.GetByIdAsync(id);

    public async Task AddRoleAsync(Role role) =>

        await _roleRepo.AddAsync(role);

    public async Task UpdateRoleAsync(Role role) =>

        await _roleRepo.UpdateAsync(role);

    public async Task DeleteRoleAsync(int id) =>

        await _roleRepo.DeleteAsync(id);
}

