/* ============================================================
   dbo.Products.Seed.sql
   Seeds Products + ProductTranslations (ar, en) + ProductVariants
   for the Hawa Cafeteria menu, in the normalized multilingual
   schema. Self-contained: resolves Category/TaxRate/Size ids
   by name so it can be run independently or via seed.sql.
   'ml' ProductTranslations are omitted — no reliable Malayalam
   product data exists in the project to seed from.

   Pricing model: Products carry no price of their own. Each
   Product has one or more ProductVariants (Size + UnitPrice),
   which is the only source of selling price. Single-size items
   get a single 'Regular' variant.
   ============================================================ */

DECLARE @StdTaxId INT = (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard');

DECLARE @FoodId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Food' AND ParentCategoryId IS NULL);
DECLARE @ManakeeshId  INT = (SELECT CategoryId FROM Categories WHERE Name = N'Manakeesh'  AND ParentCategoryId = @FoodId);
DECLARE @FatayerId    INT = (SELECT CategoryId FROM Categories WHERE Name = N'Fatayer'    AND ParentCategoryId = @FoodId);
DECLARE @PizzaId      INT = (SELECT CategoryId FROM Categories WHERE Name = N'Pizza'      AND ParentCategoryId = @FoodId);
DECLARE @ShakhtouraId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Shakhtoura' AND ParentCategoryId = @FoodId);
DECLARE @FarshouhaId  INT = (SELECT CategoryId FROM Categories WHERE Name = N'Farshouha'  AND ParentCategoryId = @FoodId);
DECLARE @JuicesId     INT = (SELECT CategoryId FROM Categories WHERE Name = N'Juices'     AND ParentCategoryId IS NULL);

/* Staging table: one row per PRODUCT (not per size) to be seeded,
   carrying its Arabic/Malayalam names alongside the base columns
   so we can correlate freshly-generated ProductIds back to
   translations and variants via SeqNo (bulk INSERT ... OUTPUT
   pattern). */
DECLARE @ProductStage TABLE
(
    SeqNo         INT IDENTITY(1,1),
    Name          NVARCHAR(200),
    Description   NVARCHAR(500),
    CategoryId    INT,
    ArabicName    NVARCHAR(200),
    MalayalamName NVARCHAR(200)
);

INSERT INTO @ProductStage (Name, Description, CategoryId, ArabicName, MalayalamName)
VALUES
    (N'Zater', N'Manakeesh', @ManakeeshId, N'زعتر', N'സഅ്തർ'),
    (N'Zater & Olives', N'Manakeesh', @ManakeeshId, N'زعتر مع زيتون', N'സഅ്തർ & ഒലിവ്'),
    (N'Meat', N'Manakeesh', @ManakeeshId, N'لحمة', N'ഇറച്ചി'),
    (N'Cheese', N'Manakeesh', @ManakeeshId, N'جبنة', N'ചീസ്'),
    (N'Cheese & Meat', N'Manakeesh', @ManakeeshId, N'جبن مع لحم', N'ചീസ് & ഇറച്ചി'),
    (N'Cheese & Zater', N'Manakeesh', @ManakeeshId, N'جبن مع زعتر', N'ചീസ് & സഅ്തർ'),
    (N'Cheese & Baraka', N'Manakeesh', @ManakeeshId, N'جبن مع حبة البركة', N'ചീസ് & കരിഞ്ചീരകം'),
    (N'Cheese & Veg.', N'Manakeesh', @ManakeeshId, N'جبن مع خضار', N'ചീസ് & പച്ചക്കറി'),
    (N'Cheese & Muhamar', N'Manakeesh', @ManakeeshId, N'جبن محمر', N'ചീസ് & മുഹമ്മർ'),
    (N'Cheese & Egg', N'Manakeesh', @ManakeeshId, N'جبن مع بيض', N'ചീസ് & മുട്ട'),
    (N'Cheese & Chicken', N'Manakeesh', @ManakeeshId, N'جبن مع دجاج', N'ചീസ് & ചിക്കൻ'),
    (N'Cheese & Olives', N'Manakeesh', @ManakeeshId, N'جبن مع زيتون', N'ചീസ് & ഒലിവ്'),
    (N'Cheese & Mashrom', N'Manakeesh', @ManakeeshId, N'جبن مع مشروم', N'ചീസ് & കൂൺ'),
    (N'Cheese & Hotdog', N'Manakeesh', @ManakeeshId, N'جبن مع نقانق', N'ചീസ് & ഹോട്ട് ഡോഗ്'),
    (N'Cheese & Labna', N'Manakeesh', @ManakeeshId, N'جبن مع لبنة', N'ചീസ് & ലബ്ന'),
    (N'Cheese & Honey', N'Manakeesh', @ManakeeshId, N'جبن مع عسل', N'ചീസ് & തേൻ'),
    (N'Cheese & Sabanek', N'Manakeesh', @ManakeeshId, N'جبن مع سبانخ', N'ചീസ് & ചീര'),
    (N'Cheese & Veg. & Hotdog', N'Manakeesh', @ManakeeshId, N'جبن مع خضار و نقانق', N'ചീസ് & പച്ചക്കറി & ഹോട്ട് ഡോഗ്'),
    (N'Cheese & Oman', N'Manakeesh', @ManakeeshId, N'جبن بطاطس عمان', N'ചീസ് & ഒമാൻ ഉരുളക്കിഴങ്ങ്'),
    (N'Sabanek', N'Manakeesh', @ManakeeshId, N'سبانخ', N'ചീര'),
    (N'Muhamar', N'Manakeesh', @ManakeeshId, N'محمر', N'മുഹമ്മർ'),
    (N'Labna', N'Manakeesh', @ManakeeshId, N'لبنة', N'ലബ്ന'),
    (N'Labna & Olives', N'Manakeesh', @ManakeeshId, N'لبنة مع زيتون', N'ലബ്ന & ഒലിവ്'),
    (N'Labna & Honey', N'Manakeesh', @ManakeeshId, N'لبنة مع عسل', N'ലബ്ന & തേൻ'),
    (N'Labna & Zater', N'Manakeesh', @ManakeeshId, N'لبنة مع زعتر', N'ലബ്ന & സഅ്തർ'),
    (N'Labna & Muhamar', N'Manakeesh', @ManakeeshId, N'لبنة مع محمر', N'ലബ്ന & മുഹമ്മർ'),
    (N'Labna & Mashrom', N'Manakeesh', @ManakeeshId, N'لبنة مع مشروم', N'ലബ്ന & കൂൺ'),
    (N'Labna & Falafel', N'Manakeesh', @ManakeeshId, N'لبنة مع فلافل', N'ലബ്ന & ഫലാഫൽ'),
    (N'Labna & Hotdog', N'Manakeesh', @ManakeeshId, N'لبنة مع نقانق', N'ലബ്ന & ഹോട്ട് ഡോഗ്'),
    (N'Kraft', N'Manakeesh', @ManakeeshId, N'كرافت', N'ക്രാഫ്റ്റ്'),
    (N'Kraft & Zater', N'Manakeesh', @ManakeeshId, N'كرافت زعتر', N'ക്രാഫ്റ്റ് & സഅ്തർ'),
    (N'Kraft & Honey', N'Manakeesh', @ManakeeshId, N'كرافت عسل', N'ക്രാഫ്റ്റ് & തേൻ'),
    (N'Kraft & Olives', N'Manakeesh', @ManakeeshId, N'كرافت زيتون', N'ക്രാഫ്റ്റ് & ഒലിവ്'),
    (N'Kraft & Hotdog', N'Manakeesh', @ManakeeshId, N'كرافت نقانق', N'ക്രാഫ്റ്റ് & ഹോട്ട് ഡോഗ്'),
    (N'Kraft & Chicken', N'Manakeesh', @ManakeeshId, N'كرافت دجاج', N'ക്രാഫ്റ്റ് & ചിക്കൻ'),
    (N'Kraft & Muhamar', N'Manakeesh', @ManakeeshId, N'كرافت محمر', N'ക്രാഫ്റ്റ് & മുഹമ്മർ'),
    (N'Meat Fatayer', N'Fatayer per piece', @FatayerId, N'لحم', N'ഇറച്ചി ഫതായർ'),
    (N'Cheese Fatayer', N'Fatayer per piece', @FatayerId, N'جبن', N'ചീസ് ഫതായർ'),
    (N'Zater Fatayer', N'Fatayer per piece', @FatayerId, N'زعتر', N'സഅ്തർ ഫതായർ'),
    (N'Muhamar Fatayer', N'Fatayer per piece', @FatayerId, N'محمر', N'മുഹമ്മർ ഫതായർ'),
    (N'Sabanek Fatayer', N'Fatayer per piece', @FatayerId, N'سبانخ', N'ചീര ഫതായർ'),
    (N'Onion Fatayer', N'Fatayer per piece', @FatayerId, N'بصل', N'ഉള്ളി ഫതായർ'),
    (N'Pizza Fatayer', N'Fatayer per piece', @FatayerId, N'بيتزا', N'പിസ്സ ഫതായർ'),
    (N'Pizza & Chicken Fatayer', N'Fatayer per piece', @FatayerId, N'بيتزا مع دجاج', N'പിസ്സ & ചിക്കൻ ഫതായർ'),
    (N'Cheese & Baraka Fatayer', N'Fatayer per piece', @FatayerId, N'جبن مع حبة البركة', N'ചീസ് & കരിഞ്ചീരകം ഫതായർ'),
    (N'Cheese & Zater Fatayer', N'Fatayer per piece', @FatayerId, N'جبن مع زعتر', N'ചീസ് & സഅ്തർ ഫതായർ'),
    (N'Labna & Olives Fatayer', N'Fatayer per piece', @FatayerId, N'لبنة مع زيتون', N'ലബ്ന & ഒലിവ് ഫതായർ'),
    (N'Labna & Zater Fatayer', N'Fatayer per piece', @FatayerId, N'لبنة مع زعتر', N'ലബ്ന & സഅ്തർ ഫതായർ'),
    (N'Labna & Falafel Fatayer', N'Fatayer per piece', @FatayerId, N'لبنة مع فلافل', N'ലബ്ന & ഫലാഫൽ ഫതായർ'),
    (N'Kibbeh Maqli', N'Fatayer per piece', @FatayerId, N'كبة مقلي', N'കിബ്ബെ വറുത്തത്'),
    (N'Sambosa Vegetable', N'Fatayer per piece', @FatayerId, N'سمبوسة خضار', N'പച്ചക്കറി സമോസ'),
    (N'Meat Pizza', N'Pizza', @PizzaId, N'لحم', N'ഇറച്ചി പിസ്സ'),
    (N'Cheese Pizza', N'Pizza', @PizzaId, N'جبن', N'ചീസ് പിസ്സ'),
    (N'Cheese & Meat Pizza', N'Pizza', @PizzaId, N'جبن مع لحم', N'ചീസ് & ഇറച്ചി പിസ്സ'),
    (N'Cheese & Chicken Pizza', N'Pizza', @PizzaId, N'جبن مع دجاج', N'ചീസ് & ചിക്കൻ പിസ്സ'),
    (N'Cheese & Hotdog Pizza', N'Pizza', @PizzaId, N'جبن مع نقانق', N'ചീസ് & ഹോട്ട് ഡോഗ് പിസ്സ'),
    (N'Cheese & Veg. Pizza', N'Pizza', @PizzaId, N'جبن مع خضار', N'ചീസ് & പച്ചക്കറി പിസ്സ'),
    (N'Cheese & Veg. & Chicken Pizza', N'Pizza', @PizzaId, N'جبن مع خضار و دجاج', N'ചീസ് & പച്ചക്കറി & ചിക്കൻ പിസ്സ'),
    (N'Vegetables Pizza', N'Pizza', @PizzaId, N'خضار', N'പച്ചക്കറി പിസ്സ'),
    (N'Shrimp Pizza', N'Pizza', @PizzaId, N'روبيان', N'ചെമ്മീൻ പിസ്സ'),
    (N'Chicken Pizza', N'Pizza', @PizzaId, N'دجاج', N'ചിക്കൻ പിസ്സ'),
    (N'Pepperoni Pizza', N'Pizza', @PizzaId, N'بيبروني', N'പെപ്പറോണി പിസ്സ'),
    (N'Meat Shakhtoura', N'Shakhtoura', @ShakhtouraId, N'لحم', N'ഇറച്ചി ഷഖ്തൂറ'),
    (N'Cheese Shakhtoura', N'Shakhtoura', @ShakhtouraId, N'جبن', N'ചീസ് ഷഖ്തൂറ'),
    (N'Zater Shakhtoura', N'Shakhtoura', @ShakhtouraId, N'زعتر', N'സഅ്തർ ഷഖ്തൂറ'),
    (N'Muhamar Shakhtoura', N'Shakhtoura', @ShakhtouraId, N'محمر', N'മുഹമ്മർ ഷഖ്തൂറ'),
    (N'Sabanek Shakhtoura', N'Shakhtoura', @ShakhtouraId, N'سبانخ', N'ചീര ഷഖ്തൂറ'),
    (N'Labneh Shakhtoura', N'Shakhtoura', @ShakhtouraId, N'لبنة', N'ലബ്ന ഷഖ്തൂറ'),
    (N'Mix Shakhtoura', N'Shakhtoura', @ShakhtouraId, N'مكس', N'മിക്സ് ഷഖ്തൂറ'),
    (N'Meat Farshouha', N'Farshouha', @FarshouhaId, N'لحم', N'ഇറച്ചി ഫർഷൂഹ'),
    (N'Cheese Farshouha', N'Farshouha', @FarshouhaId, N'جبن', N'ചീസ് ഫർഷൂഹ'),
    (N'Zater Farshouha', N'Farshouha', @FarshouhaId, N'زعتر', N'സഅ്തർ ഫർഷൂഹ'),
    (N'Muhamar Farshouha', N'Farshouha', @FarshouhaId, N'محمر', N'മുഹമ്മർ ഫർഷൂഹ'),
    (N'Sabanek Farshouha', N'Farshouha', @FarshouhaId, N'سبانخ', N'ചീര ഫർഷൂഹ'),
    (N'Labneh Farshouha', N'Farshouha', @FarshouhaId, N'لبنة', N'ലബ്ന ഫർഷൂഹ'),
    (N'Mix Farshouha', N'Farshouha', @FarshouhaId, N'مكس', N'മിക്സ് ഫർഷൂഹ'),
    (N'Orange Juice', N'Juice', @JuicesId, N'برتقال', N'ഓറഞ്ച് ജ്യൂസ്'),
    (N'Mango Juice', N'Juice', @JuicesId, N'مانجو', N'മാങ്ങ ജ്യൂസ്'),
    (N'Pomegranate Juice', N'Juice', @JuicesId, N'رمان', N'മാതളം ജ്യൂസ്'),
    (N'Strawberry Juice', N'Juice', @JuicesId, N'فراولة', N'സ്ട്രോബെറി ജ്യൂസ്'),
    (N'Lemon Juice', N'Juice', @JuicesId, N'ليمون', N'നാരങ്ങ ജ്യൂസ്'),
    (N'Lemon with Mint Juice', N'Juice', @JuicesId, N'ليمون و نعناع', N'നാരങ്ങ & പുതിന ജ്യൂസ്'),
    (N'Cocktail Juice', N'Juice', @JuicesId, N'كوكتيل', N'കോക്ക്ടെയിൽ ജ്യൂസ്'),
    (N'Avocado Juice', N'Juice', @JuicesId, N'افوكادو', N'അവക്കാഡോ ജ്യൂസ്'),
    (N'Apple Juice', N'Juice', @JuicesId, N'تفاح', N'ആപ്പിൾ ജ്യൂസ്'),
    (N'Soft Drinks', N'Soft Drinks', @JuicesId, N'غازيات', N'ശീതളപാനീയങ്ങൾ'),
    (N'Melco', N'Melco', @JuicesId, N'ملكو', N'മെൽക്കോ'),
    (N'Water', N'Water', @JuicesId, N'ماء', N'വെള്ളം');;

/* Variant staging table: one row per Size/Price for a given
   product SeqNo (references @ProductStage.SeqNo). A single-size
   product has exactly one row here with SizeName = 'Regular'. */
DECLARE @VariantStage TABLE
(
    ProductSeqNo INT,
    SizeName     NVARCHAR(50),
    UnitPrice    DECIMAL(10,2)
);

INSERT INTO @VariantStage (ProductSeqNo, SizeName, UnitPrice)
VALUES
    (1, N'Large', 6.50),
    (1, N'Small', 4.00),
    (2, N'Large', 7.50),
    (2, N'Small', 4.50),
    (3, N'Large', 7.50),
    (3, N'Small', 4.00),
    (4, N'Large', 7.50),
    (4, N'Small', 4.00),
    (5, N'Large', 8.50),
    (5, N'Small', 4.50),
    (6, N'Large', 8.00),
    (6, N'Small', 4.50),
    (7, N'Large', 8.00),
    (7, N'Small', 4.50),
    (8, N'Large', 8.50),
    (8, N'Small', 4.50),
    (9, N'Large', 8.00),
    (9, N'Small', 4.50),
    (10, N'Large', 8.00),
    (10, N'Small', 4.50),
    (11, N'Large', 9.00),
    (11, N'Small', 5.00),
    (12, N'Large', 8.00),
    (12, N'Small', 4.50),
    (13, N'Large', 8.00),
    (13, N'Small', 4.50),
    (14, N'Large', 8.50),
    (14, N'Small', 4.50),
    (15, N'Large', 8.50),
    (15, N'Small', 4.50),
    (16, N'Large', 8.00),
    (16, N'Small', 4.50),
    (17, N'Large', 8.00),
    (17, N'Small', 4.50),
    (18, N'Large', 9.50),
    (18, N'Small', 5.00),
    (19, N'Large', 8.00),
    (19, N'Small', 4.50),
    (20, N'Large', 6.50),
    (20, N'Small', 4.00),
    (21, N'Large', 6.50),
    (21, N'Small', 4.00),
    (22, N'Large', 6.50),
    (22, N'Small', 4.00),
    (23, N'Large', 7.50),
    (23, N'Small', 4.50),
    (24, N'Large', 7.50),
    (24, N'Small', 4.50),
    (25, N'Large', 7.50),
    (25, N'Small', 4.50),
    (26, N'Large', 7.50),
    (26, N'Small', 4.50),
    (27, N'Large', 7.50),
    (27, N'Small', 4.50),
    (28, N'Large', 7.50),
    (28, N'Small', 4.50),
    (29, N'Large', 7.50),
    (29, N'Small', 5.00),
    (30, N'Large', 6.50),
    (30, N'Small', 4.00),
    (31, N'Large', 7.50),
    (31, N'Small', 4.50),
    (32, N'Large', 7.50),
    (32, N'Small', 4.50),
    (33, N'Large', 7.50),
    (33, N'Small', 4.50),
    (34, N'Large', 7.50),
    (34, N'Small', 5.00),
    (35, N'Large', 9.00),
    (35, N'Small', 5.00),
    (36, N'Large', 7.50),
    (36, N'Small', 5.00),
    (37, N'Regular', 1.75),
    (38, N'Regular', 1.75),
    (39, N'Regular', 1.75),
    (40, N'Regular', 1.75),
    (41, N'Regular', 1.75),
    (42, N'Regular', 2.25),
    (43, N'Regular', 2.25),
    (44, N'Regular', 2.75),
    (45, N'Regular', 2.25),
    (46, N'Regular', 2.25),
    (47, N'Regular', 2.25),
    (48, N'Regular', 2.25),
    (49, N'Regular', 2.25),
    (50, N'Regular', 2.50),
    (51, N'Regular', 2.00),
    (52, N'Large', 36.00),
    (52, N'Medium', 31.00),
    (52, N'Small', 26.00),
    (52, N'XSmall', 10.50),
    (53, N'Large', 36.00),
    (53, N'Medium', 31.00),
    (53, N'Small', 26.00),
    (53, N'XSmall', 10.50),
    (54, N'Large', 36.00),
    (54, N'Medium', 31.00),
    (54, N'Small', 26.00),
    (54, N'XSmall', 10.50),
    (55, N'Large', 36.00),
    (55, N'Medium', 31.00),
    (55, N'Small', 26.00),
    (55, N'XSmall', 10.50),
    (56, N'Large', 36.00),
    (56, N'Medium', 31.00),
    (56, N'Small', 26.00),
    (56, N'XSmall', 10.50),
    (57, N'Large', 36.00),
    (57, N'Medium', 31.00),
    (57, N'Small', 26.00),
    (57, N'XSmall', 10.50),
    (58, N'Large', 36.00),
    (58, N'Medium', 31.00),
    (58, N'Small', 26.00),
    (58, N'XSmall', 10.50),
    (59, N'Large', 36.00),
    (59, N'Medium', 31.00),
    (59, N'Small', 26.00),
    (59, N'XSmall', 10.50),
    (60, N'Large', 36.00),
    (60, N'Medium', 31.00),
    (60, N'Small', 26.00),
    (60, N'XSmall', 10.50),
    (61, N'Large', 36.00),
    (61, N'Medium', 31.00),
    (61, N'Small', 26.00),
    (61, N'XSmall', 10.50),
    (62, N'Large', 36.00),
    (62, N'Medium', 31.00),
    (62, N'Small', 26.00),
    (62, N'XSmall', 10.50),
    (63, N'Regular', 9.00),
    (64, N'Regular', 9.00),
    (65, N'Regular', 8.00),
    (66, N'Regular', 8.00),
    (67, N'Regular', 8.00),
    (68, N'Regular', 8.50),
    (69, N'Regular', 9.50),
    (70, N'Regular', 17.00),
    (71, N'Regular', 17.00),
    (72, N'Regular', 17.00),
    (73, N'Regular', 17.00),
    (74, N'Regular', 17.00),
    (75, N'Regular', 17.00),
    (76, N'Regular', 18.00),
    (77, N'Large', 25.00),
    (77, N'Medium', 11.00),
    (77, N'Small', 8.00),
    (78, N'Large', 25.00),
    (78, N'Medium', 11.00),
    (78, N'Small', 8.00),
    (79, N'Large', 25.00),
    (79, N'Medium', 11.00),
    (79, N'Small', 8.00),
    (80, N'Large', 25.00),
    (80, N'Medium', 11.00),
    (80, N'Small', 8.00),
    (81, N'Large', 25.00),
    (81, N'Medium', 11.00),
    (81, N'Small', 8.00),
    (82, N'Large', 25.00),
    (82, N'Medium', 11.00),
    (82, N'Small', 8.00),
    (83, N'Large', 25.00),
    (83, N'Medium', 11.00),
    (83, N'Small', 8.00),
    (84, N'Large', 25.00),
    (84, N'Medium', 11.00),
    (84, N'Small', 8.00),
    (85, N'Large', 25.00),
    (85, N'Medium', 11.00),
    (85, N'Small', 8.00),
    (86, N'Regular', 4.00),
    (87, N'Regular', 2.00),
    (88, N'Regular', 1.00);

/* Insert Products, capturing the new ProductId per staged row.
   Uses MERGE (always NOT MATCHED) so that the OUTPUT clause can
   reference source columns – plain INSERT … SELECT OUTPUT cannot. */
DECLARE @Inserted TABLE (ProductId INT, SeqNo INT);

MERGE INTO Products AS tgt
USING @ProductStage AS src
    ON 1 = 0  -- never matches → always INSERT
WHEN NOT MATCHED THEN
    INSERT (Name, Description, CategoryId, TaxRateId, IsActive, CreatedAt, UpdatedAt)
    VALUES (src.Name, src.Description, src.CategoryId, @StdTaxId, 1, GETDATE(), GETDATE())
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

/* ProductVariants (one or more per product, size resolved by name) */
INSERT INTO ProductVariants (ProductId, SizeId, UnitPrice, IsActive, CreatedAt, UpdatedAt)
SELECT i.ProductId, sz.SizeId, v.UnitPrice, 1, GETDATE(), GETDATE()
FROM @VariantStage v
JOIN @Inserted i ON i.SeqNo = v.ProductSeqNo
JOIN Sizes sz ON sz.Name = v.SizeName;

GO
