USE [POS_DB];
GO

/* ============================================================
   POS_DB - Indexes
   ============================================================ */

CREATE INDEX IX_Users_Username           ON dbo.Users(Username);
CREATE INDEX IX_Sessions_UserId           ON dbo.Sessions(UserId);
CREATE INDEX IX_Sessions_LoginAt          ON dbo.Sessions(LoginAt);
CREATE INDEX IX_Categories_ParentCategoryId ON dbo.Categories(ParentCategoryId);
CREATE INDEX IX_Products_CategoryId       ON dbo.Products(CategoryId);
CREATE INDEX IX_Products_TaxRateId        ON dbo.Products(TaxRateId);
CREATE INDEX IX_Shifts_UserId             ON dbo.Shifts(UserId);
CREATE INDEX IX_Shifts_Status             ON dbo.Shifts(Status);
CREATE UNIQUE INDEX UX_Shifts_OpenShift_User ON dbo.Shifts(UserId) WHERE Status = 1;
CREATE INDEX IX_Transactions_ShiftId      ON dbo.Transactions(ShiftId);
CREATE INDEX IX_Transactions_CashierId    ON dbo.Transactions(CashierId);
CREATE INDEX IX_Transactions_TransactionDate ON dbo.Transactions(TransactionDate);
CREATE INDEX IX_Transactions_Status       ON dbo.Transactions(Status);
CREATE INDEX IX_Suppliers_TRN             ON dbo.Suppliers(TRN);
CREATE INDEX IX_PurchaseReceipts_InvoiceDate ON dbo.PurchaseReceipts(InvoiceDate);
CREATE INDEX IX_PurchaseReceipts_Supplier ON dbo.PurchaseReceipts(SupplierId);
CREATE INDEX IX_PurchaseReceipts_ReceiptType ON dbo.PurchaseReceipts(ReceiptTypeId);
CREATE INDEX IX_PurchaseReceipts_Category ON dbo.PurchaseReceipts(Category);
CREATE INDEX IX_PurchaseReceipts_InvoiceNumber ON dbo.PurchaseReceipts(InvoiceNumber);
CREATE INDEX IX_TransactionItems_TransactionId ON dbo.TransactionItems(TransactionId);
CREATE INDEX IX_Payments_TransactionId    ON dbo.Payments(TransactionId);
CREATE INDEX IX_Payments_PaidAt           ON dbo.Payments(PaidAt);
CREATE INDEX IX_Payments_Method           ON dbo.Payments(PaymentMethod);
CREATE INDEX IX_AuditLogs_UserId          ON dbo.AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_OccurredAt      ON dbo.AuditLogs(OccurredAt);
CREATE INDEX IX_AuditLogs_ActionType      ON dbo.AuditLogs(ActionType);

/* ============================================================
   Translation & Variant tables
   ============================================================ */
CREATE INDEX IX_ProductVariants_Product_Size ON dbo.ProductVariants(ProductId, SizeId) INCLUDE (UnitPrice, IsActive);
CREATE INDEX IX_ProductVariants_SizeId      ON dbo.ProductVariants(SizeId);
CREATE INDEX IX_ProductTranslations_LanguageCode ON dbo.ProductTranslations(LanguageCode);
CREATE INDEX IX_CategoryTranslations_LanguageCode ON dbo.CategoryTranslations(LanguageCode);
CREATE INDEX IX_SizeTranslations_LanguageCode ON dbo.SizeTranslations(LanguageCode);
CREATE INDEX IX_TransactionItems_VariantId  ON dbo.TransactionItems(VariantId)
    INCLUDE (TransactionId, Quantity, UnitPrice, LineTotal);
GO

/* ============================================================
   Modifier tables
   ============================================================ */
CREATE INDEX IX_ModifierOptions_ModifierGroupId ON dbo.ModifierOptions(ModifierGroupId)
    INCLUDE (Name, PriceAdd, IsActive, SortOrder);
CREATE INDEX IX_ModifierGroupTranslations_LanguageCode ON dbo.ModifierGroupTranslations(LanguageCode);
CREATE INDEX IX_ModifierOptionTranslations_LanguageCode ON dbo.ModifierOptionTranslations(LanguageCode);
CREATE INDEX IX_TransactionItemModifiers_TransactionItemId ON dbo.TransactionItemModifiers(TransactionItemId);
CREATE INDEX IX_TransactionItemModifiers_ModifierOptionId ON dbo.TransactionItemModifiers(ModifierOptionId);
GO
