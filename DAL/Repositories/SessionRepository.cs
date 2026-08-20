using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories
{
    public class SessionRepository : Repository<Session>, ISessionRepository
    {
        public SessionRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
        {
        }
    }
}

