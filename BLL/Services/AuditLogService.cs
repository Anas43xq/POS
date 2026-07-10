using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditlogrepo;

        public AuditLogService(IAuditLogRepository AuditLogRepo)
        {
            _auditlogrepo = AuditLogRepo;
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllAuditLogsAsync()
        {
            var entities = await _auditlogrepo.GetAllAsync();
            return entities.Select(MapToDto);
        }

        public async Task<AuditLogDto?> GetAuditLogByIdAsync(int id)
        {
            var entity = await _auditlogrepo.GetByIdAsync(id);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task AddAuditLogAsync(AuditLogDto auditLog) =>
            await _auditlogrepo.AddAsync(MapToEntity(auditLog));

        public async Task UpdateAuditLogAsync(AuditLogDto auditLog) =>
            await _auditlogrepo.UpdateAsync(MapToEntity(auditLog));

        public async Task DeleteAuditLogAsync(int id) =>
            await _auditlogrepo.DeleteAsync(id);

        private static AuditLogDto MapToDto(AuditLog e) => new()
        {
            AuditLogId = e.AuditLogId,
            UserId = e.UserId,
            ActionType = e.ActionType,
            EntityName = e.EntityName,
            EntityId = e.EntityId,
            OldValue = e.OldValue,
            NewValue = e.NewValue,
            OccurredAt = e.OccurredAt
        };

        private static AuditLog MapToEntity(AuditLogDto d) => new()
        {
            AuditLogId = d.AuditLogId,
            UserId = d.UserId,
            ActionType = d.ActionType,
            EntityName = d.EntityName,
            EntityId = d.EntityId,
            OldValue = d.OldValue,
            NewValue = d.NewValue,
            OccurredAt = d.OccurredAt
        };
    }
}