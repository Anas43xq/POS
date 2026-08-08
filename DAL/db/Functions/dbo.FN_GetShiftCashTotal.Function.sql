USE [POS_DB]
GO

/* ============================================================
   Scalar function: returns SUM(AmountTendered) for Cash payments
   belonging to transactions in a given ShiftId.

   Mirrors the semantics ShiftService.CloseShiftAsync used to compute
   in memory: it does NOT filter by transaction Status, so Voided
   transactions' cash payments are still included, exactly as before.
   This function only replaces the *mechanism* (a DB-side aggregate
   instead of loading every row in Payments/Transactions and filtering
   client-side) — it is not a behavior change to what counts as
   "expected cash". If that in-memory-preserved behavior is not
   actually what's wanted (i.e. Voided transactions should NOT count
   toward expected cash), that's a separate, deliberate fix.
   ============================================================ */
CREATE OR ALTER FUNCTION dbo.FN_GetShiftCashTotal
(
    @ShiftId INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @Total DECIMAL(18,2);

    SELECT @Total = ISNULL(SUM(p.AmountTendered), 0)
    FROM dbo.Payments p
    JOIN dbo.Transactions t
        ON t.TransactionId = p.TransactionId
    WHERE t.ShiftId = @ShiftId
      AND p.PaymentMethod = 'Cash';

    RETURN @Total;
END
GO
