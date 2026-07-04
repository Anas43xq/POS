using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync() =>

            await _userRepo.GetAllAsync();

        public async Task<User?> GetUserByIdAsync(int id) =>

            await _userRepo.GetByIdAsync(id);

        public async Task AddUserAsync(User User) =>

            await _userRepo.AddAsync(User);

        public async Task UpdateUserAsync(User User) =>

            await _userRepo.UpdateAsync(User);

        public async Task DeleteUserAsync(int id) =>

            await _userRepo.DeleteAsync(id);

        public async Task<bool?> IsActiveUser(int id)
        {
            return await _userRepo.IsActiveUser(id);
        }
    }
}
