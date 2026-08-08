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
    VALUES (N'Standard', 0.11, GETDATE(), GETDATE());
GO

/* -------------------------------------------------
   2. CATEGORIES (+ CategoryTranslations)
   ------------------------------------------------- */
:r .\DAL\db\Seeds\dbo.Categories.Seed.sql
GO

/* -------------------------------------------------
   3. SIZES (+ SizeTranslations)
   ------------------------------------------------- */
:r .\DAL\db\Seeds\dbo.Sizes.Seed.sql
GO

/* -------------------------------------------------
   4. dbo.Products.Seed.sql (Products, ProductTranslations,
      ProductVariants) runs immediately after this script,
      via 00_Master_Deployment.sql.
   ------------------------------------------------- */
:r .\DAL\db\Seeds\dbo.Products.Seed.sql
GO

PRINT 'seed.sql deployed successfully.';
GO
