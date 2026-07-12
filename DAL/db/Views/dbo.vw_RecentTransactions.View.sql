USE [POS_DB]
GO
/****** Object:  View [dbo].[vw_RecentTransactions]    Script Date: 05/07/2026 12:02:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_RecentTransactions]
AS
SELECT dbo.Transactions.TransactionId, dbo.Transactions.ReceiptNumber, dbo.Transactions.GrandTotal, COALESCE(dbo.Payments.PaymentMethod, '') AS PaymentMethod, dbo.Transactions.TransactionDate, dbo.Users.FullName AS CashierName, dbo.Transactions.Status
FROM     dbo.Transactions
    LEFT OUTER JOIN dbo.Payments ON dbo.Payments.TransactionId = dbo.Transactions.TransactionId
    INNER JOIN dbo.Users ON dbo.Transactions.CashierId = dbo.Users.UserId
GO
