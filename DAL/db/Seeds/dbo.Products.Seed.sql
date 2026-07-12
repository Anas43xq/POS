/* ============================================================
   dbo.Products.Seed.sql
   Seeds Products + ProductTranslations (ar) + ProductVariants
   for the Hawa Cafeteria menu, in the normalized multilingual
   schema. Self-contained: resolves Category/TaxRate/Size ids
   by name so it can be run independently or via seed.sql.
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
    SeqNo       INT IDENTITY(1,1),
    Name        NVARCHAR(200),
    Description NVARCHAR(500),
    CategoryId  INT,
    UnitPrice   DECIMAL(10,2),
    ArabicName  NVARCHAR(200),
    SizeName    NVARCHAR(50)
);

INSERT INTO @ProductStage (Name, Description, CategoryId, UnitPrice, ArabicName, SizeName)
VALUES
    (N'Zater', N'Manakeesh Large', @ManakeeshId, 6.50, N'زعتر', N'Large'),
    (N'Zater', N'Manakeesh Small', @ManakeeshId, 4.00, N'زعتر', N'Small'),
    (N'Zater & Olives', N'Manakeesh Large', @ManakeeshId, 7.50, N'زعتر مع زيتون', N'Large'),
    (N'Zater & Olives', N'Manakeesh Small', @ManakeeshId, 4.50, N'زعتر مع زيتون', N'Small'),
    (N'Meat', N'Manakeesh Large', @ManakeeshId, 7.50, N'لحمة', N'Large'),
    (N'Meat', N'Manakeesh Small', @ManakeeshId, 4.00, N'لحمة', N'Small'),
    (N'Cheese', N'Manakeesh Large', @ManakeeshId, 7.50, N'جبنة', N'Large'),
    (N'Cheese', N'Manakeesh Small', @ManakeeshId, 4.00, N'جبنة', N'Small'),
    (N'Cheese & Meat', N'Manakeesh Large', @ManakeeshId, 8.50, N'جبن مع لحم', N'Large'),
    (N'Cheese & Meat', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع لحم', N'Small'),
    (N'Cheese & Zater', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع زعتر', N'Large'),
    (N'Cheese & Zater', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع زعتر', N'Small'),
    (N'Cheese & Baraka', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع حبة البركة', N'Large'),
    (N'Cheese & Baraka', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع حبة البركة', N'Small'),
    (N'Cheese & Veg.', N'Manakeesh Large', @ManakeeshId, 8.50, N'جبن مع خضار', N'Large'),
    (N'Cheese & Veg.', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع خضار', N'Small'),
    (N'Cheese & Muhamar', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن محمر', N'Large'),
    (N'Cheese & Muhamar', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن محمر', N'Small'),
    (N'Cheese & Egg', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع بيض', N'Large'),
    (N'Cheese & Egg', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع بيض', N'Small'),
    (N'Cheese & Chicken', N'Manakeesh Large', @ManakeeshId, 9.00, N'جبن مع دجاج', N'Large'),
    (N'Cheese & Chicken', N'Manakeesh Small', @ManakeeshId, 5.00, N'جبن مع دجاج', N'Small'),
    (N'Cheese & Olives', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع زيتون', N'Large'),
    (N'Cheese & Olives', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع زيتون', N'Small'),
    (N'Cheese & Mashrom', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع مشروم', N'Large'),
    (N'Cheese & Mashrom', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع مشروم', N'Small'),
    (N'Cheese & Hotdog', N'Manakeesh Large', @ManakeeshId, 8.50, N'جبن مع نقانق', N'Large'),
    (N'Cheese & Hotdog', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع نقانق', N'Small'),
    (N'Cheese & Labna', N'Manakeesh Large', @ManakeeshId, 8.50, N'جبن مع لبنة', N'Large'),
    (N'Cheese & Labna', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع لبنة', N'Small'),
    (N'Cheese & Honey', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع عسل', N'Large'),
    (N'Cheese & Honey', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع عسل', N'Small'),
    (N'Cheese & Sabanek', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن مع سبانخ', N'Large'),
    (N'Cheese & Sabanek', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن مع سبانخ', N'Small'),
    (N'Cheese & Veg. & Hotdog', N'Manakeesh Large', @ManakeeshId, 9.50, N'جبن مع خضار و نقانق', N'Large'),
    (N'Cheese & Veg. & Hotdog', N'Manakeesh Small', @ManakeeshId, 5.00, N'جبن مع خضار و نقانق', N'Small'),
    (N'Cheese & Oman', N'Manakeesh Large', @ManakeeshId, 8.00, N'جبن بطاطس عمان', N'Large'),
    (N'Cheese & Oman', N'Manakeesh Small', @ManakeeshId, 4.50, N'جبن بطاطس عمان', N'Small'),
    (N'Sabanek', N'Manakeesh Large', @ManakeeshId, 6.50, N'سبانخ', N'Large'),
    (N'Sabanek', N'Manakeesh Small', @ManakeeshId, 4.00, N'سبانخ', N'Small'),
    (N'Muhamar', N'Manakeesh Large', @ManakeeshId, 6.50, N'محمر', N'Large'),
    (N'Muhamar', N'Manakeesh Small', @ManakeeshId, 4.00, N'محمر', N'Small'),
    (N'Labna', N'Manakeesh Large', @ManakeeshId, 6.50, N'لبنة', N'Large'),
    (N'Labna', N'Manakeesh Small', @ManakeeshId, 4.00, N'لبنة', N'Small'),
    (N'Labna & Olives', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع زيتون', N'Large'),
    (N'Labna & Olives', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع زيتون', N'Small'),
    (N'Labna & Honey', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع عسل', N'Large'),
    (N'Labna & Honey', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع عسل', N'Small'),
    (N'Labna & Zater', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع زعتر', N'Large'),
    (N'Labna & Zater', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع زعتر', N'Small'),
    (N'Labna & Muhamar', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع محمر', N'Large'),
    (N'Labna & Muhamar', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع محمر', N'Small'),
    (N'Labna & Mashrom', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع مشروم', N'Large'),
    (N'Labna & Mashrom', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع مشروم', N'Small'),
    (N'Labna & Falafel', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع فلافل', N'Large'),
    (N'Labna & Falafel', N'Manakeesh Small', @ManakeeshId, 4.50, N'لبنة مع فلافل', N'Small'),
    (N'Labna & Hotdog', N'Manakeesh Large', @ManakeeshId, 7.50, N'لبنة مع نقانق', N'Large'),
    (N'Labna & Hotdog', N'Manakeesh Small', @ManakeeshId, 5.00, N'لبنة مع نقانق', N'Small'),
    (N'Kraft', N'Manakeesh Large', @ManakeeshId, 6.50, N'كرافت', N'Large'),
    (N'Kraft', N'Manakeesh Small', @ManakeeshId, 4.00, N'كرافت', N'Small'),
    (N'Kraft & Zater', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت زعتر', N'Large'),
    (N'Kraft & Zater', N'Manakeesh Small', @ManakeeshId, 4.50, N'كرافت زعتر', N'Small'),
    (N'Kraft & Honey', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت عسل', N'Large'),
    (N'Kraft & Honey', N'Manakeesh Small', @ManakeeshId, 4.50, N'كرافت عسل', N'Small'),
    (N'Kraft & Olives', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت زيتون', N'Large'),
    (N'Kraft & Olives', N'Manakeesh Small', @ManakeeshId, 4.50, N'كرافت زيتون', N'Small'),
    (N'Kraft & Hotdog', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت نقانق', N'Large'),
    (N'Kraft & Hotdog', N'Manakeesh Small', @ManakeeshId, 5.00, N'كرافت نقانق', N'Small'),
    (N'Kraft & Chicken', N'Manakeesh Large', @ManakeeshId, 9.00, N'كرافت دجاج', N'Large'),
    (N'Kraft & Chicken', N'Manakeesh Small', @ManakeeshId, 5.00, N'كرافت دجاج', N'Small'),
    (N'Kraft & Muhamar', N'Manakeesh Large', @ManakeeshId, 7.50, N'كرافت محمر', N'Large'),
    (N'Kraft & Muhamar', N'Manakeesh Small', @ManakeeshId, 5.00, N'كرافت محمر', N'Small'),
    (N'Meat Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'لحم', N'Regular'),
    (N'Cheese Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'جبن', N'Regular'),
    (N'Zater Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'زعتر', N'Regular'),
    (N'Muhamar Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'محمر', N'Regular'),
    (N'Sabanek Fatayer', N'Fatayer per piece', @FatayerId, 1.75, N'سبانخ', N'Regular'),
    (N'Onion Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'بصل', N'Regular'),
    (N'Pizza Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'بيتزا', N'Regular'),
    (N'Pizza & Chicken Fatayer', N'Fatayer per piece', @FatayerId, 2.75, N'بيتزا مع دجاج', N'Regular'),
    (N'Cheese & Baraka Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'جبن مع حبة البركة', N'Regular'),
    (N'Cheese & Zater Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'جبن مع زعتر', N'Regular'),
    (N'Labna & Olives Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'لبنة مع زيتون', N'Regular'),
    (N'Labna & Zater Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'لبنة مع زعتر', N'Regular'),
    (N'Labna & Falafel Fatayer', N'Fatayer per piece', @FatayerId, 2.25, N'لبنة مع فلافل', N'Regular'),
    (N'Kibbeh Maqli', N'Fatayer per piece', @FatayerId, 2.50, N'كبة مقلي', N'Regular'),
    (N'Sambosa Vegetable', N'Fatayer per piece', @FatayerId, 2.00, N'سمبوسة خضار', N'Regular'),
    (N'Meat Pizza', N'Pizza Large', @PizzaId, 36.00, N'لحم', N'Large'),
    (N'Meat Pizza', N'Pizza Medium', @PizzaId, 31.00, N'لحم', N'Medium'),
    (N'Meat Pizza', N'Pizza Small', @PizzaId, 26.00, N'لحم', N'Small'),
    (N'Meat Pizza', N'Pizza XS', @PizzaId, 10.50, N'لحم', N'XSmall'),
    (N'Cheese Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن', N'Large'),
    (N'Cheese Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن', N'Medium'),
    (N'Cheese Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن', N'Small'),
    (N'Cheese Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن', N'XSmall'),
    (N'Cheese & Meat Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع لحم', N'Large'),
    (N'Cheese & Meat Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع لحم', N'Medium'),
    (N'Cheese & Meat Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع لحم', N'Small'),
    (N'Cheese & Meat Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع لحم', N'XSmall'),
    (N'Cheese & Chicken Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع دجاج', N'Large'),
    (N'Cheese & Chicken Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع دجاج', N'Medium'),
    (N'Cheese & Chicken Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع دجاج', N'Small'),
    (N'Cheese & Chicken Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع دجاج', N'XSmall'),
    (N'Cheese & Hotdog Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع نقانق', N'Large'),
    (N'Cheese & Hotdog Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع نقانق', N'Medium'),
    (N'Cheese & Hotdog Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع نقانق', N'Small'),
    (N'Cheese & Hotdog Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع نقانق', N'XSmall'),
    (N'Cheese & Veg. Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع خضار', N'Large'),
    (N'Cheese & Veg. Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع خضار', N'Medium'),
    (N'Cheese & Veg. Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع خضار', N'Small'),
    (N'Cheese & Veg. Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع خضار', N'XSmall'),
    (N'Cheese & Veg. & Chicken Pizza', N'Pizza Large', @PizzaId, 36.00, N'جبن مع خضار و دجاج', N'Large'),
    (N'Cheese & Veg. & Chicken Pizza', N'Pizza Medium', @PizzaId, 31.00, N'جبن مع خضار و دجاج', N'Medium'),
    (N'Cheese & Veg. & Chicken Pizza', N'Pizza Small', @PizzaId, 26.00, N'جبن مع خضار و دجاج', N'Small'),
    (N'Cheese & Veg. & Chicken Pizza', N'Pizza XS', @PizzaId, 10.50, N'جبن مع خضار و دجاج', N'XSmall'),
    (N'Vegetables Pizza', N'Pizza Large', @PizzaId, 36.00, N'خضار', N'Large'),
    (N'Vegetables Pizza', N'Pizza Medium', @PizzaId, 31.00, N'خضار', N'Medium'),
    (N'Vegetables Pizza', N'Pizza Small', @PizzaId, 26.00, N'خضار', N'Small'),
    (N'Vegetables Pizza', N'Pizza XS', @PizzaId, 10.50, N'خضار', N'XSmall'),
    (N'Shrimp Pizza', N'Pizza Large', @PizzaId, 36.00, N'روبيان', N'Large'),
    (N'Shrimp Pizza', N'Pizza Medium', @PizzaId, 31.00, N'روبيان', N'Medium'),
    (N'Shrimp Pizza', N'Pizza Small', @PizzaId, 26.00, N'روبيان', N'Small'),
    (N'Shrimp Pizza', N'Pizza XS', @PizzaId, 10.50, N'روبيان', N'XSmall'),
    (N'Chicken Pizza', N'Pizza Large', @PizzaId, 36.00, N'دجاج', N'Large'),
    (N'Chicken Pizza', N'Pizza Medium', @PizzaId, 31.00, N'دجاج', N'Medium'),
    (N'Chicken Pizza', N'Pizza Small', @PizzaId, 26.00, N'دجاج', N'Small'),
    (N'Chicken Pizza', N'Pizza XS', @PizzaId, 10.50, N'دجاج', N'XSmall'),
    (N'Pepperoni Pizza', N'Pizza Large', @PizzaId, 36.00, N'بيبروني', N'Large'),
    (N'Pepperoni Pizza', N'Pizza Medium', @PizzaId, 31.00, N'بيبروني', N'Medium'),
    (N'Pepperoni Pizza', N'Pizza Small', @PizzaId, 26.00, N'بيبروني', N'Small'),
    (N'Pepperoni Pizza', N'Pizza XS', @PizzaId, 10.50, N'بيبروني', N'XSmall'),
    (N'Meat Shakhtoura', N'Shakhtoura', @ShakhtouraId, 9.00, N'لحم', N'Regular'),
    (N'Cheese Shakhtoura', N'Shakhtoura', @ShakhtouraId, 9.00, N'جبن', N'Regular'),
    (N'Zater Shakhtoura', N'Shakhtoura', @ShakhtouraId, 8.00, N'زعتر', N'Regular'),
    (N'Muhamar Shakhtoura', N'Shakhtoura', @ShakhtouraId, 8.00, N'محمر', N'Regular'),
    (N'Sabanek Shakhtoura', N'Shakhtoura', @ShakhtouraId, 8.00, N'سبانخ', N'Regular'),
    (N'Labneh Shakhtoura', N'Shakhtoura', @ShakhtouraId, 8.50, N'لبنة', N'Regular'),
    (N'Mix Shakhtoura', N'Shakhtoura', @ShakhtouraId, 9.50, N'مكس', N'Regular'),
    (N'Meat Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'لحم', N'Regular'),
    (N'Cheese Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'جبن', N'Regular'),
    (N'Zater Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'زعتر', N'Regular'),
    (N'Muhamar Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'محمر', N'Regular'),
    (N'Sabanek Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'سبانخ', N'Regular'),
    (N'Labneh Farshouha', N'Farshouha', @FarshouhaId, 17.00, N'لبنة', N'Regular'),
    (N'Mix Farshouha', N'Farshouha', @FarshouhaId, 18.00, N'مكس', N'Regular'),
    (N'Orange Juice', N'Juice Large', @JuicesId, 25.00, N'برتقال', N'Large'),
    (N'Orange Juice', N'Juice Medium', @JuicesId, 11.00, N'برتقال', N'Medium'),
    (N'Orange Juice', N'Juice Small', @JuicesId, 8.00, N'برتقال', N'Small'),
    (N'Mango Juice', N'Juice Large', @JuicesId, 25.00, N'مانجو', N'Large'),
    (N'Mango Juice', N'Juice Medium', @JuicesId, 11.00, N'مانجو', N'Medium'),
    (N'Mango Juice', N'Juice Small', @JuicesId, 8.00, N'مانجو', N'Small'),
    (N'Pomegranate Juice', N'Juice Large', @JuicesId, 25.00, N'رمان', N'Large'),
    (N'Pomegranate Juice', N'Juice Medium', @JuicesId, 11.00, N'رمان', N'Medium'),
    (N'Pomegranate Juice', N'Juice Small', @JuicesId, 8.00, N'رمان', N'Small'),
    (N'Strawberry Juice', N'Juice Large', @JuicesId, 25.00, N'فراولة', N'Large'),
    (N'Strawberry Juice', N'Juice Medium', @JuicesId, 11.00, N'فراولة', N'Medium'),
    (N'Strawberry Juice', N'Juice Small', @JuicesId, 8.00, N'فراولة', N'Small'),
    (N'Lemon Juice', N'Juice Large', @JuicesId, 25.00, N'ليمون', N'Large'),
    (N'Lemon Juice', N'Juice Medium', @JuicesId, 11.00, N'ليمون', N'Medium'),
    (N'Lemon Juice', N'Juice Small', @JuicesId, 8.00, N'ليمون', N'Small'),
    (N'Lemon with Mint Juice', N'Juice Large', @JuicesId, 25.00, N'ليمون و نعناع', N'Large'),
    (N'Lemon with Mint Juice', N'Juice Medium', @JuicesId, 11.00, N'ليمون و نعناع', N'Medium'),
    (N'Lemon with Mint Juice', N'Juice Small', @JuicesId, 8.00, N'ليمون و نعناع', N'Small'),
    (N'Cocktail Juice', N'Juice Large', @JuicesId, 25.00, N'كوكتيل', N'Large'),
    (N'Cocktail Juice', N'Juice Medium', @JuicesId, 11.00, N'كوكتيل', N'Medium'),
    (N'Cocktail Juice', N'Juice Small', @JuicesId, 8.00, N'كوكتيل', N'Small'),
    (N'Avocado Juice', N'Juice Large', @JuicesId, 25.00, N'افوكادو', N'Large'),
    (N'Avocado Juice', N'Juice Medium', @JuicesId, 11.00, N'افوكادو', N'Medium'),
    (N'Avocado Juice', N'Juice Small', @JuicesId, 8.00, N'افوكادو', N'Small'),
    (N'Apple Juice', N'Juice Large', @JuicesId, 25.00, N'تفاح', N'Large'),
    (N'Apple Juice', N'Juice Medium', @JuicesId, 11.00, N'تفاح', N'Medium'),
    (N'Apple Juice', N'Juice Small', @JuicesId, 8.00, N'تفاح', N'Small'),
    (N'Soft Drinks', N'Soft Drinks', @JuicesId, 4.00, N'غازيات', N'Regular'),
    (N'Melco', N'Melco', @JuicesId, 2.00, N'ملكو', N'Regular'),
    (N'Water', N'Water', @JuicesId, 1.00, N'ماء', N'Regular');

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

/* ProductVariants (one per product, size resolved by name) */
INSERT INTO ProductVariants (ProductId, SizeId, UnitPrice, IsActive, CreatedAt, UpdatedAt)
SELECT i.ProductId, sz.SizeId, s.UnitPrice, 1, GETDATE(), GETDATE()
FROM @Inserted i
JOIN @ProductStage s ON s.SeqNo = i.SeqNo
JOIN Sizes sz ON sz.Name = s.SizeName;

GO
