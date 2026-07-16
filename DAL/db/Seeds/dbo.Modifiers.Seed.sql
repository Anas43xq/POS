/* ============================================================
   1a. MODIFIER SEED DATA (MVP — Manakeesh only)
   ============================================================ */

/* ---- Modifier Groups ---- */
IF NOT EXISTS (SELECT 1 FROM ModifierGroups WHERE Name = N'Extras')
BEGIN
    SET IDENTITY_INSERT ModifierGroups ON;

    /* GroupType: 1=SingleSelect, 2=MultiSelect, 3=Quantity */
    INSERT INTO ModifierGroups (ModifierGroupId, Name, GroupType, IsRequired, MinSelections, MaxSelections, SortOrder, CreatedAt, UpdatedAt)
    VALUES
        (1, N'Extras',          2, 0, 0, 3, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
        (2, N'Dough Thickness', 1, 1, 1, 1, 2, SYSUTCDATETIME(), SYSUTCDATETIME());

    SET IDENTITY_INSERT ModifierGroups OFF;
END
ELSE
    PRINT 'ModifierGroups already seeded — skipping.';
GO

/* ---- Modifier Options ---- */
IF NOT EXISTS (SELECT 1 FROM ModifierOptions WHERE Name = N'Extra Cheese')
BEGIN
    SET IDENTITY_INSERT ModifierOptions ON;

    INSERT INTO ModifierOptions (ModifierOptionId, ModifierGroupId, Name, PriceAdd, IsActive, AllowQuantity, IsDefault, SortOrder, CreatedAt, UpdatedAt)
    VALUES
        /* Extras (ModifierGroupId = 1) */
        (1, 1, N'Extra Cheese', 2.00, 1, 0, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
        (2, 1, N'Extra Meat',   3.00, 1, 0, 0, 2, SYSUTCDATETIME(), SYSUTCDATETIME()),
        (3, 1, N'Extra Zaatar', 1.00, 1, 0, 0, 3, SYSUTCDATETIME(), SYSUTCDATETIME()),

        /* Dough Thickness (ModifierGroupId = 2) */
        (4, 2, N'Thin Dough',    0.00, 1, 0, 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME()),
        (5, 2, N'Regular Dough', 0.00, 1, 0, 1, 2, SYSUTCDATETIME(), SYSUTCDATETIME());

    SET IDENTITY_INSERT ModifierOptions OFF;
END
ELSE
    PRINT 'ModifierOptions already seeded — skipping.';
GO

/* ---- Assign modifier groups to the Manakeesh category ---- */
DECLARE @ManakeeshId INT;
SELECT @ManakeeshId = CategoryId FROM Categories WHERE Name = N'Manakeesh' AND ParentCategoryId IS NOT NULL;

IF @ManakeeshId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM CategoryModifierGroups WHERE CategoryId = @ManakeeshId AND ModifierGroupId = 1)
BEGIN
    INSERT INTO CategoryModifierGroups (CategoryId, ModifierGroupId)
    VALUES
        (@ManakeeshId, 1),   /* Extras          */
        (@ManakeeshId, 2);   /* Dough Thickness  */
END
ELSE
    PRINT 'CategoryModifierGroups already seeded — skipping.';
GO

/* ---- Modifier Group Translations (English) ---- */
DECLARE @LangEn NVARCHAR(10) = N'en';

IF NOT EXISTS (SELECT 1 FROM ModifierGroupTranslations WHERE ModifierGroupId = 1 AND LanguageCode = @LangEn)
    INSERT INTO ModifierGroupTranslations (ModifierGroupId, LanguageCode, Name, CreatedAt)
    VALUES (1, @LangEn, N'Extras', SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM ModifierGroupTranslations WHERE ModifierGroupId = 2 AND LanguageCode = @LangEn)
    INSERT INTO ModifierGroupTranslations (ModifierGroupId, LanguageCode, Name, CreatedAt)
    VALUES (2, @LangEn, N'Dough Thickness', SYSUTCDATETIME());

/* ---- Modifier Group Translations (Arabic) ---- */
DECLARE @LangAr NVARCHAR(10) = N'ar';

IF NOT EXISTS (SELECT 1 FROM ModifierGroupTranslations WHERE ModifierGroupId = 1 AND LanguageCode = @LangAr)
    INSERT INTO ModifierGroupTranslations (ModifierGroupId, LanguageCode, Name, CreatedAt)
    VALUES (1, @LangAr, N'إضافات', SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM ModifierGroupTranslations WHERE ModifierGroupId = 2 AND LanguageCode = @LangAr)
    INSERT INTO ModifierGroupTranslations (ModifierGroupId, LanguageCode, Name, CreatedAt)
    VALUES (2, @LangAr, N'سماكة العجين', SYSUTCDATETIME());
GO

/* ---- Modifier Option Translations (English) ---- */
DECLARE @LangEnOpt NVARCHAR(10) = N'en';

IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 1 AND LanguageCode = @LangEnOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (1, @LangEnOpt, N'Extra Cheese',  SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 2 AND LanguageCode = @LangEnOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (2, @LangEnOpt, N'Extra Meat',    SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 3 AND LanguageCode = @LangEnOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (3, @LangEnOpt, N'Extra Zaatar',  SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 4 AND LanguageCode = @LangEnOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (4, @LangEnOpt, N'Thin Dough',    SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 5 AND LanguageCode = @LangEnOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (5, @LangEnOpt, N'Regular Dough', SYSUTCDATETIME());

/* ---- Modifier Option Translations (Arabic) ---- */
DECLARE @LangArOpt NVARCHAR(10) = N'ar';

IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 1 AND LanguageCode = @LangArOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (1, @LangArOpt, N'جبنة إضافية', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 2 AND LanguageCode = @LangArOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (2, @LangArOpt, N'لحمة إضافية', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 3 AND LanguageCode = @LangArOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (3, @LangArOpt, N'زعتر إضافي', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 4 AND LanguageCode = @LangArOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (4, @LangArOpt, N'عجين رقيق', SYSUTCDATETIME());
IF NOT EXISTS (SELECT 1 FROM ModifierOptionTranslations WHERE ModifierOptionId = 5 AND LanguageCode = @LangArOpt)
    INSERT INTO ModifierOptionTranslations (ModifierOptionId, LanguageCode, Name, CreatedAt) VALUES (5, @LangArOpt, N'عجين عادي', SYSUTCDATETIME());
GO

PRINT 'Modifier seed data deployed successfully.';
GO