using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
        {
            
        }

        public async Task<bool?> IsActiveUser(int id) =>
            (await (await _contextFactory!.CreateDbContextAsync()).Users.FirstOrDefaultAsync(e => e.UserId == id))?.IsActive;

        public async Task<User?> GetByUsernameAsync(string username)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(
                    u => u.Username == username);
        }
    }
}

