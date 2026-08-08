/* ============================================================
   seed.sql
   Core reference data: TaxRates, Categories, Sizes, and their
   translations (ar / en / ml). Runs before dbo.Products.Seed.sql
   and dbo.Modifiers.Seed.sql, which resolve these rows by Name.
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
   2. CATEGORIES
   ------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Food' AND ParentCategoryId IS NULL)
    INSERT INTO Categories (Name, ParentCategoryId, Description, CreatedAt, UpdatedAt)
    VALUES (N'Food', NULL, NULL, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Juices' AND ParentCategoryId IS NULL)
    INSERT INTO Categories (Name, ParentCategoryId, Description, CreatedAt, UpdatedAt)
    VALUES (N'Juices', NULL, NULL, GETDATE(), GETDATE());
GO

DECLARE @FoodId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Food' AND ParentCategoryId IS NULL);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Manakeesh' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description, CreatedAt, UpdatedAt)
    VALUES (N'Manakeesh', @FoodId, NULL, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Fatayer' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description, CreatedAt, UpdatedAt)
    VALUES (N'Fatayer', @FoodId, NULL, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Pizza' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description, CreatedAt, UpdatedAt)
    VALUES (N'Pizza', @FoodId, NULL, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Shakhtoura' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description, CreatedAt, UpdatedAt)
    VALUES (N'Shakhtoura', @FoodId, NULL, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Farshouha' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description, CreatedAt, UpdatedAt)
    VALUES (N'Farshouha', @FoodId, NULL, GETDATE(), GETDATE());
GO

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
   4. CATEGORY TRANSLATIONS
   ------------------------------------------------- */
DECLARE @FoodId2      INT = (SELECT CategoryId FROM Categories WHERE Name = N'Food'      AND ParentCategoryId IS NULL);
DECLARE @JuicesId     INT = (SELECT CategoryId FROM Categories WHERE Name = N'Juices'    AND ParentCategoryId IS NULL);
DECLARE @ManakeeshId  INT = (SELECT CategoryId FROM Categories WHERE Name = N'Manakeesh'  AND ParentCategoryId = @FoodId2);
DECLARE @FatayerId    INT = (SELECT CategoryId FROM Categories WHERE Name = N'Fatayer'    AND ParentCategoryId = @FoodId2);
DECLARE @PizzaId      INT = (SELECT CategoryId FROM Categories WHERE Name = N'Pizza'      AND ParentCategoryId = @FoodId2);
DECLARE @ShakhtouraId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Shakhtoura' AND ParentCategoryId = @FoodId2);
DECLARE @FarshouhaId  INT = (SELECT CategoryId FROM Categories WHERE Name = N'Farshouha'  AND ParentCategoryId = @FoodId2);

/* ---- Category Translations (Arabic) ---- */
DECLARE @LangAr NVARCHAR(10) = N'ar';

IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @FoodId2 AND LanguageCode = @LangAr)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@FoodId2, @LangAr, N'طعام', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @JuicesId AND LanguageCode = @LangAr)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@JuicesId, @LangAr, N'عصائر', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @ManakeeshId AND LanguageCode = @LangAr)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@ManakeeshId, @LangAr, N'مناقيش', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @FatayerId AND LanguageCode = @LangAr)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@FatayerId, @LangAr, N'فطاير', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @PizzaId AND LanguageCode = @LangAr)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@PizzaId, @LangAr, N'بيتزا', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @ShakhtouraId AND LanguageCode = @LangAr)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@ShakhtouraId, @LangAr, N'شكتورة', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @FarshouhaId AND LanguageCode = @LangAr)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@FarshouhaId, @LangAr, N'فرشوحة', GETDATE());

/* ---- Category Translations (English) [FIX: previously missing] ---- */
DECLARE @LangEn NVARCHAR(10) = N'en';

IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @FoodId2 AND LanguageCode = @LangEn)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@FoodId2, @LangEn, N'Food', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @JuicesId AND LanguageCode = @LangEn)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@JuicesId, @LangEn, N'Juices', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @ManakeeshId AND LanguageCode = @LangEn)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@ManakeeshId, @LangEn, N'Manakeesh', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @FatayerId AND LanguageCode = @LangEn)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@FatayerId, @LangEn, N'Fatayer', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @PizzaId AND LanguageCode = @LangEn)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@PizzaId, @LangEn, N'Pizza', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @ShakhtouraId AND LanguageCode = @LangEn)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@ShakhtouraId, @LangEn, N'Shakhtoura', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @FarshouhaId AND LanguageCode = @LangEn)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@FarshouhaId, @LangEn, N'Farshouha', GETDATE());

/* ---- Category Translations (Malayalam) [FIX: previously missing] ---- */
DECLARE @LangMl NVARCHAR(10) = N'ml';

IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @FoodId2 AND LanguageCode = @LangMl)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@FoodId2, @LangMl, N'ഭക്ഷണം', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @JuicesId AND LanguageCode = @LangMl)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@JuicesId, @LangMl, N'ജ്യൂസുകൾ', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @ManakeeshId AND LanguageCode = @LangMl)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@ManakeeshId, @LangMl, N'മനാഖീഷ്', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @FatayerId AND LanguageCode = @LangMl)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@FatayerId, @LangMl, N'ഫതായർ', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @PizzaId AND LanguageCode = @LangMl)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@PizzaId, @LangMl, N'പിസ്സ', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @ShakhtouraId AND LanguageCode = @LangMl)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@ShakhtouraId, @LangMl, N'ഷഖ്തൂറ', GETDATE());
IF NOT EXISTS (SELECT 1 FROM CategoryTranslations WHERE CategoryId = @FarshouhaId AND LanguageCode = @LangMl)
    INSERT INTO CategoryTranslations (CategoryId, LanguageCode, Name, CreatedAt) VALUES (@FarshouhaId, @LangMl, N'ഫർഷൂഹ', GETDATE());
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

/* -------------------------------------------------
   6. dbo.Products.Seed.sql (Products, ProductTranslations,
      ProductVariants) runs immediately after this script,
      via 00_Master_Deployment.sql.
   ------------------------------------------------- */
:r .\DAL\db\Seeds\dbo.Products.Seed.sql
GO

PRINT 'seed.sql deployed successfully.';
GO
