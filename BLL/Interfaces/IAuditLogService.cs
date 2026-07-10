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
    }
}