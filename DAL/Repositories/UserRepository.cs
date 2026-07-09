using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IDbContextFactory<PosDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        public async Task<bool?> IsActiveUser(int id) =>
            (await (await _contextFactory!.CreateDbContextAsync()).Users.FirstOrDefaultAsync(e => e.UserId == id))?.IsActive;

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var normalizedUsername = username?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedUsername))
            {
                return null;
            }

            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    u.Username != null &&
                    u.Username.Trim().ToLower() == normalizedUsername.ToLower());
        }

        public async Task<User?> GetDefaultCashierAsync()
        {
            // Current policy: first active user whose role is "Cashier".
            // The method name is forward-compatible so a future
            // dedicated flag (e.g. IsDefaultCashier) can replace this
            // without changing callers.
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName == "Cashier")
                .OrderBy(u => u.UserId)
                .FirstOrDefaultAsync();
        }
    }
}
