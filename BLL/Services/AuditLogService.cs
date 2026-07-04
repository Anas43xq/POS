using DAL.Entities;
using DAL.Interfaces;
using BLL.Interfaces;

namespace BLL.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditlogrepo;

        public AuditLogService(IAuditLogRepository AuditLogRepo)
        {
            _auditlogrepo = AuditLogRepo;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAuditLogsAsync() =>
            await _auditlogrepo.GetAllAsync();

        public async Task<AuditLog?> GetAuditLogByIdAsync(int id) =>
            await _auditlogrepo.GetByIdAsync(id);

        public async Task AddAuditLogAsync(AuditLog AuditLog) =>
            await _auditlogrepo.AddAsync(AuditLog);

        public async Task UpdateAuditLogAsync(AuditLog AuditLog) =>
            await _auditlogrepo.UpdateAsync(AuditLog);

        public async Task DeleteAuditLogAsync(int id) =>
            await _auditlogrepo.DeleteAsync(id);
    }
}