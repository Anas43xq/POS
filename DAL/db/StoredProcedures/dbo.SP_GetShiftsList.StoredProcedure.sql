USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetShiftsList]    Script Date: 12/07/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GetShiftsList]
(
    @PeriodType   NVARCHAR(10),       -- 'Today' | 'Week' | 'Month' | 'Custom'
    @FromDate     DATE = NULL,
    @ToDate       DATE = NULL,
    @StatusFilter NVARCHAR(10) = NULL, -- 'Open' | 'Closed' | NULL (all)
    @CashierId    INT = NULL,
    @PageNumber   INT = 1,
    @PageSize     INT = 50
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME;
    DECLARE @EndDate   DATETIME;

    -------------------------------------------------
    -- 1. Resolve date range based on PeriodType
    -------------------------------------------------
    IF @PeriodType IN ('Custom', 'Today', 'Week', 'Month')
    BEGIN
        EXEC dbo.SP_GetPeriodDateRange
            @PeriodType = @PeriodType,
            @FromDate = @FromDate,
            @ToDate = @ToDate,
            @StartDate = @StartDate OUTPUT,
            @EndDate = @EndDate OUTPUT;
    END
    ELSE
    BEGIN
        RAISERROR('Invalid PeriodType. Expected Today, Week, Month, or Custom.', 16, 1);
        RETURN;
    END

    -------------------------------------------------
    -- 2. Guard pagination inputs
    -------------------------------------------------
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize   < 1 SET @PageSize   = 50;

    -------------------------------------------------
    -- 3. Total count (for paging UI)
    -------------------------------------------------
    SELECT COUNT(*) AS TotalCount
    FROM Shifts s
    WHERE s.OpenedAt >= @StartDate
      AND s.OpenedAt <  @EndDate
      AND (@StatusFilter IS NULL
           OR s.Status = CASE @StatusFilter
                          WHEN 'Open'   THEN 1
                          WHEN 'Closed' THEN 0
                          ELSE s.Status END)
      AND (@CashierId IS NULL OR s.UserId = @CashierId);

    -------------------------------------------------
    -- 4. Paged rows
    -------------------------------------------------
    SELECT
        s.ShiftId,
        s.UserId,
        u.FullName AS CashierName,
        s.OpenedAt,
        s.ClosedAt,
        s.OpeningCash,
        s.ClosingCash,
        s.ExpectedCash,
        s.CashDifference,
        s.Status,
        CASE s.Status WHEN 0 THEN N'Closed' WHEN 1 THEN N'Open' ELSE N'Unknown' END AS StatusLabel,
        CAST(DATEDIFF(SECOND, s.OpenedAt, ISNULL(s.ClosedAt, SYSDATETIME())) / 3600.0 AS DECIMAL(10,2)) AS DurationHours
    FROM Shifts s
    LEFT OUTER JOIN Users u ON u.UserId = s.UserId
    WHERE s.OpenedAt >= @StartDate
      AND s.OpenedAt <  @EndDate
      AND (@StatusFilter IS NULL
           OR s.Status = CASE @StatusFilter
                          WHEN 'Open'   THEN 1
                          WHEN 'Closed' THEN 0
                          ELSE s.Status END)
      AND (@CashierId IS NULL OR s.UserId = @CashierId)
    ORDER BY s.OpenedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
