using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash)
        {
            await using var context = await _contextFactory!.CreateDbContextAsync();
            return await context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt =>
                    rt.TokenHash == tokenHash &&
                    rt.RevokedAt == null &&
                    rt.ExpiresAt > DateTime.UtcNow);
        }
    }
}
