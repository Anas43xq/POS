/* ============================================================
   dbo.SalesData.Seed.sql
   Generates ~14 days of realistic cafeteria sales data:
   Shifts, Transactions, TransactionItems, and Payments.

   Run AFTER master data seed (Products, Variants, Users exist).
   Safe to re-run: deletes existing sales data first.

   Business rules enforced:
   - Every transaction belongs to a shift (no orphans)
   - Shifts never overlap for the same user
   - Transaction totals match sum of item totals
   - Payments match transaction grand totals
   - Cash payments have valid AmountTendered / ChangeGiven
   - All FK references are valid (VariantId, not ProductId)
   ============================================================ */

SET NOCOUNT ON;

-- ============================================================
-- SECTION 1: CLEANUP EXISTING SALES DATA
-- ============================================================
DELETE FROM Payments
 WHERE TransactionId IN (SELECT TransactionId FROM Transactions);

DELETE FROM TransactionItems
 WHERE TransactionId IN (SELECT TransactionId FROM Transactions);

DELETE FROM Transactions;
DELETE FROM Shifts;

-- ============================================================
-- SECTION 2: BUILD VARIANT CATALOG WITH POOL ASSIGNMENTS
-- ============================================================
DECLARE @Variants TABLE (
    RowId       INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(200),
    SizeName    NVARCHAR(50),
    VariantId   INT,
    UnitPrice   DECIMAL(18,2),
    TaxRateVal  DECIMAL(5,4),
    Pool        NVARCHAR(20)  -- 'morning','afternoon','evening','any'
);

INSERT INTO @Variants (ProductName, SizeName, VariantId, UnitPrice, TaxRateVal, Pool)
SELECT
    p.Name,
    sz.Name,
    pv.VariantId,
    pv.UnitPrice,
    tr.Rate,
    CASE
        -- Morning: Manakeesh + Fatayer
        WHEN p.CategoryId IN (
            SELECT c.CategoryId FROM Categories c
            WHERE c.Name IN (N'Manakeesh', N'Fatayer')
              AND c.ParentCategoryId = (
                  SELECT fc.CategoryId FROM Categories fc
                  WHERE fc.Name = N'Food' AND fc.ParentCategoryId IS NULL))
            THEN N'morning'
        -- Afternoon: Pizza + Juices
        WHEN p.CategoryId IN (
            SELECT c.CategoryId FROM Categories c
            WHERE c.Name = N'Pizza'
              AND c.ParentCategoryId = (
                  SELECT fc.CategoryId FROM Categories fc
                  WHERE fc.Name = N'Food' AND fc.ParentCategoryId IS NULL))
            THEN N'afternoon'
        WHEN p.CategoryId IN (
            SELECT c.CategoryId FROM Categories c
            WHERE c.Name = N'Juices' AND c.ParentCategoryId IS NULL)
            THEN N'afternoon'
        -- Evening: Shakhtoura + Farshouha
        WHEN p.CategoryId IN (
            SELECT c.CategoryId FROM Categories c
            WHERE c.Name IN (N'Shakhtoura', N'Farshouha')
              AND c.ParentCategoryId = (
                  SELECT fc.CategoryId FROM Categories fc
                  WHERE fc.Name = N'Food' AND fc.ParentCategoryId IS NULL))
            THEN N'evening'
        ELSE N'any'
    END
FROM ProductVariants pv
JOIN Products p   ON pv.ProductId = p.ProductId
JOIN Sizes sz     ON pv.SizeId   = sz.SizeId
JOIN TaxRates tr  ON p.TaxRateId = tr.TaxRateId
WHERE pv.IsActive = 1;

-- Pre-compute pool sizes for random selection
DECLARE @MorningPoolSize   INT,
        @AfternoonPoolSize INT,
        @EveningPoolSize   INT;

SELECT @MorningPoolSize = COUNT(*)
  FROM @Variants WHERE Pool = N'morning' OR Pool = N'any';

SELECT @AfternoonPoolSize = COUNT(*)
  FROM @Variants WHERE Pool = N'afternoon' OR Pool = N'any';

SELECT @EveningPoolSize = COUNT(*)
  FROM @Variants WHERE Pool IN (N'evening', N'morning', N'afternoon', N'any');

