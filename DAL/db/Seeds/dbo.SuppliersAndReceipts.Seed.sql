/* ============================================================
   dbo.SuppliersAndReceipts.Seed.sql
   Seeds PurchaseReceiptTypes + Suppliers + PurchaseReceipts + Sales Transactions
   ============================================================ */

/* Ensure receipt types exist */
IF NOT EXISTS (SELECT 1 FROM PurchaseReceiptTypes WHERE Name = 'VAT')
    INSERT INTO PurchaseReceiptTypes (ReceiptTypeId, Name) VALUES (1, 'VAT');
IF NOT EXISTS (SELECT 1 FROM PurchaseReceiptTypes WHERE Name = 'Non-VAT')
    INSERT INTO PurchaseReceiptTypes (ReceiptTypeId, Name) VALUES (2, 'Non-VAT');

DECLARE @VatTypeId INT = ISNULL((SELECT ReceiptTypeId FROM PurchaseReceiptTypes WHERE Name = 'VAT'), 1);
DECLARE @NonVatTypeId INT = ISNULL((SELECT ReceiptTypeId FROM PurchaseReceiptTypes WHERE Name = 'Non-VAT'), 2);

-- ============================================================
-- SEED SUPPLIERS
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE CompanyName = N'Emirates Foodstuff Trading')
    INSERT INTO Suppliers (CompanyName, TRN, Address, Phone, Email, Notes, CreatedAt)
    VALUES (N'Emirates Foodstuff Trading', N'100234567890', N'Dubai Industrial City, Dubai', N'+971-4-1234567', N'orders@emiratesfood.ae', N'Main supplier for food items', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE CompanyName = N'Al Rawabi Dairy Co.')
    INSERT INTO Suppliers (CompanyName, TRN, Address, Phone, Email, Notes, CreatedAt)
    VALUES (N'Al Rawabi Dairy Co.', N'100345678901', N'Al Ain Road, Dubai', N'+971-4-2345678', N'sales@alrawabi.ae', N'Dairy products supplier', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE CompanyName = N'Fresh Supplies LLC')
    INSERT INTO Suppliers (CompanyName, TRN, Address, Phone, Email, Notes, CreatedAt)
    VALUES (N'Fresh Supplies LLC', N'100456789012', N'Jebel Ali Free Zone, Dubai', N'+971-4-3456789', N'info@freshsupplies.ae', N'Vegetables and fruits supplier', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE CompanyName = N'Arabian Beverages')
    INSERT INTO Suppliers (CompanyName, TRN, Address, Phone, Email, Notes, CreatedAt)
    VALUES (N'Arabian Beverages', N'100567890123', N'Sheikh Zayed Road, Dubai', N'+971-4-4567890', N'delivery@arabianbev.ae', N'Beverages and juices supplier', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE CompanyName = N'Prime Meats Trading')
    INSERT INTO Suppliers (CompanyName, TRN, Address, Phone, Email, Notes, CreatedAt)
    VALUES (N'Prime Meats Trading', N'100678901234', N'Deira, Dubai', N'+971-4-5678901', N'orders@primemeats.ae', N'Meat products supplier', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE CompanyName = N'General Paper Products')
    INSERT INTO Suppliers (CompanyName, TRN, Address, Phone, Email, Notes, CreatedAt)
    VALUES (N'General Paper Products', N'100789012345', N'Bur Dubai, Dubai', N'+971-4-6789012', N'sales@genpaper.ae', N'Packaging materials supplier', GETDATE());

DECLARE @Supplier1 INT = (SELECT SupplierId FROM Suppliers WHERE CompanyName = N'Emirates Foodstuff Trading');
DECLARE @Supplier2 INT = (SELECT SupplierId FROM Suppliers WHERE CompanyName = N'Al Rawabi Dairy Co.');
DECLARE @Supplier3 INT = (SELECT SupplierId FROM Suppliers WHERE CompanyName = N'Fresh Supplies LLC');
DECLARE @Supplier4 INT = (SELECT SupplierId FROM Suppliers WHERE CompanyName = N'Arabian Beverages');
DECLARE @Supplier5 INT = (SELECT SupplierId FROM Suppliers WHERE CompanyName = N'Prime Meats Trading');
DECLARE @Supplier6 INT = (SELECT SupplierId FROM Suppliers WHERE CompanyName = N'General Paper Products');

-- ============================================================
-- SEED VAT PURCHASE RECEIPTS
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-001')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@VatTypeId, @Supplier1, N'INV-2024-001', '2024-01-15', N'Food Items', N'Monthly food supplies', 2500.00, 5.00, 125.00, 2625.00, N'Paid via bank transfer', NULL, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-002')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@VatTypeId, @Supplier2, N'INV-2024-002', '2024-01-15', N'Dairy Products', N'Milk, Yogurt, Cheese', 800.00, 5.00, 40.00, 840.00, N'Cash on delivery', NULL, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-003')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@VatTypeId, @Supplier3, N'INV-2024-003', '2024-01-16', N'Fresh Produce', N'Vegetables and fruits', 450.00, 5.00, 22.50, 472.50, N'Paid via bank transfer', NULL, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-004')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@VatTypeId, @Supplier4, N'INV-2024-004', '2024-01-16', N'Beverages', N'Juices and soft drinks', 1200.00, 5.00, 60.00, 1260.00, N'Cheque payment', NULL, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-005')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@VatTypeId, @Supplier5, N'INV-2024-005', '2024-01-17', N'Meat Products', N'Chicken and meat supplies', 1800.00, 5.00, 90.00, 1890.00, N'Paid via bank transfer', NULL, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-006')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@VatTypeId, @Supplier6, N'INV-2024-006', '2024-01-17', N'Packaging', N'Paper cups, plates, napkins', 350.00, 5.00, 17.50, 367.50, N'Cash on delivery', NULL, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-007')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@VatTypeId, @Supplier1, N'INV-2024-007', '2024-01-18', N'Food Items', N'Spices and condiments', 650.00, 5.00, 32.50, 682.50, N'Cheque payment', NULL, 1, GETDATE());

-- ============================================================
-- SEED NON-VAT PURCHASE RECEIPTS
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-NV01')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@NonVatTypeId, @Supplier6, N'INV-2024-NV01', '2024-01-15', N'Cleaning Supplies', N'Detergents and sanitizers', 280.00, 0.00, 0.00, 280.00, N'No VAT applicable', NULL, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-NV02')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@NonVatTypeId, @Supplier6, N'INV-2024-NV02', '2024-01-16', N'Stationery', N'Printer paper and pens', 120.00, 0.00, 0.00, 120.00, N'No VAT applicable', NULL, 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM PurchaseReceipts WHERE InvoiceNumber = N'INV-2024-NV03')
    INSERT INTO PurchaseReceipts (ReceiptTypeId, SupplierId, InvoiceNumber, InvoiceDate, Category, Description, Subtotal, VatRate, VatAmount, GrandTotal, Notes, ImagePath, CreatedBy, CreatedAt)
    VALUES (@NonVatTypeId, @Supplier1, N'INV-2024-NV03', '2024-01-17', N'Maintenance', N'Kitchen equipment maintenance', 450.00, 0.00, 0.00, 450.00, N'Service charge - No VAT', NULL, 1, GETDATE());

GO

/* ============================================================
   SALES TRANSACTIONS SEED
   Moved to dbo.SalesData.Seed.sql
   ============================================================ */
