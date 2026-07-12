USE master;
GO

/* ============================================================
   POS_DB Master Deployment Script
   Executes all schema, index, procedure, view, and seed scripts
   in proper dependency order.
   ============================================================ */
ALTER DATABASE POS_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE IF EXISTS POS_DB;
GO

CREATE DATABASE POS_DB;
GO

USE POS_DB;
GO

/* -------------------------------------------------
   1. CORE TABLES (dependency-ordered)
   ------------------------------------------------- */
:r .\DAL\db\Tables\dbo.Roles.Table.sql
GO

:r .\DAL\db\Tables\dbo.Users.Table.sql
GO

:r .\DAL\db\Tables\dbo.Sessions.Table.sql
GO

:r .\DAL\db\Tables\dbo.Categories.Table.sql
GO

:r .\DAL\db\Tables\dbo.TaxRates.Table.sql
GO

:r .\DAL\db\Tables\dbo.Products.Table.sql
GO

:r .\DAL\db\Tables\dbo.CategoryTranslations.Table.sql
GO

:r .\DAL\db\Tables\dbo.ProductTranslations.Table.sql
GO

:r .\DAL\db\Tables\dbo.Sizes.Table.sql
GO

:r .\DAL\db\Tables\dbo.SizeTranslations.Table.sql
GO

:r .\DAL\db\Tables\dbo.ProductVariants.Table.sql
GO

:r .\DAL\db\Tables\dbo.Shifts.Table.sql
GO

:r .\DAL\db\Tables\dbo.Suppliers.Table.sql
GO

:r .\DAL\db\Tables\dbo.PurchaseReceiptTypes.Table.sql
GO

:r .\DAL\db\Tables\dbo.ReceiptCounters.Table.sql
GO

:r .\DAL\db\Tables\dbo.PurchaseReceipts.Table.sql
GO

:r .\DAL\db\Tables\dbo.Transactions.Table.sql
GO

:r .\DAL\db\Tables\dbo.TransactionItems.Table.sql
GO

:r .\DAL\db\Tables\dbo.Payments.Table.sql
GO

:r .\DAL\db\Tables\dbo.AuditLogs.Table.sql
GO
/* -------------------------------------------------
   2. USER-DEFINED TYPES
   ------------------------------------------------- */
:r .\DAL\db\UserDefinedTypes\dbo.TransactionItemType.UserDefinedTableType.sql
GO

/* -------------------------------------------------
   3. INDEXES
   ------------------------------------------------- */
:r .\DAL\db\Indexes\Indexes.sql
GO

/* -------------------------------------------------
   4. VIEWS
   ------------------------------------------------- */
:r .\DAL\db\Views\dbo.vw_RecentTransactions.View.sql
GO

:r .\DAL\db\Views\dbo.vw_ShiftSummary.View.sql
GO

:r .\DAL\db\Views\dbo.vw_Transactions.View.sql
GO

:r .\DAL\db\Views\dbo.vw_ShiftManagement.View.sql
GO

:r .\DAL\db\Views\dbo.vw_TransactionsReport.View.sql
GO

/* -------------------------------------------------
   5. STORED PROCEDURES
   ------------------------------------------------- */
:r .\DAL\db\StoredProcedures\dbo.SP_GetPeriodDateRange.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.SP_GetNextReceiptNumber.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.SP_CreateTransaction.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.SP_GetTransactionsList.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.SP_GetTransactionKpis.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.SP_GetProductSalesReport.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.SP_GetTransactionsReport.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.GetRecentTransactionsByCashier.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.SP_GetTopProducts.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.SP_GetShiftsList.StoredProcedure.sql
GO

:r .\DAL\db\StoredProcedures\dbo.SP_GetShiftDetail.StoredProcedure.sql
GO

/* -------------------------------------------------
   6. SEED DATA
   ------------------------------------------------- */
:r .\DAL\db\seed.sql
GO

PRINT 'POS_DB deployment completed successfully.';
GO
