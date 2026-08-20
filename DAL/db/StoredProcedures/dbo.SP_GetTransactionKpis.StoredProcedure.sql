USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetTransactionKpis]    Script Date: 05/07/2026 12:02:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GetTransactionKpis]
(
    @PeriodType NVARCHAR(10),   -- 'Today' | 'Week' | 'Month' | 'Custom'
    @FromDate   DATE = NULL,    -- required when @PeriodType = 'Custom'
    @ToDate     DATE = NULL     -- optional when @PeriodType = 'Custom'
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
        @StartDate  = @StartDate OUTPUT,
        @EndDate    = @EndDate OUTPUT;
END
ELSE
BEGIN
    RAISERROR('Invalid PeriodType. Expected Today, Week, Month, or Custom.', 16, 1);
    RETURN;
END

    -------------------------------------------------
    -- 2. Compute KPIs
    -------------------------------------------------
    SELECT
        ISNULL(SUM(t.GrandTotal), 0)                                            AS TotalPrice,
        CAST(ISNULL(ROUND(AVG(t.GrandTotal), 2), 0) AS DECIMAL(18,2)) AS TotalAverage,
        COUNT(DISTINCT t.TransactionId)                                         AS TotalOrders,
        ISNULL(SUM(CASE WHEN p.PaymentMethod = 'Cash' THEN t.GrandTotal END), 0) AS TotalCash,
        ISNULL(SUM(CASE WHEN p.PaymentMethod = 'Card' THEN t.GrandTotal END), 0) AS TotalCard
    FROM Transactions t
    LEFT OUTER JOIN Payments p 
        ON p.TransactionId = t.TransactionId
    WHERE t.TransactionDate >= @StartDate
      AND t.TransactionDate <  @EndDate
      AND t.Status = 1;
END
GO
