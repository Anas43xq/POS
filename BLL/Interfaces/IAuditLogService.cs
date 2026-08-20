using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLogDto>> GetAllAuditLogsAsync();

        Task<AuditLogDto?> GetAuditLogByIdAsync(int id);

        Task AddAuditLogAsync(AuditLogDto AuditLog);

        Task UpdateAuditLogAsync(AuditLogDto AuditLog);

        Task DeleteAuditLogAsync(int id);

        /// <summary>
        /// Writes one audit entry. <paramref name="oldValue"/> / <paramref name="newValue"/>
        /// are serialized to JSON snapshots. Pass null for oldValue on Create and
        /// null for newValue on Delete. Failures are logged and swallowed — audit
        /// writes must never fail the business operation they're attached to.
        /// </summary>
        /// <param name="actionType">"Create" | "Update" | "Delete" (free text, kept short).</param>
        /// <param name="entityName">Logical entity name, e.g. "User", "Product".</param>
        /// <param name="entityId">Primary key of the affected row, if known.</param>
        /// <param name="userId">Acting user, if known (null for system/unauthenticated actions).</param>
        /// <param name="oldValue">Snapshot before the change, or null.</param>
        /// <param name="newValue">Snapshot after the change, or null.</param>
        Task LogAsync(
            string actionType,
            string entityName,
            int? entityId,
            int? userId,
            object? oldValue,
            object? newValue);

        /// <summary>Audit trail for a single entity instance, newest first.</summary>
        Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, int entityId, int take = 100);

        /// <summary>Audit trail of everything a given user did, newest first.</summary>
        Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId, int take = 100);
    }
}
