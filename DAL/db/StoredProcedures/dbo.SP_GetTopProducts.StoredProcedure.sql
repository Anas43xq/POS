USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetTopProducts]    Script Date: 11/07/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GetTopProducts]
(
    @PeriodType NVARCHAR(10),   -- 'Today' | 'Week' | 'Month' | 'Custom'
    @FromDate   DATE = NULL,
    @ToDate     DATE = NULL,
    @TopCount   INT = 7
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
    -- 2. Guard inputs
    -------------------------------------------------
    IF @TopCount < 1 SET @TopCount = 7;

    -------------------------------------------------
    -- 3. Top products by revenue within date range
    -------------------------------------------------
    SELECT TOP (@TopCount)
        ti.ProductName,
        SUM(ti.LineTotal) AS TotalSales
    FROM TransactionItems ti
    INNER JOIN Transactions t
        ON ti.TransactionId = t.TransactionId
    WHERE t.TransactionDate >= @StartDate
      AND t.TransactionDate <  @EndDate
      AND t.Status = 1
    GROUP BY ti.ProductName
    ORDER BY TotalSales DESC, ti.ProductName ASC;
END
GO
