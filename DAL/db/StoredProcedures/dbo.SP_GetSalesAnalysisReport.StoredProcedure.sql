USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetSalesAnalysisReport]    Script Date: 08/03/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GetSalesAnalysisReport]
(
    @PeriodType NVARCHAR(10),
    @FromDate   DATE = NULL,
    @ToDate     DATE = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StartDate DATETIME;
    DECLARE @EndDate   DATETIME;

    -------------------------------------------------
    -- 1. PERIOD LOGIC (IDENTICAL STYLE)
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
    -- 2. SALES ANALYSIS DATA
    -- Flat rows aggregated per Category / Product / Size.
    -- Hierarchy (Category -> Product -> Size) and totals
    -- are built by the caller during report generation.
    -------------------------------------------------
    SELECT
        c.CategoryId,
        c.Name           AS CategoryName,
        pr.ProductId,
        pr.Name          AS ProductName,
        s.SizeId,
        s.Name           AS SizeName,
        s.DisplayOrder   AS SizeDisplayOrder,
        SUM(ti.Quantity)  AS Quantity,
        SUM(ti.LineTotal) AS LineTotal
    FROM Transactions t
    INNER JOIN TransactionItems ti
        ON t.TransactionId = ti.TransactionId
    INNER JOIN ProductVariants pv
        ON ti.VariantId = pv.VariantId
    INNER JOIN Products pr
        ON pv.ProductId = pr.ProductId
    INNER JOIN Categories c
        ON pr.CategoryId = c.CategoryId
    INNER JOIN Sizes s
        ON pv.SizeId = s.SizeId
    WHERE
        t.TransactionDate >= @StartDate
        AND t.TransactionDate <  @EndDate
    GROUP BY
        c.CategoryId, c.Name,
        pr.ProductId, pr.Name,
        s.SizeId, s.Name, s.DisplayOrder
    ORDER BY
        c.Name ASC,
        pr.Name ASC,
        s.DisplayOrder ASC;
END
GO
