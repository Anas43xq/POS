USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetAuditLogs]    Script Date: 20/08/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================================
-- SP_GetAuditLogs
--
-- Paged, newest-first audit trail query. All filters are optional:
--   * @EntityName + @EntityId  -> "what happened to record X?"      (IX_AuditLogs_Entity)
--   * @UserId                  -> "what did user X do?"             (IX_AuditLogs_UserId)
--   * none of the above        -> most recent activity across the whole log
--
-- Mirrors the EF-side DAL.Repositories.AuditLogRepository.QueryAsync,
-- provided for reporting/ad-hoc tooling that talks to SQL directly
-- (e.g. SSMS, a future admin report) without going through the app tier.
-- ============================================================
CREATE PROCEDURE [dbo].[SP_GetAuditLogs]
(
    @EntityName NVARCHAR(100) = NULL,
    @EntityId   INT           = NULL,
    @UserId     INT           = NULL,
    @PageNumber INT           = 1,
    @PageSize   INT           = 100
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize   < 1 SET @PageSize   = 100;
    IF @PageSize   > 1000 SET @PageSize = 1000;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -------------------------------------------------
    -- Total count (for paging UI)
    -------------------------------------------------
    SELECT COUNT(*) AS TotalCount
    FROM dbo.AuditLogs a
    WHERE (@EntityName IS NULL OR a.EntityName = @EntityName)
      AND (@EntityId   IS NULL OR a.EntityId   = @EntityId)
      AND (@UserId     IS NULL OR a.UserId     = @UserId);

    -------------------------------------------------
    -- Page of results, newest first
    -------------------------------------------------
    SELECT
        a.AuditLogId,
        a.UserId,
        u.Username,
        a.ActionType,
        a.EntityName,
        a.EntityId,
        a.OldValue,
        a.NewValue,
        a.OccurredAt
    FROM dbo.AuditLogs a
    LEFT JOIN dbo.Users u ON u.UserId = a.UserId
    WHERE (@EntityName IS NULL OR a.EntityName = @EntityName)
      AND (@EntityId   IS NULL OR a.EntityId   = @EntityId)
      AND (@UserId     IS NULL OR a.UserId     = @UserId)
    ORDER BY a.OccurredAt DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END
