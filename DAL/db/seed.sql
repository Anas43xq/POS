/* ============================================================
   seed.sql
   Core reference data: TaxRates, then delegates to
   dbo.Categories.Seed.sql, dbo.Sizes.Seed.sql, and
   dbo.Products.Seed.sql (in that dependency order) under
   DAL/db/Seeds/. Categories/Sizes must run before Products,
   which resolves their ids by Name.
   Idempotent: every INSERT is guarded by IF NOT EXISTS so the
   script can be re-run safely without creating duplicates.
   ============================================================ */

/* -------------------------------------------------
   1. TAX RATES
   ------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM TaxRates WHERE Name = N'Standard')
    INSERT INTO TaxRates (Name, Rate, CreatedAt, UpdatedAt)
    VALUES (N'Standard', 0.05, GETDATE(), GETDATE());
GO

/* -------------------------------------------------
   2. ROLES
   ------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = N'Manager')
    INSERT INTO Roles (RoleName, Description, CreatedAt)
    VALUES (N'Manager', N'Full system access', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = N'Cashier')
    INSERT INTO Roles (RoleName, Description, CreatedAt)
    VALUES (N'Cashier', N'Point-of-sale operator', GETDATE());
GO

/* -------------------------------------------------
   3. USERS
   ------------------------------------------------- */
DECLARE @ManagerRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = N'Manager');
DECLARE @CashierRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = N'Cashier');

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N'manager')
    INSERT INTO Users (FullName, Username, PasswordHash, RoleId, IsActive, CreatedAt, UpdatedAt)
    VALUES (N'Manager', N'manager', N'$2a$08$kvZWp3TedomSkmYBUSd5R.T17Y9aeQU0T6DROel0pNdZTnnUxBDpG', @ManagerRoleId, 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N'cashier')
    INSERT INTO Users (FullName, Username, PasswordHash, RoleId, IsActive, CreatedAt, UpdatedAt)
    VALUES (N'Cashier', N'cashier', N'', @CashierRoleId, 1, GETDATE(), GETDATE());
GO

/* -------------------------------------------------
   4. CATEGORIES (+ CategoryTranslations)
   ------------------------------------------------- */
:r .\DAL\db\Seeds\dbo.Categories.Seed.sql
GO

/* -------------------------------------------------
   5. SIZES (+ SizeTranslations)
   ------------------------------------------------- */
:r .\DAL\db\Seeds\dbo.Sizes.Seed.sql
GO

/* -------------------------------------------------
   6. dbo.Products.Seed.sql (Products, ProductTranslations,
      ProductVariants) runs immediately after this script,
      via 00_Master_Deployment.sql.
   ------------------------------------------------- */
:r .\DAL\db\Seeds\dbo.Products.Seed.sql
GO


:r .\DAL\db\Seeds\dbo.Modifiers.Seed.sql
GO

:r ./Dal/db/Seeds/dbo.SalesData.Seed.sql
GO

:r ./Dal/db/Seeds/dbo.SuppliersAndReceipts.Seed.sql
GO

PRINT 'seed.sql deployed successfully.';
GO