-- ============================================================
-- SECTION 3: DEFINE SHIFTS (14 days)
-- ============================================================
DECLARE @ShiftDefs TABLE (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    OpenedAt        DATETIME2,
    ClosedAt        DATETIME2,
    DurationMinutes INT,
    ProductPool     NVARCHAR(20),
    PoolSize        INT,
    MinTxnCount     INT,
    MaxTxnCount     INT
);

DECLARE @Day     DATE = DATEADD(DAY, -13, CAST(GETDATE() AS DATE)),
        @EndDate DATE = CAST(GETDATE() AS DATE),
        @Dow     INT,
        @DayDt   DATETIME2;  -- DATETIME2 for HOUR/MINUTE arithmetic

WHILE @Day <= @EndDate
BEGIN
    SET @Dow   = DATEPART(WEEKDAY, @Day);
    SET @DayDt = CAST(@Day AS DATETIME2);

    -- Morning shift
    IF @Dow BETWEEN 2 AND 6  -- weekday
        INSERT INTO @ShiftDefs (OpenedAt, ClosedAt, ProductPool, PoolSize, MinTxnCount, MaxTxnCount)
        VALUES (DATEADD(MINUTE,30,DATEADD(HOUR,6,@DayDt)),
                DATEADD(MINUTE,30,DATEADD(HOUR,10,@DayDt)),
                N'morning', @MorningPoolSize, 15, 25);
    ELSE                     -- weekend
        INSERT INTO @ShiftDefs (OpenedAt, ClosedAt, ProductPool, PoolSize, MinTxnCount, MaxTxnCount)
        VALUES (DATEADD(HOUR,8,@DayDt),
                DATEADD(MINUTE,30,DATEADD(HOUR,12,@DayDt)),
                N'morning', @MorningPoolSize, 10, 18);

    -- Afternoon shift (all days)
    IF @Dow BETWEEN 2 AND 6
        INSERT INTO @ShiftDefs (OpenedAt, ClosedAt, ProductPool, PoolSize, MinTxnCount, MaxTxnCount)
        VALUES (DATEADD(HOUR,11,@DayDt),
                DATEADD(HOUR,15,@DayDt),
                N'afternoon', @AfternoonPoolSize, 20, 30);
    ELSE
        INSERT INTO @ShiftDefs (OpenedAt, ClosedAt, ProductPool, PoolSize, MinTxnCount, MaxTxnCount)
        VALUES (DATEADD(HOUR,13,@DayDt),
                DATEADD(MINUTE,30,DATEADD(HOUR,17,@DayDt)),
                N'afternoon', @AfternoonPoolSize, 14, 22);

    -- Evening shift (weekdays only)
    IF @Dow BETWEEN 2 AND 6
        INSERT INTO @ShiftDefs (OpenedAt, ClosedAt, ProductPool, PoolSize, MinTxnCount, MaxTxnCount)
        VALUES (DATEADD(HOUR,16,@DayDt),
                DATEADD(HOUR,20,@DayDt),
                N'evening', @EveningPoolSize, 12, 22);

    SET @Day = DATEADD(DAY, 1, @Day);
END

-- Fill duration
UPDATE @ShiftDefs
   SET DurationMinutes = DATEDIFF(MINUTE, OpenedAt, ClosedAt);

-- ============================================================
-- SECTION 4: GENERATE SHIFTS + TRANSACTIONS + ITEMS + PAYMENTS
-- ============================================================
DECLARE
    @DefId          INT,
    @CreatedShiftId INT,
    @ShiftStart     DATETIME2,
    @ShiftEnd       DATETIME2,
    @ShiftDuration  INT,
    @Pool           NVARCHAR(20),
    @PoolSz         INT,
    @MinTxn         INT,
    @MaxTxn         INT,
    @TxnCount       INT,
    @TxnIdx         INT,
    @TxnTime        DATETIME2,
    @TxnId          INT,
    @ItemCount      INT,
    @ItemIdx        INT,
    @RandomRow      INT,
    @VariantId      INT,
    @ProductName    NVARCHAR(200),
    @UnitPrice      DECIMAL(18,2),
    @TaxRate        DECIMAL(5,4),
    @Qty            INT,
    @LineSubtotal   DECIMAL(18,2),
    @LineTax        DECIMAL(18,2),
    @LineTotal      DECIMAL(18,2),
    @TxnSubtotal    DECIMAL(18,2),
    @TxnTaxTotal    DECIMAL(18,2),
    @TxnGrandTotal  DECIMAL(18,2),
    @PayMethod      NVARCHAR(20),
    @AmtTendered    DECIMAL(10,2),
    @Change         DECIMAL(10,2),
    @RefNum         NVARCHAR(100),
    @ReceiptNum     NVARCHAR(30),
    @ReceiptSeq     INT,
    @CashierId      INT = 2,
    @OpeningCash    DECIMAL(18,2) = 500.00,
    @ShiftDay       DATE,
    @PrevDay        DATE = NULL,
    @CashTotal      DECIMAL(18,2);

