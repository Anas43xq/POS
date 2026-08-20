USE [POS_DB]
GO

/* ============================================================
   Scalar function: returns SUM(GrandTotal) for a given ShiftId
   where transactions are Completed (Status = 1).
   ============================================================ */
CREATE OR ALTER FUNCTION dbo.FN_GetShiftTotalSales
(
    @ShiftId INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @Total DECIMAL(18,2);

    SELECT @Total = ISNULL(SUM(GrandTotal), 0)
    FROM dbo.Transactions
    WHERE ShiftId   = @ShiftId
      AND Status    = 1;   -- 1 = Completed (excludes Voided)

    RETURN @Total;
END
GO