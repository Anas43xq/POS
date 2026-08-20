
using DAL.Entities;
namespace DAL.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        /// <summary>
        /// Convenience insert used by the audit writer — same as AddAsync but
        /// named for call-site clarity ("log this event") rather than the
        /// generic repository verb.
        /// </summary>
        Task LogAsync(AuditLog log);

        /// <summary>
        /// Filtered, paged, newest-first audit query. All filters are optional;
        /// passing none returns the most recent entries across the whole log.
        /// Backed by IX_AuditLogs_UserId / IX_AuditLogs_Entity (Pass 2).
        /// </summary>
        Task<IEnumerable<AuditLog>> QueryAsync(
            string? entityName = null,
            int? entityId = null,
            int? userId = null,
            int take = 100);
    }
}