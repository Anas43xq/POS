using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllRolesAsync();

    Task<Role?> GetRoleByIdAsync(int id);

    Task AddRoleAsync(Role role);

    Task UpdateRoleAsync(Role role);

    Task DeleteRoleAsync(int id);
}

