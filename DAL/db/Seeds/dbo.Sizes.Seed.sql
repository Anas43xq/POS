/* ============================================================
   dbo.Sizes.Seed.sql
   Seeds Sizes + SizeTranslations (ar / en / ml) for the Hawa
   Cafeteria menu.
   Extracted from seed.sql (split into per-entity files to match
   the naming convention used by dbo.Products.Seed.sql and
   dbo.Modifiers.Seed.sql). Data and logic unchanged from
   seed.sql — reorganized only.
   Idempotent: every INSERT is guarded by IF NOT EXISTS so the
   script can be re-run safely without creating duplicates.
   ============================================================ */

/* -------------------------------------------------
   3. SIZES
   ------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM Sizes WHERE Name = N'XSmall')
    INSERT INTO Sizes (Name, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (N'XSmall', 1, 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Sizes WHERE Name = N'Small')
    INSERT INTO Sizes (Name, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (N'Small', 2, 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Sizes WHERE Name = N'Medium')
    INSERT INTO Sizes (Name, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (N'Medium', 3, 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Sizes WHERE Name = N'Large')
    INSERT INTO Sizes (Name, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (N'Large', 4, 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Sizes WHERE Name = N'Regular')
    INSERT INTO Sizes (Name, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES (N'Regular', 5, 1, GETDATE(), GETDATE());
GO


/* -------------------------------------------------
   5. SIZE TRANSLATIONS
   ------------------------------------------------- */
DECLARE @XSmallId INT = (SELECT SizeId FROM Sizes WHERE Name = N'XSmall');
DECLARE @SmallId  INT = (SELECT SizeId FROM Sizes WHERE Name = N'Small');
DECLARE @MediumId INT = (SELECT SizeId FROM Sizes WHERE Name = N'Medium');
DECLARE @LargeId  INT = (SELECT SizeId FROM Sizes WHERE Name = N'Large');
DECLARE @RegularId INT = (SELECT SizeId FROM Sizes WHERE Name = N'Regular');

/* ---- Size Translations (Arabic) ---- */
DECLARE @SzLangAr NVARCHAR(10) = N'ar';

IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @XSmallId AND LanguageCode = @SzLangAr)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@XSmallId, @SzLangAr, N'صغير جدا', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @SmallId AND LanguageCode = @SzLangAr)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@SmallId, @SzLangAr, N'صغير', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @MediumId AND LanguageCode = @SzLangAr)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@MediumId, @SzLangAr, N'وسط', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @LargeId AND LanguageCode = @SzLangAr)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@LargeId, @SzLangAr, N'كبير', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @RegularId AND LanguageCode = @SzLangAr)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@RegularId, @SzLangAr, N'عادي', GETDATE());

/* ---- Size Translations (English) [FIX: previously missing] ---- */
DECLARE @SzLangEn NVARCHAR(10) = N'en';

IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @XSmallId AND LanguageCode = @SzLangEn)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@XSmallId, @SzLangEn, N'XSmall', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @SmallId AND LanguageCode = @SzLangEn)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@SmallId, @SzLangEn, N'Small', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @MediumId AND LanguageCode = @SzLangEn)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@MediumId, @SzLangEn, N'Medium', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @LargeId AND LanguageCode = @SzLangEn)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@LargeId, @SzLangEn, N'Large', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @RegularId AND LanguageCode = @SzLangEn)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@RegularId, @SzLangEn, N'Regular', GETDATE());

/* ---- Size Translations (Malayalam) [FIX: previously missing] ---- */
DECLARE @SzLangMl NVARCHAR(10) = N'ml';

IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @XSmallId AND LanguageCode = @SzLangMl)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@XSmallId, @SzLangMl, N'വളരെ ചെറുത്', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @SmallId AND LanguageCode = @SzLangMl)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@SmallId, @SzLangMl, N'ചെറുത്', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @MediumId AND LanguageCode = @SzLangMl)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@MediumId, @SzLangMl, N'ഇടത്തരം', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @LargeId AND LanguageCode = @SzLangMl)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@LargeId, @SzLangMl, N'വലുത്', GETDATE());
IF NOT EXISTS (SELECT 1 FROM SizeTranslations WHERE SizeId = @RegularId AND LanguageCode = @SzLangMl)
    INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreatedAt) VALUES (@RegularId, @SzLangMl, N'സാധാരണ', GETDATE());
GO


PRINT 'Size seed data deployed successfully.';
GO
