using DAL.Entities;
using DAL.Entities.Data;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DAL.Repositories
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(IDbContextFactory<PosDbContext> contextFactory) : base(contextFactory)
        {
        }

        public async Task LogAsync(AuditLog log)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            await context.AuditLogs.AddAsync(log);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> QueryAsync(
            string? entityName = null,
            int? entityId = null,
            int? userId = null,
            int take = 100)
        {
            if (take < 1) take = 1;
            if (take > 1000) take = 1000;

            await using var context = await _contextFactory.CreateDbContextAsync();

            // Query shapes deliberately mirror IX_AuditLogs_Entity (EntityName, EntityId, OccurredAt DESC)
            // and IX_AuditLogs_UserId (UserId, OccurredAt DESC) so both filter paths hit a covering seek.
            var query = context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityName))
                query = query.Where(a => a.EntityName == entityName);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            return await query
                .OrderByDescending(a => a.OccurredAt)
                .Take(take)
                .ToListAsync();
        }
    }
}
