USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_VoidTransaction] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[SP_VoidTransaction]', N'P') IS NOT NULL
    DROP PROCEDURE [dbo].[SP_VoidTransaction];
GO

CREATE PROCEDURE [dbo].[SP_VoidTransaction]
(
	@TransactionId INT,
	@VoidReason NVARCHAR(500) = NULL
)
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE dbo.Transactions
	SET Status = 2,
		VoidReason = @VoidReason
	WHERE TransactionId = @TransactionId
		AND Status = 1;

	SELECT @@ROWCOUNT AS RowsUpdated;
END
GO
