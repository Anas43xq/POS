/* ============================================================
   dbo.Products.Seed.sql
   Seeds Products + ProductTranslations (ar, en) + ProductVariants
   for the Hawa Cafeteria menu, in the normalized multilingual
   schema. Self-contained: resolves Category/TaxRate/Size ids
   by name so it can be run independently or via seed.sql.
   'ml' ProductTranslations are omitted — no reliable Malayalam
   product data exists in the project to seed from.
   ============================================================ */

DECLARE @StdTaxId INT = (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard');

DECLARE @FoodId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Food' AND ParentCategoryId IS NULL);
DECLARE @ManakeeshId  INT = (SELECT CategoryId FROM Categories WHERE Name = N'Manakeesh'  AND ParentCategoryId = @FoodId);
DECLARE @FatayerId    INT = (SELECT CategoryId FROM Categories WHERE Name = N'Fatayer'    AND ParentCategoryId = @FoodId);
DECLARE @PizzaId      INT = (SELECT CategoryId FROM Categories WHERE Name = N'Pizza'      AND ParentCategoryId = @FoodId);
DECLARE @ShakhtouraId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Shakhtoura' AND ParentCategoryId = @FoodId);
DECLARE @FarshouhaId  INT = (SELECT CategoryId FROM Categories WHERE Name = N'Farshouha'  AND ParentCategoryId = @FoodId);
DECLARE @JuicesId     INT = (SELECT CategoryId FROM Categories WHERE Name = N'Juices'     AND ParentCategoryId IS NULL);

/* Staging table: one row per product to be seeded, carrying its
   Arabic name and Size alongside the base columns so we can
   correlate freshly-generated ProductIds back to translations
   and variants via SeqNo (bulk INSERT ... OUTPUT pattern). */
DECLARE @ProductStage TABLE
(
    SeqNo         INT IDENTITY(1,1),
    Name          NVARCHAR(200),
    Description   NVARCHAR(500),
    CategoryId    INT,
    UnitPrice     DECIMAL(10,2),
    ArabicName    NVARCHAR(200),
    SizeName      NVARCHAR(50),
    MalayalamName NVARCHAR(200)
);

INSERT INTO @ProductStage (Name, Description, CategoryId, UnitPrice, ArabicName, SizeName, MalayalamName)
VALUES
    (N'Zater', N'Manakeesh Large', @ManakeeshId, 6.50, N'زعتر', N'Large', N'സഅ്തർ'),
    (N'Zater', N'Manakeesh Small', @ManakeeshId, 4.00, N'زعتر', N'Small', N'സഅ്തർ'),
    (N'Zater & Olives', N'Manakeesh Large', @ManakeeshId, 7.50, N'زعتر مع زيتون', N'Large', N'സഅ്തർ & ഒലിവ്'),
    (N'Zater & Olives', N'Manakeesh Small', @ManakeeshId, 4.50, N'زعتر مع زيتون', N'Small', N'സഅ്തർ & ഒലിവ്'),
    (N'Meat', N'Manakeesh Large', @ManakeeshId, 7.50, N'لحمة', N'Large', N'ഇറച്ചി'),
    (N'Meat', N'Manakeesh Small', @ManakeeshId, 4.00, N'لحمة', N'Small', N'ഇറച്ചി'),
    (N'Cheese', N'Manakeesh Large', @ManakeeshId, 7.50, N'جبنة', N'Large', N'ചീസ്'),
    (N'Cheese', N'Manakeesh Small', @ManakeeshId, 4.00, N'جبنة', N'Small', N'ചീസ്'),
    (N'Cheese & Meat', N'Manakeesh Large', @ManakeeshId, 8.50, N'جبن مع لحم', N'Large', N'ചീസ് & ഇറച്ചി'),
    (N'Cheese & Meat', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع لحم', N'Small', N'ചീസ് & ഇറച്ചി'),
    (N'Cheese & Zater', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع زعتر', N'Large', N'ചീസ് & സഅ്തർ'),
    (N'Cheese & Zater', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع زعتر', N'Small', N'ചീസ് & സഅ്തർ'),
    (N'Cheese & Baraka', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع حبة البركة', N'Large', N'ചീസ് & കരിഞ്ചീരകം'),
    (N'Cheese & Baraka', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع حبة البركة', N'Small', N'ചീസ് & കരിഞ്ചീരകം'),
    (N'Cheese & Veg.', N'Manakeesh Large', @ManakeeshId, 8.50, N'جبن مع خضار', N'Large', N'ചീസ് & പച്ചക്കറി'),
    (N'Cheese & Veg.', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع خضار', N'Small', N'ചീസ് & പച്ചക്കറി'),
    (N'Cheese & Muhamar', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن محمر', N'Large', N'ചീസ് & മുഹമ്മർ'),
    (N'Cheese & Muhamar', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن محمر', N'Small', N'ചീസ് & മുഹമ്മർ'),
    (N'Cheese & Egg', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع بيض', N'Large', N'ചീസ് & മുട്ട'),
    (N'Cheese & Egg', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع بيض', N'Small', N'ചീസ് & മുട്ട'),
    (N'Cheese & Chicken', N'Manakeesh Large', @ManakeeshId, 9.00, N'جبن مع دجاج', N'Large', N'ചീസ് & ചിക്കൻ'),
    (N'Cheese & Chicken', N'Manakeesh Small', @ManakeeshId, 5.00, N'جبن مع دجاج', N'Small', N'ചീസ് & ചിക്കൻ'),
    (N'Cheese & Olives', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع زيتون', N'Large', N'ചീസ് & ഒലിവ്'),
    (N'Cheese & Olives', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع زيتون', N'Small', N'ചീസ് & ഒലിവ്'),
    (N'Cheese & Mashrom', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع مشروم', N'Large', N'ചീസ് & കൂൺ'),
    (N'Cheese & Mashrom', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع مشروم', N'Small', N'ചീസ് & കൂൺ'),
    (N'Cheese & Hotdog', N'Manakeesh Large', @ManakeeshId, 8.50, N'جبن مع نقانق', N'Large', N'ചീസ് & ഹോട്ട് ഡോഗ്'),
    (N'Cheese & Hotdog', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع نقانق', N'Small', N'ചീസ് & ഹോട്ട് ഡോഗ്'),
    (N'Cheese & Labna', N'Manakeesh Large', @ManakeeshId, 8.50, N'جبن مع لبنة', N'Large', N'ചീസ് & ലബ്ന'),
    (N'Cheese & Labna', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع لبنة', N'Small', N'ചീസ് & ലബ്ന'),
    (N'Cheese & Honey', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع عسل', N'Large', N'ചീസ് & തേൻ'),
    (N'Cheese & Honey', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع عسل', N'Small', N'ചീസ് & തേൻ'),
    (N'Cheese & Sabanek', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع سبانخ', N'Large', N'ചീസ് & ചീര'),
    (N'Cheese & Sabanek', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع سبانخ', N'Small', N'ചീസ് & ചീര'),
    (N'Cheese & Veg. & Hotdog', N'Manakeesh Large', @ManakeeshId, 9.50, N'جبن مع خضار و نقانق', N'Large', N'ചീസ് & പച്ചക്കറി & ഹോട്ട് ഡോഗ്'),
    (N'Cheese & Veg. & Hotdog', N'Manakeesh Small', @ManakeeshId, 5.00, N'جبن مع خضار و نقانق', N'Small', N'ചീസ് & പച്ചക്കറി & ഹോട്ട് ഡോഗ്'),
    (N'Cheese & Oman', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن بطاطس عمان', N'Large', N'ചീസ് & ഒമാൻ ഉരുളക്കിഴങ്ങ്'),
    (N'Cheese & Oman', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن بطاطس عمان', N'Small', N'ചീസ് & ഒമാൻ ഉരുളക്കിഴങ്ങ്'),
    (N'Sabanek', N'Manakeesh Large', @ManakeeshId, 6.50, N'سبانخ', N'Large', N'ചീര'),
    (N'Sabanek', N'Manakeesh Small', @ManakeeshId, 4.00, N'سبانخ', N'Small', N'ചീര'),
    (N'Muhamar', N'Manakeesh Large', @ManakeeshId, 6.50, N'محمر', N'Large', N'മുഹമ്മർ'),
    (N'Muhamar', N'Manakeesh Small', @ManakeeshId, 4.00, N'محمر', N'Small', N'മുഹമ്മർ'),
    (N'Labna', N'Manakeesh Large', @ManakeeshId, 6.50, N'لبنة', N'Large', N'ലബ്ന'),
    (N'Labna', N'Manakeesh Small', @ManakeeshId, 4.00, N'لبنة', N'Small', N'ലബ്ന'),
    (N'Labna & Olives', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع زيتون', N'Large', N'ലബ്ന & ഒലിവ്'),
    (N'Labna & Olives', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع زيتون', N'Small', N'ലബ്ന & ഒലിവ്'),
    (N'Labna & Honey', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع عسل', N'Large', N'ലബ്ന & തേൻ'),
    (N'Labna & Honey', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع عسل', N'Small', N'ലബ്ന & തേൻ'),
    (N'Labna & Zater', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع زعتر', N'Large', N'ലബ്ന & സഅ്തർ'),
    (N'Labna & Zater', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع زعتر', N'Small', N'ലബ്ന & സഅ്തർ'),
    (N'Labna & Muhamar', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع محمر', N'Large', N'ലബ്ന & മുഹമ്മർ'),
    (N'Labna & Muhamar', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع محمر', N'Small', N'ലബ്ന & മുഹമ്മർ'),
    (N'Labna & Mashrom', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع مشروم', N'Large', N'ലബ്ന & കൂൺ'),
    (N'Labna & Mashrom', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع مشروم', N'Small', N'ലബ്ന & കൂൺ'),
    (N'Labna & Falafel', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع فلافل', N'Large', N'ലബ്ന & ഫലാഫൽ'),
    (N'Labna & Falafel', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع فلافل', N'Small', N'ലബ്ന & ഫലാഫൽ'),
    (N'Labna & Hotdog', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع نقانق', N'Large', N'ലബ്ന & ഹോട്ട് ഡോഗ്'),
    (N'Labna & Hotdog', N'Manakeesh Small', @ManakeeshId, 5.00, N'لبنة مع نقانق', N'Small', N'ലബ്ന & ഹോട്ട് ഡോഗ്'),
    (N'Kraft', N'Manakeesh Large', @ManakeeshId, 6.50, N'كرافت', N'Large', N'ക്രാഫ്റ്റ്'),
    (N'Kraft', N'Manakeesh Small', @ManakeeshId, 4.00, N'كرافت', N'Small', N'ക്രാഫ്റ്റ്'),
    (N'Kraft & Zater', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت زعتر', N'Large', N'ക്രാഫ്റ്റ് & സഅ്തർ'),
    (N'Kraft & Zater', N'Manakeesh Small', @ManakeeshId, 4.50, N'كرافت زعتر', N'Small', N'ക്രാഫ്റ്റ് & സഅ്തർ'),
    (N'Kraft & Honey', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت عسل', N'Large', N'ക്രാഫ്റ്റ് & തേൻ'),
    (N'Kraft & Honey', N'Manakeesh Small', @ManakeeshId, 4.50, N'كرافت عسل', N'Small', N'ക്രാഫ്റ്റ് & തേൻ'),
    (N'Kraft & Olives', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت زيتون', N'Large', N'ക്രാഫ്റ്റ് & ഒലിവ്'),
    (N'Kraft & Olives', N'Manakeesh Small', @ManakeeshId, 4.50, N'كرافت زيتون', N'Small', N'ക്രാഫ്റ്റ് & ഒലിവ്'),
    (N'Kraft & Hotdog', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت نقانق', N'Large', N'ക്രാഫ്റ്റ് & ഹോട്ട് ഡോഗ്'),
    (N'Kraft & Hotdog', N'Manakeesh Small', @ManakeeshId, 5.00, N'كرافت نقانق', N'Small', N'ക്രാഫ്റ്റ് & ഹോട്ട് ഡോഗ്'),
    (N'Kraft & Chicken', N'Manakeesh Large', @ManakeeshId, 9.00, N'كرافت دجاج', N'Large', N'ക്രാഫ്റ്റ് & ചിക്കൻ'),
    (N'Kraft & Chicken', N'Manakeesh Small', @ManakeeshId, 5.00, N'كرافت دجاج', N'Small', N'ക്രാഫ്റ്റ് & ചിക്കൻ'),
    (N'Kraft & Muhamar', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت محمر', N'Large', N'ക്രാഫ്റ്റ് & മുഹമ്മർ'),
    (N'Kraft & Muhamar', N'Manakeesh Small', @ManakeeshId, 5.00, N'كرافت محمر', N'Small', N'ക്രാഫ്റ്റ് & മുഹമ്മർ'),
    (N'Meat Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'لحم', N'Regular', N'ഇറച്ചി ഫതായർ'),
    (N'Cheese Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'جبن', N'Regular', N'ചീസ് ഫതായർ'),
    (N'Zater Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'زعتر', N'Regular', N'സഅ്തർ ഫതായർ'),
    (N'Muhamar Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'محمر', N'Regular', N'മുഹമ്മർ ഫതായർ'),
    (N'Sabanek Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'سبانخ', N'Regular', N'ചീര ഫതായർ'),
    (N'Onion Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'بصل', N'Regular', N'ഉള്ളി ഫതായർ'),
    (N'Pizza Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'بيتزا', N'Regular', N'പിസ്സ ഫതായർ'),
    (N'Pizza & Chicken Fatayer', N'Fatayer per piece', @FatayerId, 2.75, N'بيتزا مع دجاج', N'Regular', N'പിസ്സ & ചിക്കൻ ഫതായർ'),
    (N'Cheese & Baraka Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'جبن مع حبة البركة', N'Regular', N'ചീസ് & കരിഞ്ചീരകം ഫതായർ'),
    (N'Cheese & Zater Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'جبن مع زعتر', N'Regular', N'ചീസ് & സഅ്തർ ഫതായർ'),
    (N'Labna & Olives Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'لبنة مع زيتون', N'Regular', N'ലബ്ന & ഒലിവ് ഫതായർ'),
    (N'Labna & Zater Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'لبنة مع زعتر', N'Regular', N'ലബ്ന & സഅ്തർ ഫതായർ'),
    (N'Labna & Falafel Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'لبنة مع فلافل', N'Regular', N'ലബ്ന & ഫലാഫൽ ഫതായർ'),
    (N'Kibbeh Maqli', N'Fatayer per piece', @FatayerId, 2.50, N'كبة مقلي', N'Regular', N'കിബ്ബെ വറുത്തത്'),
    (N'Sambosa Vegetable', N'Fatayer per piece', @FatayerId, 2.00, N'سمبوسة خضار', N'Regular', N'പച്ചക്കറി സമോസ'),
    (N'Meat Pizza', N'Pizza Large', @PizzaId, 36.00, N'لحم', N'Large', N'ഇറച്ചി പിസ്സ'),
    (N'Meat Pizza', N'Pizza Medium', @PizzaId, 31.00, N'لحم', N'Medium', N'ഇറച്ചി പിസ്സ'),
    (N'Meat Pizza', N'Pizza Small', @PizzaId, 26.00, N'لحم', N'Small', N'ഇറച്ചി പിസ്സ'),
    (N'Meat Pizza', N'Pizza XS', @PizzaId, 10.50, N'لحم', N'XSmall', N'ഇറച്ചി പിസ്സ'),
    (N'Cheese Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن', N'Large', N'ചീസ് പിസ്സ'),
    (N'Cheese Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن', N'Medium', N'ചീസ് പിസ്സ'),
    (N'Cheese Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن', N'Small', N'ചീസ് പിസ്സ'),
    (N'Cheese Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن', N'XSmall', N'ചീസ് പിസ്സ'),
    (N'Cheese & Meat Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع لحم', N'Large', N'ചീസ് & ഇറച്ചി പിസ്സ'),
    (N'Cheese & Meat Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع لحم', N'Medium', N'ചീസ് & ഇറച്ചി പിസ്സ'),
    (N'Cheese & Meat Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع لحم', N'Small', N'ചീസ് & ഇറച്ചി പിസ്സ'),
    (N'Cheese & Meat Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع لحم', N'XSmall', N'ചീസ് & ഇറച്ചി പിസ്സ'),
    (N'Cheese & Chicken Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع دجاج', N'Large', N'ചീസ് & ചിക്കൻ പിസ്സ'),
    (N'Cheese & Chicken Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع دجاج', N'Medium', N'ചീസ് & ചിക്കൻ പിസ്സ'),
    (N'Cheese & Chicken Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع دجاج', N'Small', N'ചീസ് & ചിക്കൻ പിസ്സ'),
    (N'Cheese & Chicken Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع دجاج', N'XSmall', N'ചീസ് & ചിക്കൻ പിസ്സ'),
    (N'Cheese & Hotdog Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع نقانق', N'Large', N'ചീസ് & ഹോട്ട് ഡോഗ് പിസ്സ'),
    (N'Cheese & Hotdog Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع نقانق', N'Medium', N'ചീസ് & ഹോട്ട് ഡോഗ് പിസ്സ'),
    (N'Cheese & Hotdog Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع نقانق', N'Small', N'ചീസ് & ഹോട്ട് ഡോഗ് പിസ്സ'),
    (N'Cheese & Hotdog Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع نقانق', N'XSmall', N'ചീസ് & ഹോട്ട് ഡോഗ് പിസ്സ'),
    (N'Cheese & Veg. Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع خضار', N'Large', N'ചീസ് & പച്ചക്കറി പിസ്സ'),
    (N'Cheese & Veg. Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع خضار', N'Medium', N'ചീസ് & പച്ചക്കറി പിസ്സ'),
    (N'Cheese & Veg. Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع خضار', N'Small', N'ചീസ് & പച്ചക്കറി പിസ്സ'),
    (N'Cheese & Veg. Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع خضار', N'XSmall', N'ചീസ് & പച്ചക്കറി പിസ്സ'),
    (N'Cheese & Veg. & Chicken Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع خضار و دجاج', N'Large', N'ചീസ് & പച്ചക്കറി & ചിക്കൻ പിസ്സ'),
    (N'Cheese & Veg. & Chicken Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع خضار و دجاج', N'Medium', N'ചീസ് & പച്ചക്കറി & ചിക്കൻ പിസ്സ'),
    (N'Cheese & Veg. & Chicken Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع خضار و دجاج', N'Small', N'ചീസ് & പച്ചക്കറി & ചിക്കൻ പിസ്സ'),
    (N'Cheese & Veg. & Chicken Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع خضار و دجاج', N'XSmall', N'ചീസ് & പച്ചക്കറി & ചിക്കൻ പിസ്സ'),
    (N'Vegetables Pizza', N'Pizza Large', @PizzaId, 36.00, N'خضار', N'Large', N'പച്ചക്കറി പിസ്സ'),
    (N'Vegetables Pizza', N'Pizza Medium', @PizzaId, 31.00, N'خضار', N'Medium', N'പച്ചക്കറി പിസ്സ'),
    (N'Vegetables Pizza', N'Pizza Small', @PizzaId, 26.00, N'خضار', N'Small', N'പച്ചക്കറി പിസ്സ'),
    (N'Vegetables Pizza', N'Pizza XS', @PizzaId, 10.50, N'خضار', N'XSmall', N'പച്ചക്കറി പിസ്സ'),
    (N'Shrimp Pizza', N'Pizza Large', @PizzaId, 36.00, N'روبيان', N'Large', N'ചെമ്മീൻ പിസ്സ'),
    (N'Shrimp Pizza', N'Pizza Medium', @PizzaId, 31.00, N'روبيان', N'Medium', N'ചെമ്മീൻ പിസ്സ'),
    (N'Shrimp Pizza', N'Pizza Small', @PizzaId, 26.00, N'روبيان', N'Small', N'ചെമ്മീൻ പിസ്സ'),
    (N'Shrimp Pizza', N'Pizza XS', @PizzaId, 10.50, N'روبيان', N'XSmall', N'ചെമ്മീൻ പിസ്സ'),
    (N'Chicken Pizza', N'Pizza Large', @PizzaId, 36.00, N'دجاج', N'Large', N'ചിക്കൻ പിസ്സ'),
    (N'Chicken Pizza', N'Pizza Medium', @PizzaId, 31.00, N'دجاج', N'Medium', N'ചിക്കൻ പിസ്സ'),
    (N'Chicken Pizza', N'Pizza Small', @PizzaId, 26.00, N'دجاج', N'Small', N'ചിക്കൻ പിസ്സ'),
    (N'Chicken Pizza', N'Pizza XS', @PizzaId, 10.50, N'دجاج', N'XSmall', N'ചിക്കൻ പിസ്സ'),
    (N'Pepperoni Pizza', N'Pizza Large', @PizzaId, 36.00, N'بيبروني', N'Large', N'പെപ്പറോണി പിസ്സ'),
    (N'Pepperoni Pizza', N'Pizza Medium', @PizzaId, 31.00, N'بيبروني', N'Medium', N'പെപ്പറോണി പിസ്സ'),
    (N'Pepperoni Pizza', N'Pizza Small', @PizzaId, 26.00, N'بيبروني', N'Small', N'പെപ്പറോണി പിസ്സ'),
    (N'Pepperoni Pizza', N'Pizza XS', @PizzaId, 10.50, N'بيبروني', N'XSmall', N'പെപ്പറോണി പിസ്സ'),
    (N'Meat Shakhtoura', N'Shakhtoura', @ShakhtouraId, 9.00, N'لحم', N'Regular', N'ഇറച്ചി ഷഖ്തൂറ'),
    (N'Cheese Shakhtoura', N'Shakhtoura', @ShakhtouraId, 9.00, N'جبن', N'Regular', N'ചീസ് ഷഖ്തൂറ'),
    (N'Zater Shakhtoura', N'Shakhtoura', @ShakhtouraId, 8.00, N'زعتر', N'Regular', N'സഅ്തർ ഷഖ്തൂറ'),
    (N'Muhamar Shakhtoura', N'Shakhtoura', @ShakhtouraId, 8.00, N'محمر', N'Regular', N'മുഹമ്മർ ഷഖ്തൂറ'),
    (N'Sabanek Shakhtoura', N'Shakhtoura', @ShakhtouraId, 8.00, N'سبانخ', N'Regular', N'ചീര ഷഖ്തൂറ'),
    (N'Labneh Shakhtoura', N'Shakhtoura', @ShakhtouraId, 8.50, N'لبنة', N'Regular', N'ലബ്ന ഷഖ്തൂറ'),
    (N'Mix Shakhtoura', N'Shakhtoura', @ShakhtouraId, 9.50, N'مكس', N'Regular', N'മിക്സ് ഷഖ്തൂറ'),
    (N'Meat Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'لحم', N'Regular', N'ഇറച്ചി ഫർഷൂഹ'),
    (N'Cheese Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'جبن', N'Regular', N'ചീസ് ഫർഷൂഹ'),
    (N'Zater Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'زعتر', N'Regular', N'സഅ്തർ ഫർഷൂഹ'),
    (N'Muhamar Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'محمر', N'Regular', N'മുഹമ്മർ ഫർഷൂഹ'),
    (N'Sabanek Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'سبانخ', N'Regular', N'ചീര ഫർഷൂഹ'),
    (N'Labneh Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'لبنة', N'Regular', N'ലബ്ന ഫർഷൂഹ'),
    (N'Mix Farshouha', N'Farshouha', @FarshouhaId, 18.00, N'مكس', N'Regular', N'മിക്സ് ഫർഷൂഹ'),
    (N'Orange Juice', N'Juice Large', @JuicesId, 25.00, N'برتقال', N'Large', N'ഓറഞ്ച് ജ്യൂസ്'),
    (N'Orange Juice', N'Juice Medium', @JuicesId, 11.00, N'برتقال', N'Medium', N'ഓറഞ്ച് ജ്യൂസ്'),
    (N'Orange Juice', N'Juice Small', @JuicesId, 8.00, N'برتقال', N'Small', N'ഓറഞ്ച് ജ്യൂസ്'),
    (N'Mango Juice', N'Juice Large', @JuicesId, 25.00, N'مانجو', N'Large', N'മാങ്ങ ജ്യൂസ്'),
    (N'Mango Juice', N'Juice Medium', @JuicesId, 11.00, N'مانجو', N'Medium', N'മാങ്ങ ജ്യൂസ്'),
    (N'Mango Juice', N'Juice Small', @JuicesId, 8.00, N'مانجو', N'Small', N'മാങ്ങ ജ്യൂസ്'),
    (N'Pomegranate Juice', N'Juice Large', @JuicesId, 25.00, N'رمان', N'Large', N'മാതളം ജ്യൂസ്'),
    (N'Pomegranate Juice', N'Juice Medium', @JuicesId, 11.00, N'رمان', N'Medium', N'മാതളം ജ്യൂസ്'),
    (N'Pomegranate Juice', N'Juice Small', @JuicesId, 8.00, N'رمان', N'Small', N'മാതളം ജ്യൂസ്'),
    (N'Strawberry Juice', N'Juice Large', @JuicesId, 25.00, N'فراولة', N'Large', N'സ്ട്രോബെറി ജ്യൂസ്'),
    (N'Strawberry Juice', N'Juice Medium', @JuicesId, 11.00, N'فراولة', N'Medium', N'സ്ട്രോബെറി ജ്യൂസ്'),
    (N'Strawberry Juice', N'Juice Small', @JuicesId, 8.00, N'فراولة', N'Small', N'സ്ട്രോബെറി ജ്യൂസ്'),
    (N'Lemon Juice', N'Juice Large', @JuicesId, 25.00, N'ليمون', N'Large', N'നാരങ്ങ ജ്യൂസ്'),
    (N'Lemon Juice', N'Juice Medium', @JuicesId, 11.00, N'ليمون', N'Medium', N'നാരങ്ങ ജ്യൂസ്'),
    (N'Lemon Juice', N'Juice Small', @JuicesId, 8.00, N'ليمون', N'Small', N'നാരങ്ങ ജ്യൂസ്'),
    (N'Lemon with Mint Juice', N'Juice Large', @JuicesId, 25.00, N'ليمون و نعناع', N'Large', N'നാരങ്ങ & പുതിന ജ്യൂസ്'),
    (N'Lemon with Mint Juice', N'Juice Medium', @JuicesId, 11.00, N'ليمون و نعناع', N'Medium', N'നാരങ്ങ & പുതിന ജ്യൂസ്'),
    (N'Lemon with Mint Juice', N'Juice Small', @JuicesId, 8.00, N'ليمون و نعناع', N'Small', N'നാരങ്ങ & പുതിന ജ്യൂസ്'),
    (N'Cocktail Juice', N'Juice Large', @JuicesId, 25.00, N'كوكتيل', N'Large', N'കോക്ക്ടെയിൽ ജ്യൂസ്'),
    (N'Cocktail Juice', N'Juice Medium', @JuicesId, 11.00, N'كوكتيل', N'Medium', N'കോക്ക്ടെയിൽ ജ്യൂസ്'),
    (N'Cocktail Juice', N'Juice Small', @JuicesId, 8.00, N'كوكتيل', N'Small', N'കോക്ക്ടെയിൽ ജ്യൂസ്'),
    (N'Avocado Juice', N'Juice Large', @JuicesId, 25.00, N'افوكادو', N'Large', N'അവക്കാഡോ ജ്യൂസ്'),
    (N'Avocado Juice', N'Juice Medium', @JuicesId, 11.00, N'افوكادو', N'Medium', N'അവക്കാഡോ ജ്യൂസ്'),
    (N'Avocado Juice', N'Juice Small', @JuicesId, 8.00, N'افوكادو', N'Small', N'അവക്കാഡോ ജ്യൂസ്'),
    (N'Apple Juice', N'Juice Large', @JuicesId, 25.00, N'تفاح', N'Large', N'ആപ്പിൾ ജ്യൂസ്'),
    (N'Apple Juice', N'Juice Medium', @JuicesId, 11.00, N'تفاح', N'Medium', N'ആപ്പിൾ ജ്യൂസ്'),
    (N'Apple Juice', N'Juice Small', @JuicesId, 8.00, N'تفاح', N'Small', N'ആപ്പിൾ ജ്യൂസ്'),
    (N'Soft Drinks', N'Soft Drinks', @JuicesId, 4.00, N'غازيات', N'Regular', N'ശീതളപാനീയങ്ങൾ'),
    (N'Melco', N'Melco', @JuicesId, 2.00, N'ملكو', N'Regular', N'മെൽക്കോ'),
    (N'Water', N'Water', @JuicesId, 1.00, N'ماء', N'Regular', N'വെള്ളം');

/* Insert Products, capturing the new ProductId per staged row.
   Uses MERGE (always NOT MATCHED) so that the OUTPUT clause can
   reference source columns – plain INSERT … SELECT OUTPUT cannot. */
DECLARE @Inserted TABLE (ProductId INT, SeqNo INT);

MERGE INTO Products AS tgt
USING @ProductStage AS src
    ON 1 = 0  -- never matches → always INSERT
WHEN NOT MATCHED THEN
    INSERT (Name, Description, CategoryId, UnitPrice, TaxRateId, IsActive, CreatedAt, UpdatedAt)
    VALUES (src.Name, src.Description, src.CategoryId, src.UnitPrice, @StdTaxId, 1, GETDATE(), GETDATE())
OUTPUT INSERTED.ProductId, src.SeqNo INTO @Inserted (ProductId, SeqNo);

/* Arabic ProductTranslations */
INSERT INTO ProductTranslations (ProductId, LanguageCode, Name, Description, CreatedAt)
SELECT i.ProductId, N'ar', s.ArabicName, NULL, GETDATE()
FROM @Inserted i
JOIN @ProductStage s ON s.SeqNo = i.SeqNo;

/* English ProductTranslations (FIX: previously missing).
   Reuses the base Products.Name / Products.Description values,
   which are already English, as the 'en' translation row. */
INSERT INTO ProductTranslations (ProductId, LanguageCode, Name, Description, CreatedAt)
SELECT i.ProductId, N'en', s.Name, s.Description, GETDATE()
FROM @Inserted i
JOIN @ProductStage s ON s.SeqNo = i.SeqNo;

/* Malayalam ProductTranslations
   NOTE: best-effort translations generated from the existing
   Arabic dish names — not sourced from a native Malayalam
   speaker or an existing repository value. Flag for
   native-speaker review before treating as production data. */
INSERT INTO ProductTranslations (ProductId, LanguageCode, Name, Description, CreatedAt)
SELECT i.ProductId, N'ml', s.MalayalamName, NULL, GETDATE()
FROM @Inserted i
JOIN @ProductStage s ON s.SeqNo = i.SeqNo;

/* ProductVariants (one per product, size resolved by name) */
INSERT INTO ProductVariants (ProductId, SizeId, UnitPrice, IsActive, CreatedAt, UpdatedAt)
SELECT i.ProductId, sz.SizeId, s.UnitPrice, 1, GETDATE(), GETDATE()
FROM @Inserted i
JOIN @ProductStage s ON s.SeqNo = i.SeqNo
JOIN Sizes sz ON sz.Name = s.SizeName;

GO
