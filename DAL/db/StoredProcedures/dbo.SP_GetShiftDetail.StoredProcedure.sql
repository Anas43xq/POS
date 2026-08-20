USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetShiftDetail]    Script Date: 12/07/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GetShiftDetail]
(
    @ShiftId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Result Set 1: Shift info + statistics
    SELECT
        s.ShiftId,
        u.FullName AS CashierName,
        s.OpenedAt,
        s.ClosedAt,
        CAST(DATEDIFF(SECOND, s.OpenedAt, ISNULL(s.ClosedAt, SYSDATETIME())) / 3600.0 AS DECIMAL(10,2)) AS DurationHours,
        s.OpeningCash,
        s.ClosingCash,
        s.ExpectedCash,
        s.CashDifference,
        CASE s.Status WHEN 0 THEN N'Closed' WHEN 1 THEN N'Open' ELSE N'Unknown' END AS StatusLabel,
        ISNULL(stats.TotalTransactions, 0) AS TotalTransactions,
        ISNULL(stats.CashTransactions, 0) AS CashTransactions,
        ISNULL(stats.CardTransactions, 0) AS CardTransactions,
        ISNULL(stats.CashSales, 0) AS CashSales,
        ISNULL(stats.CardSales, 0) AS CardSales,
        ISNULL(stats.TotalSales, 0) AS TotalSales,
        ISNULL(stats.RefundsCount, 0) AS RefundsCount
    FROM Shifts s
    LEFT OUTER JOIN Users u ON u.UserId = s.UserId
    OUTER APPLY (
        SELECT
            COUNT(DISTINCT t.TransactionId) AS TotalTransactions,
            COUNT(DISTINCT CASE WHEN p.PaymentMethod = 'Cash' THEN t.TransactionId END) AS CashTransactions,
            COUNT(DISTINCT CASE WHEN p.PaymentMethod = 'Card' THEN t.TransactionId END) AS CardTransactions,
            SUM(CASE WHEN p.PaymentMethod = 'Cash' THEN t.GrandTotal ELSE 0 END) AS CashSales,
            SUM(CASE WHEN p.PaymentMethod = 'Card' THEN t.GrandTotal ELSE 0 END) AS CardSales,
            SUM(t.GrandTotal) AS TotalSales,
            SUM(CASE WHEN t.Status = 2 THEN 1 ELSE 0 END) AS RefundsCount
        FROM Transactions t
        LEFT OUTER JOIN Payments p ON p.TransactionId = t.TransactionId
        WHERE t.ShiftId = s.ShiftId
    ) stats
    WHERE s.ShiftId = @ShiftId;

    -- Result Set 2: Transactions for this shift
    SELECT
        t.TransactionId,
        t.ReceiptNumber,
        t.TransactionDate,
        COALESCE(p.PaymentMethod, '') AS PaymentMethod,
        t.GrandTotal,
        CASE t.Status WHEN 0 THEN 'Pending' WHEN 1 THEN 'Completed' WHEN 2 THEN 'Voided' ELSE 'Unknown' END AS Status
    FROM Transactions t
    LEFT OUTER JOIN Payments p ON p.TransactionId = t.TransactionId
    WHERE t.ShiftId = @ShiftId
    ORDER BY t.TransactionDate DESC;
END
GO
