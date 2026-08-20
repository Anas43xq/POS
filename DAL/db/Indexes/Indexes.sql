USE [POS_DB];
GO

/* ============================================================
   POS_DB — Indexes
   Pass 2 Implementation (2026-08-20)
   
   Changes from Pass 1:
     DROPPED   IX_Users_Username               (P3-1 — redundant with UQ constraint)
     DROPPED   IX_Transactions_TransactionDate (P0-1 — superseded by covering composite)
     DROPPED   IX_Transactions_Status          (P0-1 — superseded by covering composite)
     DROPPED   IX_Shifts_Status                (P0-3 — superseded by covering composite)
     EXTENDED  IX_Transactions_TransactionDate_Status → covering INCLUDE + DESC direction
     EXTENDED  IX_Payments_TransactionId        → INCLUDE (PaymentMethod, AmountTendered, ChangeGiven, PaidAt)
     EXTENDED  IX_TransactionItems_TransactionId → INCLUDE (VariantId, ProductName, Quantity, UnitPrice, LineTotal, LineTax)
     EXTENDED  IX_TransactionItemModifiers_TransactionItemId → INCLUDE all modifier columns
     ADDED     IX_Shifts_UserId_Status          covering composite (P0-3)
     ADDED     IX_Sessions_ActiveByUser         filtered index (P2-2)
     ADDED     IX_AuditLogs_UserId              (P2-1 / FK-02)
     ADDED     IX_AuditLogs_Entity              (P2-1)
     ADDED     IX_RefreshTokens_ActiveByUser    filtered index (P3-2)
     ADDED     IX_PurchaseReceipts_DateSupplier consolidated covering composite (P2-3)
     ADDED     IX_TransactionItemModifiers_ModifierGroupId  (FK-01 / P1-4)
     ADDED     IX_AuditLogs_CreatedBy           (FK-03 / P2-5) on PurchaseReceipts.CreatedBy
   ============================================================ */

/* ============================================================
   USERS
   IX_Users_Username is intentionally NOT recreated here.
   UQ_Users_Username (defined on the table) is an identical
   unique nonclustered index and already serves all lookups.
   Recreating it would force SQL Server to maintain two
   identical structures on every Users write.
   ============================================================ */

/* ============================================================
   SESSIONS
   ============================================================ */

-- Active-session lookup: "is this user currently logged in?"
-- Fires on every cashier login and manager action.
-- Filtered index (LogoutAt IS NULL) is tiny — only open sessions.
-- Supersedes the utility of IX_Sessions_LoginAt for the common path.
CREATE INDEX IX_Sessions_UserId
    ON dbo.Sessions(UserId);

CREATE INDEX IX_Sessions_LoginAt
    ON dbo.Sessions(LoginAt);

-- Filtered covering index for open sessions only.
-- When a session is closed (LogoutAt set), SQL Server automatically
-- removes the row from this index — no manual cleanup needed.
CREATE INDEX IX_Sessions_ActiveByUser
    ON dbo.Sessions(UserId)
    INCLUDE (LoginAt)
    WHERE LogoutAt IS NULL;

/* ============================================================
   CATEGORIES
   ============================================================ */
CREATE INDEX IX_Categories_ParentCategoryId
    ON dbo.Categories(ParentCategoryId);

/* ============================================================
   PRODUCTS
   ============================================================ */
CREATE INDEX IX_Products_CategoryId
    ON dbo.Products(CategoryId);

CREATE INDEX IX_Products_TaxRateId
    ON dbo.Products(TaxRateId);

/* ============================================================
   SHIFTS
   P0-3: Composite covering index on (UserId, Status).
   
   Why: SP_CreateTransaction validates the open shift with
        WHERE ShiftId = @ShiftId AND UserId = @CashierId AND Status = 1
   on EVERY sale — this is the single hottest read path in the
   application. The old pair of single-column indexes
   (IX_Shifts_UserId, IX_Shifts_Status) forced SQL Server to
   choose one and apply a residual predicate for the other.
   
   IX_Shifts_Status is NOT recreated — it is fully superseded
   by the second column of this composite.
   
   UX_Shifts_OpenShift_User (filtered unique) is kept because
   it enforces the "one open shift per user" constraint at the
   DB level; it does not cover the validation lookup columns.
   ============================================================ */
CREATE INDEX IX_Shifts_UserId
    ON dbo.Shifts(UserId);

CREATE UNIQUE INDEX UX_Shifts_OpenShift_User
    ON dbo.Shifts(UserId)
    WHERE Status = 1;

-- P0-3: NEW — covering composite for shift-validation seek on every sale.
CREATE INDEX IX_Shifts_UserId_Status
    ON dbo.Shifts(UserId, Status)
    INCLUDE (ShiftId, OpenedAt, OpeningCash);

