using DAL.Entities;

namespace DAL.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        /// <summary>Returns the matching refresh token only if it isn't revoked and hasn't expired.</summary>
        Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash);
    }
}
