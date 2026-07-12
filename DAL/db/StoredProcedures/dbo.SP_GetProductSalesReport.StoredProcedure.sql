USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetProductSalesReport]    Script Date: 05/07/2026 12:02:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GetProductSalesReport]
(
    @PeriodType NVARCHAR(10),
    @FromDate   DATE = NULL,
    @ToDate     DATE = NULL,
    @ProductId  INT = NULL
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
    -- 2. PRODUCT SALES DATA
    -- When @ProductId is NULL, return all products.
    -------------------------------------------------
    SELECT
        t.ReceiptNumber,
        t.TransactionDate,
        p.PaymentMethod,
        ti.Quantity,
        ti.LineTotal
    FROM Transactions t
    INNER JOIN TransactionItems ti
        ON t.TransactionId = ti.TransactionId
    INNER JOIN ProductVariants pv
        ON ti.VariantId = pv.VariantId
        INNER JOIN Payments p
        ON p.TransactionId = t.TransactionId
    WHERE
        t.TransactionDate >= @StartDate
        AND t.TransactionDate <  @EndDate
        AND (@ProductId IS NULL OR pv.ProductId = @ProductId)
    ORDER BY
        t.TransactionDate ASC;
END
GO