/* ============================================================
   TRANSACTIONS  (P0-1)
   
   Before: three separate indexes
     IX_Transactions_TransactionDate        (TransactionDate)
     IX_Transactions_Status                 (Status)
     IX_Transactions_TransactionDate_Status (TransactionDate, Status)
   
   After: one covering composite replaces all three.
   The standalone TransactionDate and Status indexes are NOT
   recreated — they are fully redundant with the leading
   columns of the new composite.
   
   DESC on TransactionDate aligns with ORDER BY TransactionDate DESC
   in paged list queries and GetRecentTransactionsByCashier,
   allowing index-forward scans instead of reverse scans.
   
   INCLUDE columns make the following query shapes fully covering
   (no key lookup to the clustered index):
     - vw_TransactionsReport / SP_GetTransactionsReport
     - SP_GetTransactionsList (paged with status filter)
     - SP_GetTransactionKpis (SUM/COUNT over date range)
     - GetRecentTransactionsByCashier (after correlated→JOIN rewrite)
   ============================================================ */
CREATE INDEX IX_Transactions_ShiftId
    ON dbo.Transactions(ShiftId);

CREATE INDEX IX_Transactions_CashierId
    ON dbo.Transactions(CashierId);

-- P0-1: NEW covering composite — replaces IX_Transactions_TransactionDate
--       and IX_Transactions_Status (both omitted above).
CREATE INDEX IX_Transactions_Date_Status_Covering
    ON dbo.Transactions(TransactionDate DESC, Status)
    INCLUDE (TransactionId, ReceiptNumber, GrandTotal, Notes, CashierId, ShiftId);

-- Kept: cashier+shift composite is still useful for shift-scoped
-- aggregations (FN_GetShiftTotalSales, FN_GetShiftCashTotal) that
-- don't need the date-range filter.
CREATE INDEX IX_Transactions_ShiftId_CashierId_Status
    ON dbo.Transactions(ShiftId, CashierId, Status);

/* ============================================================
   SUPPLIERS
   ============================================================ */
CREATE INDEX IX_Suppliers_TRN
    ON dbo.Suppliers(TRN);

/* ============================================================
   PURCHASE RECEIPTS  (P2-3)
   
   Before: five narrow single-column indexes.
   After:  one covering composite for the dominant query pattern
           (date range + supplier filter) replaces two of them.
   
   IX_PurchaseReceipts_InvoiceDate and IX_PurchaseReceipts_Supplier
   are NOT recreated — both are superseded by the leading columns
   of the new composite.
   
   IX_PurchaseReceipts_ReceiptType, _Category, _InvoiceNumber are
   kept because they serve distinct filter/lookup paths not covered
   by the composite.
   ============================================================ */

-- P2-3: NEW covering composite for date-range + supplier queries.
CREATE INDEX IX_PurchaseReceipts_DateSupplier
    ON dbo.PurchaseReceipts(InvoiceDate DESC, SupplierId)
    INCLUDE (ReceiptTypeId, Category, CreatedBy, InvoiceNumber);

-- P2-5 / FK-03: index on CreatedBy FK column.
CREATE INDEX IX_PurchaseReceipts_CreatedBy
    ON dbo.PurchaseReceipts(CreatedBy);

CREATE INDEX IX_PurchaseReceipts_ReceiptType
    ON dbo.PurchaseReceipts(ReceiptTypeId);

CREATE INDEX IX_PurchaseReceipts_Category
    ON dbo.PurchaseReceipts(Category);

CREATE INDEX IX_PurchaseReceipts_InvoiceNumber
    ON dbo.PurchaseReceipts(InvoiceNumber);

/* ============================================================
   PRODUCT VARIANTS
   ============================================================ */
CREATE INDEX IX_ProductVariants_Product_Size
    ON dbo.ProductVariants(ProductId, SizeId)
    INCLUDE (UnitPrice, IsActive);

CREATE INDEX IX_ProductVariants_SizeId
    ON dbo.ProductVariants(SizeId);

/* ============================================================
   TRANSLATION TABLES
   ============================================================ */
CREATE INDEX IX_ProductTranslations_LanguageCode
    ON dbo.ProductTranslations(LanguageCode);

CREATE INDEX IX_CategoryTranslations_LanguageCode
    ON dbo.CategoryTranslations(LanguageCode);

CREATE INDEX IX_SizeTranslations_LanguageCode
    ON dbo.SizeTranslations(LanguageCode);

/* ============================================================
   TRANSACTION ITEMS  (P1-1)
   
   Before: IX_TransactionItems_TransactionId had NO INCLUDE columns,
   forcing a key lookup for every column accessed in report
   aggregations (SP_GetTopProducts, SP_GetSalesAnalysisReport,
   SP_GetShiftDetail Result Set 2).
   
   After: INCLUDE covers all columns accessed via the TransactionId
   seek path, making those SPs fully covering index reads.
   ============================================================ */