-- Cursor through every shift definition
DECLARE shift_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT Id, OpenedAt, ClosedAt, DurationMinutes,
           ProductPool, PoolSize, MinTxnCount, MaxTxnCount
      FROM @ShiftDefs
     ORDER BY OpenedAt;

OPEN shift_cursor;
FETCH NEXT FROM shift_cursor
 INTO @DefId, @ShiftStart, @ShiftEnd, @ShiftDuration,
      @Pool, @PoolSz, @MinTxn, @MaxTxn;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Day-level receipt counter
    SET @ShiftDay = CAST(@ShiftStart AS DATE);
    IF @PrevDay IS NULL OR @PrevDay <> @ShiftDay
    BEGIN
        SET @ReceiptSeq = 0;
        SET @PrevDay = @ShiftDay;
    END

    SET @CashTotal = 0;

    -- ---- Create shift ----
    INSERT INTO Shifts (UserId, OpenedAt, ClosedAt, OpeningCash, Status)
    VALUES (@CashierId, @ShiftStart, @ShiftEnd, @OpeningCash, 0);  -- Closed
    SET @CreatedShiftId = SCOPE_IDENTITY();

    -- ---- How many transactions this shift ----
    SET @TxnCount = @MinTxn + ABS(CHECKSUM(NEWID())) % (@MaxTxn - @MinTxn + 1);
    SET @TxnIdx = 0;

    WHILE @TxnIdx < @TxnCount
    BEGIN
        SET @TxnIdx = @TxnIdx + 1;

        -- Random time within the shift window
        SET @TxnTime = DATEADD(MINUTE,
                        ABS(CHECKSUM(NEWID())) % @ShiftDuration,
                        @ShiftStart);

        -- Receipt number (unique per day)
        SET @ReceiptSeq = @ReceiptSeq + 1;
        SET @ReceiptNum = N'RCP-' + FORMAT(@ShiftDay, N'yyyyMMdd')
                        + N'-' + RIGHT(N'000' + CAST(@ReceiptSeq AS VARCHAR), 3);

        -- Number of line items: 1 – 6
        SET @ItemCount = 1 + ABS(CHECKSUM(NEWID())) % 6;

        -- Create transaction shell (totals updated after items)
        INSERT INTO Transactions
            (ReceiptNumber, ShiftId, CashierId, TransactionDate,
             Subtotal, TaxTotal, GrandTotal, Status, CreatedAt)
        VALUES
            (@ReceiptNum, @CreatedShiftId, @CashierId, @TxnTime,
             0, 0, 0, 1 /*Completed*/, @TxnTime);
        SET @TxnId = SCOPE_IDENTITY();

        -- ---- Generate line items ----
        SET @ItemIdx     = 0;
        SET @TxnSubtotal = 0;
        SET @TxnTaxTotal = 0;

        WHILE @ItemIdx < @ItemCount
        BEGIN
            SET @ItemIdx = @ItemIdx + 1;

            -- Pick a random variant from the current pool
            SET @RandomRow = 1 + ABS(CHECKSUM(NEWID())) % @PoolSz;

            SELECT TOP 1
                @VariantId   = VariantId,
                @ProductName = ProductName,
                @UnitPrice   = UnitPrice,
                @TaxRate     = TaxRateVal
            FROM (
                SELECT VariantId, ProductName, UnitPrice, TaxRateVal,
                       ROW_NUMBER() OVER (ORDER BY RowId) AS rn
                  FROM @Variants
                 WHERE Pool = @Pool
                    OR Pool = N'any'
                    OR (@Pool = N'evening'
                        AND Pool IN (N'morning', N'afternoon'))
            ) v
            WHERE rn = @RandomRow;

            -- Random quantity: 1 – 4 (occasional larger orders)
            SET @Qty = 1 + ABS(CHECKSUM(NEWID())) % 4;

            -- Line math (matching existing conventions)
            SET @LineSubtotal = ROUND(@UnitPrice * @Qty, 2);
            SET @LineTax      = ROUND(@LineSubtotal * @TaxRate, 2);
            SET @LineTotal    = ROUND(@LineSubtotal + @LineTax, 2);

            INSERT INTO TransactionItems
                (TransactionId, VariantId, ProductName, UnitPrice,
                 Quantity, TaxRate, LineSubtotal, LineTax, LineTotal)
            VALUES
                (@TxnId, @VariantId, @ProductName, @UnitPrice,
                 @Qty, @TaxRate, @LineSubtotal, @LineTax, @LineTotal);

            SET @TxnSubtotal = @TxnSubtotal + @LineSubtotal;
            SET @TxnTaxTotal = @TxnTaxTotal + @LineTax;
        END

        -- Update transaction totals
        SET @TxnGrandTotal = @TxnSubtotal + @TxnTaxTotal;

        UPDATE Transactions
           SET Subtotal  = @TxnSubtotal,
               TaxTotal  = @TxnTaxTotal,
               GrandTotal = @TxnGrandTotal
         WHERE TransactionId = @TxnId;

        -- ---- Create payment ----
        -- ~70 % Cash, ~30 % Card
        SET @PayMethod = CASE
            WHEN ABS(CHECKSUM(NEWID())) % 10 < 7 THEN N'Cash'
            ELSE N'Card'
        END;

        IF @PayMethod = N'Cash'
        BEGIN
            -- Round up to nearest 0.50 so there is always change
            SET @AmtTendered = CEILING(@TxnGrandTotal * 2) / 2.0;
            IF @AmtTendered <= @TxnGrandTotal
                SET @AmtTendered = @AmtTendered + 0.50;
            SET @Change  = @AmtTendered - @TxnGrandTotal;
            SET @RefNum  = NULL;
            SET @CashTotal = @CashTotal + @TxnGrandTotal;
        END
        ELSE
        BEGIN
            SET @AmtTendered = @TxnGrandTotal;
            SET @Change  = 0;
            SET @RefNum  = N'REF-' + RIGHT(N'000000'
                            + CAST(ABS(CHECKSUM(NEWID())) % 999999 AS VARCHAR), 6);
        END

        INSERT INTO Payments
            (TransactionId, PaymentMethod, AmountTendered,
             ChangeGiven, ReferenceNumber, PaidAt)
        VALUES
            (@TxnId, @PayMethod, @AmtTendered,
             @Change, @RefNum, @TxnTime);
    END

    -- We will update shift cash totals in Section 5
    FETCH NEXT FROM shift_cursor
     INTO @DefId, @ShiftStart, @ShiftEnd, @ShiftDuration,
          @Pool, @PoolSz, @MinTxn, @MaxTxn;
END

CLOSE shift_cursor;
DEALLOCATE shift_cursor;

-- ============================================================
-- SECTION 5: FINALISE SHIFT CASH TOTALS
-- ============================================================
-- ExpectedCash = OpeningCash + cash sales (payments where method = Cash)
-- ClosingCash  = ExpectedCash ± small variance (realistic till count)
-- CashDifference = ClosingCash − ExpectedCash

UPDATE s
   SET ExpectedCash   = s.OpeningCash + ISNULL(ct.CashTotal, 0),
       CashDifference = ABS(CHECKSUM(NEWID())) % 51 - 25      -- −25 … +25
  FROM Shifts s
  LEFT JOIN (
      SELECT t.ShiftId, SUM(t.GrandTotal) AS CashTotal
        FROM Payments p
        JOIN Transactions t ON p.TransactionId = t.TransactionId
       WHERE p.PaymentMethod = N'Cash'
       GROUP BY t.ShiftId
  ) ct ON s.ShiftId = ct.ShiftId;

UPDATE Shifts
   SET ClosingCash = ExpectedCash + CashDifference
 WHERE ClosingCash IS NULL;

PRINT 'Sales data seed complete.';
GO
