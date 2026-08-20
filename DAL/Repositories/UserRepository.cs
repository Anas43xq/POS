using DAL.Entities;
using DAL.Entities.Data;
using DAL.Infrastructure;
using DAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ISqlConnectionStringProvider _connectionStringProvider;

        public UserRepository(
            IDbContextFactory<PosDbContext> contextFactory,
            ISqlConnectionStringProvider connectionStringProvider)
            : base(contextFactory)
        {
            _connectionStringProvider = connectionStringProvider;
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

            await using var connection = new SqlConnection(_connectionStringProvider.ConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SP_LoginUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add("@Username", SqlDbType.NVarChar, 50).Value = normalizedUsername;

            await using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            int roleId = reader.GetInt32(reader.GetOrdinal("RoleId"));

            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                RoleId = roleId,
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                Role = new Role
                {
                    RoleId = roleId,
                    RoleName = reader.GetString(reader.GetOrdinal("RoleName"))
                }
            };
        }

        public async Task<User?> GetByIdWithRoleAsync(int id)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<string?> GetPinHashAsync(int userId)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .Select(u => u.PinHash)
                .FirstOrDefaultAsync();
        }

        public async Task UpdatePinHashAsync(int userId, string pinHash)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            await context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.PinHash, pinHash));
        }

        public async Task<User?> GetDefaultCashierAsync()
        {
            // Current policy: first active user whose role is "Cashier".
            // The method name is forward-compatible so a future
            // dedicated flag (e.g. IsDefaultCashier) can replace this
            // without changing callers.
            await using var context = await _contextFactory!.CreateDbContextAsync();
            var user = await context.Users
                .AsNoTracking()
                .Where(u => u.IsActive && u.Role != null && u.Role.RoleName == "Cashier")
                .OrderBy(u => u.UserId)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Username,
                    u.RoleId,
                    u.IsActive,
                    RoleName = u.Role == null ? string.Empty : u.Role.RoleName
                })
                .FirstOrDefaultAsync();

            return user is null
                ? null
                : new User
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Username = user.Username,
                    RoleId = user.RoleId,
                    IsActive = user.IsActive,
                    Role = new Role
                    {
                        RoleId = user.RoleId,
                        RoleName = user.RoleName
                    }
                };
        }
    }
}
