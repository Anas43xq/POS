USE [POS_DB]
GO
/****** Object:  StoredProcedure [dbo].[SP_GetNextReceiptNumber]    Script Date: 05/07/2026 12:02:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GetNextReceiptNumber]
    @ReceiptNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    DECLARE @NextNumber INT;

    BEGIN TRY
        BEGIN TRAN;

        -- ALWAYS lock row safely (no IF EXISTS)
        UPDATE ReceiptCounters WITH (UPDLOCK, HOLDLOCK, ROWLOCK)
        SET LastNumber = LastNumber + 1
        WHERE CounterDate = @Today;

        -- If no row was updated -> create it safely
        IF @@ROWCOUNT = 0
        BEGIN
            INSERT INTO ReceiptCounters (CounterDate, LastNumber)
            VALUES (@Today, 1);

            SET @NextNumber = 1;
        END
        ELSE
        BEGIN
            SELECT @NextNumber = LastNumber
            FROM ReceiptCounters WITH (UPDLOCK, HOLDLOCK)
            WHERE CounterDate = @Today;
        END

        -- Build receipt number: HWA-YYYYMMDD-NNNN
        -- Counter resets daily
        SET @ReceiptNumber =
            'HWA-' +
            FORMAT(@Today, 'yyyyMMdd') + '-' +
            RIGHT('0000' + CAST(@NextNumber AS NVARCHAR(10)), 4);

        COMMIT;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK;

        THROW;
    END CATCH
END
GO
