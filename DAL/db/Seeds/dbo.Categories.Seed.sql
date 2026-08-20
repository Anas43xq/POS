/* ============================================================
   dbo.Categories.Seed.sql
   Seeds Categories + CategoryTranslations (ar / en / ml) for
   the Hawa Cafeteria menu.
   Extracted from seed.sql (split into per-entity files to match
   the naming convention used by dbo.Products.Seed.sql and
   dbo.Modifiers.Seed.sql). Data and logic unchanged from
   seed.sql — reorganized only.
   Idempotent: every INSERT is guarded by IF NOT EXISTS so the
   script can be re-run safely without creating duplicates.
   ============================================================ */

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


PRINT 'Category seed data deployed successfully.';
GO
