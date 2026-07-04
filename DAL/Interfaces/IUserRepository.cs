using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool?> IsActiveUser(int id = -1);
        Task<User?> GetByUsernameAsync(string username);
    }
}
