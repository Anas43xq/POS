using DAL.Entities;

namespace BLL.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync();

        Task<AuditLog?> GetAuditLogByIdAsync(int id);

        Task AddAuditLogAsync(AuditLog AuditLog);

        Task UpdateAuditLogAsync(AuditLog AuditLog);

        Task DeleteAuditLogAsync(int id);
    }
}