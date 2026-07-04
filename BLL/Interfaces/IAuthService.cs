using BLL.Models;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        Task<Result<User>> LoginAsync(string username, string password);

    }
}