-- P1-1: Extended with INCLUDE to cover all report aggregation columns.
CREATE INDEX IX_TransactionItems_TransactionId
    ON dbo.TransactionItems(TransactionId)
    INCLUDE (VariantId, ProductName, Quantity, UnitPrice, LineTotal, LineTax);

-- Kept: VariantId seek path for product-lookup and variant join,
-- covers a different entry point than the TransactionId seek above.
CREATE INDEX IX_TransactionItems_VariantId
    ON dbo.TransactionItems(VariantId)
    INCLUDE (TransactionId, Quantity, UnitPrice, LineTotal);

/* ============================================================
   PAYMENTS  (P0-2)
   
   Before: IX_Payments_TransactionId had NO INCLUDE columns.
   Every JOIN on TransactionId that reads PaymentMethod required
   a key lookup back to the clustered index. This hit:
     - vw_Transactions (every view render)
     - vw_TransactionsReport (every report)
     - vw_RecentTransactions (EF Core recent-tx load)
     - SP_GetTransactionKpis (cash/card split)
     - SP_GetShiftDetail (OUTER APPLY aggregation)
     - GetRecentTransactionsByCashier (correlated subquery — now JOIN)
   
   After: all columns accessed via TransactionId join are included,
   making every above query fully covering — zero key lookups.
   ============================================================ */

-- P0-2: Extended with full INCLUDE coverage for all JOIN access patterns.
CREATE INDEX IX_Payments_TransactionId
    ON dbo.Payments(TransactionId)
    INCLUDE (PaymentMethod, AmountTendered, ChangeGiven, PaidAt);

-- Kept for PaymentMethod-only filter queries (cash reconciliation etc.)
CREATE INDEX IX_Payments_Method
    ON dbo.Payments(PaymentMethod);

/* ============================================================
   AUDIT LOGS  (P2-1, FK-02)
   
   No index existed beyond the clustered PK. AuditLogs is
   append-only and grows continuously; management audit-trail
   queries against a bare heap would table-scan.
   ============================================================ */

-- P2-1 / FK-02: Per-user audit trail with time ordering.
CREATE INDEX IX_AuditLogs_UserId
    ON dbo.AuditLogs(UserId, OccurredAt DESC)
    INCLUDE (ActionType, EntityName, EntityId);

-- P2-1: Entity-specific audit views (what happened to record X?).
CREATE INDEX IX_AuditLogs_Entity
    ON dbo.AuditLogs(EntityName, EntityId, OccurredAt DESC);

/* ============================================================
   REFRESH TOKENS  (P3-2)
   ============================================================ */

-- P3-2: Filtered index — only active (not yet revoked) tokens.
-- Rows are automatically removed from this index when RevokedAt
-- is set, keeping it small even for users with long token histories.
CREATE INDEX IX_RefreshTokens_ActiveByUser
    ON dbo.RefreshTokens(UserId, ExpiresAt DESC)
    WHERE RevokedAt IS NULL;

/* ============================================================
   MODIFIER TABLES  (P1-4, P1-2)
   ============================================================ */
CREATE INDEX IX_ModifierOptions_ModifierGroupId
    ON dbo.ModifierOptions(ModifierGroupId)
    INCLUDE (Name, PriceAdd, IsActive, SortOrder);

CREATE INDEX IX_ModifierGroupTranslations_LanguageCode
    ON dbo.ModifierGroupTranslations(LanguageCode);

CREATE INDEX IX_ModifierOptionTranslations_LanguageCode
    ON dbo.ModifierOptionTranslations(LanguageCode);

-- P1-4 / FK-01: ModifierGroupId FK index — was missing entirely.
-- Required for ON DELETE / ON UPDATE cascade checks on modifier groups,
-- and for any query joining TransactionItemModifiers → ModifierGroups.
CREATE INDEX IX_TransactionItemModifiers_ModifierGroupId
    ON dbo.TransactionItemModifiers(ModifierGroupId);

-- P1-2: Extended with full INCLUDE — covers the EF Core
-- .ThenInclude(i => i.ModifierItems) receipt-detail load path.
-- Every column accessed when loading modifier rows for a receipt
-- is now served from the index without a key lookup.
CREATE INDEX IX_TransactionItemModifiers_TransactionItemId
    ON dbo.TransactionItemModifiers(TransactionItemId)
    INCLUDE (ModifierOptionId, ModifierGroupId, GroupName, OptionName,
             Quantity, PriceAdd, LineTotal, IsDefault);

-- Kept: ModifierOptionId seek for FK constraint path.
CREATE INDEX IX_TransactionItemModifiers_ModifierOptionId
    ON dbo.TransactionItemModifiers(ModifierOptionId);

GO
