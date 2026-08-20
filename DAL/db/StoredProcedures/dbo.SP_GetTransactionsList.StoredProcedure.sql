USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetTransactionsList]    Script Date: 05/07/2026 12:02:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GetTransactionsList]
(
    @PeriodType   NVARCHAR(10),    -- 'Today' | 'Week' | 'Month' | 'Custom'
    @FromDate     DATE = NULL,     -- required when @PeriodType = 'Custom'
    @ToDate       DATE = NULL,     -- optional when @PeriodType = 'Custom'
    @StatusFilter NVARCHAR(20) = NULL, -- 'Completed' | 'Voided' | 'Pending' | NULL = all
    @PageNumber   INT = 1,
    @PageSize     INT = 50
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME;
    DECLARE @EndDate   DATETIME; -- exclusive upper bound

    -------------------------------------------------
    -- 1. Resolve date range based on PeriodType
    -------------------------------------------------
 IF @PeriodType IN ('Custom', 'Today', 'Week', 'Month')
BEGIN
    EXEC dbo.SP_GetPeriodDateRange
        @PeriodType = @PeriodType,
        @FromDate = @FromDate,
        @ToDate = @ToDate,
        @StartDate  = @StartDate OUTPUT,
        @EndDate    = @EndDate OUTPUT;
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
    FROM Transactions t
    WHERE t.TransactionDate >= @StartDate
      AND t.TransactionDate <  @EndDate
      AND (@StatusFilter IS NULL
           OR t.Status = CASE @StatusFilter
                          WHEN 'Completed' THEN 1
                          WHEN 'Voided'    THEN 2
                          WHEN 'Pending'   THEN 0
                          ELSE t.Status END);

    -------------------------------------------------
    -- 4. Paged rows
    -------------------------------------------------
    SELECT
        t.TransactionId,
        t.ReceiptNumber,
        t.GrandTotal,
        t.Notes,
        COALESCE(p.PaymentMethod, '') AS PaymentMethod,
        CASE t.Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Voided' ELSE 'Unknown' END AS Status,
        t.TransactionDate
    FROM Transactions t
    LEFT OUTER JOIN Payments p
        ON p.TransactionId = t.TransactionId
    WHERE t.TransactionDate >= @StartDate
      AND t.TransactionDate <  @EndDate
      AND (@StatusFilter IS NULL
           OR t.Status = CASE @StatusFilter
                          WHEN 'Completed' THEN 1
                          WHEN 'Voided'    THEN 2
                          WHEN 'Pending'   THEN 0
                          ELSE t.Status END)
    ORDER BY t.TransactionDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
