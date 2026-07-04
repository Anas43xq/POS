using DAL.Entities;

namespace BLL.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> GetUserByIdAsync(int id);

        Task AddUserAsync(User User);

        Task UpdateUserAsync(User User);

        Task DeleteUserAsync(int id);

        Task<bool?> IsActiveUser(int id = -1);
    }
}
