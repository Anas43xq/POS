using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// GET api/auditlogs/entity/{entityName}/{entityId}
    /// Audit trail for a single record, newest first — e.g. /entity/Product/42
    /// </summary>
    [HttpGet("entity/{entityName}/{entityId:int}")]
    [Authorize]
    public async Task<IActionResult> GetByEntity(string entityName, int entityId, [FromQuery] int take = 100)
    {
        var logs = await _auditLogService.GetByEntityAsync(entityName, entityId, take);
        return Ok(logs.Select(MapToResponse));
    }

    /// <summary>
    /// GET api/auditlogs/user/{userId}
    /// Everything a given user did, newest first.
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] int take = 100)
    {
        var logs = await _auditLogService.GetByUserAsync(userId, take);
        return Ok(logs.Select(MapToResponse));
    }

    private static object MapToResponse(AuditLogDto a) => new
    {
        a.AuditLogId,
        a.UserId,
        a.ActionType,
        a.EntityName,
        a.EntityId,
        a.OldValue,
        a.NewValue,
        a.OccurredAt
    };
}
