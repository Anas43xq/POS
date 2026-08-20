using System.Text.Json;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class AuditLogService : IAuditLogService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = false
        };

        private readonly IAuditLogRepository _auditlogrepo;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IAuditLogRepository AuditLogRepo, ILogger<AuditLogService> logger)
        {
            _auditlogrepo = AuditLogRepo;
            _logger = logger;
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

        public async Task LogAsync(
            string actionType,
            string entityName,
            int? entityId,
            int? userId,
            object? oldValue,
            object? newValue)
        {
            try
            {
                var entry = new AuditLog
                {
                    ActionType = actionType,
                    EntityName = entityName,
                    EntityId = entityId,
                    UserId = userId,
                    OldValue = Serialize(oldValue),
                    NewValue = Serialize(newValue),
                    OccurredAt = DateTime.UtcNow
                };

                await _auditlogrepo.LogAsync(entry);
            }
            catch (Exception ex)
            {
                // Audit logging is best-effort: a failure here (e.g. a transient DB
                // hiccup) must never roll back or fail the business operation that
                // triggered it. Log and move on.
                _logger.LogError(ex,
                    "Failed to write audit log for {ActionType} {EntityName} (EntityId={EntityId}, UserId={UserId})",
                    actionType, entityName, entityId, userId);
            }
        }

        public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, int entityId, int take = 100)
        {
            var entities = await _auditlogrepo.QueryAsync(entityName: entityName, entityId: entityId, take: take);
            return entities.Select(MapToDto);
        }

        public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId, int take = 100)
        {
            var entities = await _auditlogrepo.QueryAsync(userId: userId, take: take);
            return entities.Select(MapToDto);
        }

        private static string? Serialize(object? value)
        {
            if (value is null)
                return null;

            if (value is string s)
                return s;

            try
            {
                return JsonSerializer.Serialize(value, SerializerOptions);
            }
            catch (NotSupportedException)
            {
                return value.ToString();
            }
        }

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
