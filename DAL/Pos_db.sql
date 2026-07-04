USE master;
GO

/* Drop and recreate the database from scratch */
ALTER DATABASE POS_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE IF EXISTS POS_DB;
GO

CREATE DATABASE POS_DB;
GO

USE POS_DB;
GO

SET NOCOUNT ON;
GO

/* 1. USERS & SESSIONS */

CREATE TABLE Users
(
    UserId       INT IDENTITY(1,1) NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    Username     NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsActive     BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    CreatedAt    DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt    DATETIME2(0) NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT CK_Users_IsActive CHECK (IsActive IN (0, 1))
);
GO

CREATE TABLE Sessions
(
    SessionId INT IDENTITY(1,1) NOT NULL,
    UserId    INT NOT NULL,
    LoginAt   DATETIME2(0) NOT NULL CONSTRAINT DF_Sessions_LoginAt DEFAULT SYSUTCDATETIME(),
    LogoutAt  DATETIME2(0) NULL,

    CONSTRAINT PK_Sessions PRIMARY KEY (SessionId),
    CONSTRAINT FK_Sessions_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CK_Sessions_LogoutAfterLogin CHECK (LogoutAt IS NULL OR LogoutAt >= LoginAt)
);
GO

/* 2. PRODUCT CATALOG */

CREATE TABLE Categories
(
    CategoryId       INT IDENTITY(1,1) NOT NULL,
    Name             NVARCHAR(100) NOT NULL,
    ParentCategoryId INT NULL,
    Description      NVARCHAR(255) NULL,
    CreatedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_Categories_UpdatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentCategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT UQ_Categories_Name_Parent UNIQUE (Name, ParentCategoryId),
    CONSTRAINT CK_Categories_NoSelfParent CHECK (ParentCategoryId IS NULL OR ParentCategoryId <> CategoryId)
);
GO

CREATE TABLE TaxRates
(
    TaxRateId INT IDENTITY(1,1) NOT NULL,
    Name      NVARCHAR(100) NOT NULL,
    Rate      DECIMAL(9,4) NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_TaxRates_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_TaxRates_UpdatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_TaxRates PRIMARY KEY (TaxRateId),
    CONSTRAINT UQ_TaxRates_Name UNIQUE (Name),
    CONSTRAINT CK_TaxRates_Rate CHECK (Rate >= 0 AND Rate <= 1)
);
GO

CREATE TABLE Products
(
    ProductId     INT IDENTITY(1,1) NOT NULL,
    Name          NVARCHAR(150) NOT NULL,
    CategoryId    INT NOT NULL,
    Description   NVARCHAR(255) NULL,
    UnitPrice     DECIMAL(18,2) NOT NULL,
    CostPrice     DECIMAL(18,2) NOT NULL CONSTRAINT DF_Products_CostPrice DEFAULT 0,
    TaxRateId     INT NOT NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT 1,
    CreatedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_Products_UpdatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Products PRIMARY KEY (ProductId),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT FK_Products_TaxRates FOREIGN KEY (TaxRateId) REFERENCES TaxRates(TaxRateId),
    CONSTRAINT CK_Products_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_Products_CostPrice CHECK (CostPrice >= 0),
    CONSTRAINT CK_Products_IsActive CHECK (IsActive IN (0, 1))
);
GO

/* 4. SHIFTS */

CREATE TABLE Shifts
(
    ShiftId        INT IDENTITY(1,1) NOT NULL,
    UserId         INT NOT NULL,
    OpenedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Shifts_OpenedAt DEFAULT SYSUTCDATETIME(),
    ClosedAt       DATETIME2(0) NULL,
    OpeningCash    DECIMAL(18,2) NOT NULL CONSTRAINT DF_Shifts_OpeningCash DEFAULT 0,
    ClosingCash    DECIMAL(18,2) NULL,
    ExpectedCash   DECIMAL(18,2) NULL,
    CashDifference DECIMAL(18,2) NULL,
    Status         TINYINT NOT NULL CONSTRAINT DF_Shifts_Status DEFAULT 1,

    CONSTRAINT PK_Shifts PRIMARY KEY (ShiftId),
    CONSTRAINT FK_Shifts_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CK_Shifts_Status CHECK (Status IN (0, 1)), /* 0=Closed, 1=Open */
    CONSTRAINT CK_Shifts_OpeningCash CHECK (OpeningCash >= 0),
    CONSTRAINT CK_Shifts_ClosingCash CHECK (ClosingCash IS NULL OR ClosingCash >= 0),
    CONSTRAINT CK_Shifts_ExpectedCash CHECK (ExpectedCash IS NULL OR ExpectedCash >= 0),
    CONSTRAINT CK_Shifts_ClosedAfterOpened CHECK (ClosedAt IS NULL OR ClosedAt >= OpenedAt)
);
GO

/* 6. TRANSACTIONS */

CREATE TABLE Transactions
(
    TransactionId   INT IDENTITY(1,1) NOT NULL,
    ReceiptNumber   NVARCHAR(30) NOT NULL,
    ShiftId         INT NOT NULL,
    CashierId       INT NOT NULL,
    TransactionDate DATETIME2(0) NOT NULL CONSTRAINT DF_Transactions_TransactionDate DEFAULT SYSUTCDATETIME(),
    Subtotal        DECIMAL(18,2) NOT NULL CONSTRAINT DF_Transactions_Subtotal DEFAULT 0,
    TaxTotal        DECIMAL(18,2) NOT NULL CONSTRAINT DF_Transactions_TaxTotal DEFAULT 0,
    GrandTotal      DECIMAL(18,2) NOT NULL CONSTRAINT DF_Transactions_GrandTotal DEFAULT 0,
    Status          TINYINT NOT NULL CONSTRAINT DF_Transactions_Status DEFAULT 0,
    Notes           NVARCHAR(500) NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Transactions_CreatedAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Transactions PRIMARY KEY (TransactionId),
    CONSTRAINT UQ_Transactions_ReceiptNumber UNIQUE (ReceiptNumber),
    CONSTRAINT FK_Transactions_Shifts FOREIGN KEY (ShiftId) REFERENCES Shifts(ShiftId),
    CONSTRAINT FK_Transactions_Cashier FOREIGN KEY (CashierId) REFERENCES Users(UserId),
    CONSTRAINT CK_Transactions_Status CHECK (Status IN (0, 1, 2)), /* 0=Pending, 1=Completed, 2=Voided */
    CONSTRAINT CK_Transactions_Subtotal CHECK (Subtotal >= 0),
    CONSTRAINT CK_Transactions_TaxTotal CHECK (TaxTotal >= 0),
    CONSTRAINT CK_Transactions_GrandTotal CHECK (GrandTotal >= 0)
);
GO

CREATE TABLE TransactionItems
(
    TransactionItemId INT IDENTITY(1,1) NOT NULL,
    TransactionId     INT NOT NULL,
    ProductId         INT NOT NULL,
    ProductName       NVARCHAR(150) NOT NULL,
    UnitPrice         DECIMAL(18,2) NOT NULL,
    Quantity          INT NOT NULL CONSTRAINT DF_TransactionItems_Quantity DEFAULT 1,
    TaxRate           DECIMAL(9,4) NOT NULL,
    LineSubtotal      DECIMAL(18,2) NOT NULL,
    LineTax           DECIMAL(18,2) NOT NULL,
    LineTotal         DECIMAL(18,2) NOT NULL,

    CONSTRAINT PK_TransactionItems PRIMARY KEY (TransactionItemId),
    CONSTRAINT FK_TransactionItems_Transactions FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId),
    CONSTRAINT FK_TransactionItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT CK_TransactionItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_TransactionItems_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_TransactionItems_TaxRate CHECK (TaxRate >= 0 AND TaxRate <= 1), 
    CONSTRAINT CK_TransactionItems_LineSubtotal CHECK (LineSubtotal >= 0),
    CONSTRAINT CK_TransactionItems_LineTax CHECK (LineTax >= 0),
    CONSTRAINT CK_TransactionItems_LineTotal CHECK (LineTotal >= 0)
);
GO

CREATE TABLE Payments
(
    PaymentId       INT IDENTITY(1,1) NOT NULL,
    TransactionId   INT NOT NULL,
    PaymentMethod   NVARCHAR(20) NOT NULL,
    AmountTendered  DECIMAL(18,2) NOT NULL,
    ChangeGiven     DECIMAL(18,2) NOT NULL CONSTRAINT DF_Payments_ChangeGiven DEFAULT 0,
    ReferenceNumber NVARCHAR(100) NULL,
    PaidAt          DATETIME2(0) NOT NULL CONSTRAINT DF_Payments_PaidAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Payments PRIMARY KEY (PaymentId),
    CONSTRAINT FK_Payments_Transactions FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId),
    CONSTRAINT CK_Payments_Method CHECK (PaymentMethod IN ('Cash', 'Card')),
    CONSTRAINT CK_Payments_AmountTendered CHECK (AmountTendered > 0),
    CONSTRAINT CK_Payments_ChangeGiven CHECK (ChangeGiven >= 0)
);
GO

/* 8. AUDIT LOGS */

CREATE TABLE AuditLogs
(
    AuditLogId INT IDENTITY(1,1) NOT NULL,
    UserId     INT NULL,
    ActionType NVARCHAR(100) NOT NULL,
    EntityName NVARCHAR(100) NULL,
    EntityId   INT NULL,
    OldValue   NVARCHAR(MAX) NULL,
    NewValue   NVARCHAR(MAX) NULL,
    OccurredAt DATETIME2(0) NOT NULL CONSTRAINT DF_AuditLogs_OccurredAt DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_AuditLogs PRIMARY KEY (AuditLogId),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

/* 9. INDEXES */

CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Sessions_UserId ON Sessions(UserId);
CREATE INDEX IX_Sessions_LoginAt ON Sessions(LoginAt);
CREATE INDEX IX_Categories_ParentCategoryId ON Categories(ParentCategoryId);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_TaxRateId ON Products(TaxRateId);
CREATE INDEX IX_Shifts_UserId ON Shifts(UserId);
CREATE INDEX IX_Shifts_Status ON Shifts(Status);
CREATE UNIQUE INDEX UX_Shifts_OpenShift_User ON Shifts(UserId) WHERE Status = 1;
CREATE INDEX IX_Transactions_ShiftId ON Transactions(ShiftId);
CREATE INDEX IX_Transactions_CashierId ON Transactions(CashierId);
CREATE INDEX IX_Transactions_TransactionDate ON Transactions(TransactionDate);
CREATE INDEX IX_Transactions_Status ON Transactions(Status);
CREATE INDEX IX_TransactionItems_TransactionId ON TransactionItems(TransactionId);
CREATE INDEX IX_TransactionItems_ProductId ON TransactionItems(ProductId);
CREATE INDEX IX_Payments_TransactionId ON Payments(TransactionId);
CREATE INDEX IX_Payments_PaidAt ON Payments(PaidAt);
CREATE INDEX IX_Payments_Method ON Payments(PaymentMethod);
CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_OccurredAt ON AuditLogs(OccurredAt);
CREATE INDEX IX_AuditLogs_ActionType ON AuditLogs(ActionType);
GO

/* 10. SEED DATA */

INSERT INTO TaxRates (Name, Rate)
VALUES
    ('Standard',   0.05),
    ('Zero Rated', 0.0000);
GO

INSERT INTO Categories (Name, ParentCategoryId, Description)
VALUES
    (N'Food',        NULL, N'Food products')
GO

/* Admin user seed */
INSERT INTO Users (FullName, Username, PasswordHash, IsActive)
VALUES
    ('System Admin', 'admin', '$2a$11$tC9LpOUxNhnYh7cfztcMAuJ3tdlgI5r3/Xaq0ApeoXTT7Qb3VATA6', 1),
    ('System Cashier', 'cashier', '$2a$12$Zwju2F5K5taFZu3qp3BxleeFyLW5t6qWN1dbEloAo1v/RjjsgPFwC', 1),
    ('System Manager', 'manager', '$2a$12$YKoX8tZxO9wrNqHyKmPZPeH7WVDn86.H2aZNA2TyyPwobMFg1BP9C', 1);
GO

/* Hawa Cafeteria menu categories & products */
DECLARE @FoodId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Food' AND ParentCategoryId IS NULL);

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Manakeesh (مناقيش)' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description) VALUES (N'Manakeesh (مناقيش)', @FoodId, N'منيو مناقيش');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Fatayer (فطائر)' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description) VALUES (N'Fatayer (فطائر)', @FoodId, N'منيو فطائر');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Pizza (بيتزا)' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description) VALUES (N'Pizza (بيتزا)', @FoodId, N'منيو بيتزا');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Shakhtoura (شختورة)' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description) VALUES (N'Shakhtoura (شختورة)', @FoodId, N'منيو شختورة');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Farshouha (فرشوحة)' AND ParentCategoryId = @FoodId)
    INSERT INTO Categories (Name, ParentCategoryId, Description) VALUES (N'Farshouha (فرشوحة)', @FoodId, N'منيو فرشوحة');
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Juices (عصائر)' AND ParentCategoryId IS NULL)
    INSERT INTO Categories (Name, ParentCategoryId, Description) VALUES (N'Juices (عصائر)', NULL, N'منيو عصائر');

DECLARE @ManakeeshId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Manakeesh (مناقيش)' AND ParentCategoryId = @FoodId);
DECLARE @FatayerId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Fatayer (فطائر)' AND ParentCategoryId = @FoodId);
DECLARE @PizzaId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Pizza (بيتزا)' AND ParentCategoryId = @FoodId);
DECLARE @ShakhtouraId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Shakhtoura (شختورة)' AND ParentCategoryId = @FoodId);
DECLARE @FarshouhaId INT = (SELECT CategoryId FROM Categories WHERE Name = N'Farshouha (فرشوحة)' AND ParentCategoryId = @FoodId);
DECLARE @JuicesId2 INT = (SELECT CategoryId FROM Categories WHERE Name = N'Juices (عصائر)');

INSERT INTO Products
    (Name, CategoryId, Description, UnitPrice, TaxRateId, IsActive)
VALUES
    (N'Zater (زعتر) - Large',  @ManakeeshId, N'Manakeesh Large', 6.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Zater (زعتر) - Small',  @ManakeeshId, N'Manakeesh Small', 4.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Zater & Olives (زعتر مع زيتون) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Zater & Olives (زعتر مع زيتون) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Meat (لحمة) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Meat (لحمة) - Small',  @ManakeeshId, N'Manakeesh Small', 4.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese (جبنة) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese (جبنة) - Small',  @ManakeeshId, N'Manakeesh Small', 4.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Meat (جبن مع لحم) - Large',  @ManakeeshId, N'Manakeesh Large', 8.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Meat (جبن مع لحم) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Zater (جبن مع زعتر) - Large',  @ManakeeshId, N'Manakeesh Large', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Zater (جبن مع زعتر) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Baraka (جبن مع حبة البركة) - Large',  @ManakeeshId, N'Manakeesh Large', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Baraka (جبن مع حبة البركة) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. (جبن مع خضار) - Large',  @ManakeeshId, N'Manakeesh Large', 8.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. (جبن مع خضار) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Muhamar (جبن محمر) - Large',  @ManakeeshId, N'Manakeesh Large', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Muhamar (جبن محمر) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Egg (جبن مع بيض) - Large',  @ManakeeshId, N'Manakeesh Large', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Egg (جبن مع بيض) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Chicken (جبن مع دجاج) - Large',  @ManakeeshId, N'Manakeesh Large', 9.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Chicken (جبن مع دجاج) - Small',  @ManakeeshId, N'Manakeesh Small', 5.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Olives (جبن مع زيتون) - Large',  @ManakeeshId, N'Manakeesh Large', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Olives (جبن مع زيتون) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Mashrom (جبن مع مشروم) - Large',  @ManakeeshId, N'Manakeesh Large', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Mashrom (جبن مع مشروم) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Hotdog (جبن مع نقانق) - Large',  @ManakeeshId, N'Manakeesh Large', 8.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Hotdog (جبن مع نقانق) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Labna (جبن مع لبنة) - Large',  @ManakeeshId, N'Manakeesh Large', 8.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Labna (جبن مع لبنة) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Honey (جبن مع عسل) - Large',  @ManakeeshId, N'Manakeesh Large', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Honey (جبن مع عسل) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Sabanek (جبن مع سبانخ) - Large',  @ManakeeshId, N'Manakeesh Large', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Sabanek (جبن مع سبانخ) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. & Hotdog (جبن مع خضار و نقانق) - Large',  @ManakeeshId, N'Manakeesh Large', 9.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. & Hotdog (جبن مع خضار و نقانق) - Small',  @ManakeeshId, N'Manakeesh Small', 5.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Oman (جبن بطاطس عمان) - Large',  @ManakeeshId, N'Manakeesh Large', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Oman (جبن بطاطس عمان) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Sabanek (سبانخ) - Large',  @ManakeeshId, N'Manakeesh Large', 6.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Sabanek (سبانخ) - Small',  @ManakeeshId, N'Manakeesh Small', 4.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Muhamar (محمر) - Large',  @ManakeeshId, N'Manakeesh Large', 6.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Muhamar (محمر) - Small',  @ManakeeshId, N'Manakeesh Small', 4.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna (لبنة) - Large',  @ManakeeshId, N'Manakeesh Large', 6.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna (لبنة) - Small',  @ManakeeshId, N'Manakeesh Small', 4.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Olives (لبنة مع زيتون) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Olives (لبنة مع زيتون) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Honey (لبنة مع عسل) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Honey (لبنة مع عسل) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Zater (لبنة مع زعتر) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Zater (لبنة مع زعتر) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Muhamar (لبنة مع محمر) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Muhamar (لبنة مع محمر) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Mashrom (لبنة مع مشروم) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Mashrom (لبنة مع مشروم) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Falafel (لبنة مع فلافل) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Falafel (لبنة مع فلافل) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Hotdog (لبنة مع نقانق) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Hotdog (لبنة مع نقانق) - Small',  @ManakeeshId, N'Manakeesh Small', 5.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft (كرافت) - Large',  @ManakeeshId, N'Manakeesh Large', 6.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft (كرافت) - Small',  @ManakeeshId, N'Manakeesh Small', 4.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Zater (كرافت زعتر) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Zater (كرافت زعتر) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Honey (كرافت عسل) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Honey (كرافت عسل) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Olives (كرافت زيتون) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Olives (كرافت زيتون) - Small',  @ManakeeshId, N'Manakeesh Small', 4.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Hotdog (كرافت نقانق) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Hotdog (كرافت نقانق) - Small',  @ManakeeshId, N'Manakeesh Small', 5.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Chicken (كرافت دجاج) - Large',  @ManakeeshId, N'Manakeesh Large', 9.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Chicken (كرافت دجاج) - Small',  @ManakeeshId, N'Manakeesh Small', 5.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Muhamar (كرافت محمر) - Large',  @ManakeeshId, N'Manakeesh Large', 7.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kraft & Muhamar (كرافت محمر) - Small',  @ManakeeshId, N'Manakeesh Small', 5.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Meat Fatayer (لحم)',  @FatayerId, N'Fatayer per piece', 1.75, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese Fatayer (جبن)',  @FatayerId, N'Fatayer per piece', 1.75, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Zater Fatayer (زعتر)',  @FatayerId, N'Fatayer per piece', 1.75, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Muhamar Fatayer (محمر)',  @FatayerId, N'Fatayer per piece', 1.75, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Sabanek Fatayer (سبانخ)',  @FatayerId, N'Fatayer per piece', 1.75, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Onion Fatayer (بصل)',  @FatayerId, N'Fatayer per piece', 2.25, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Pizza Fatayer (بيتزا)',  @FatayerId, N'Fatayer per piece', 2.25, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Pizza & Chicken Fatayer (بيتزا مع دجاج)',  @FatayerId, N'Fatayer per piece', 2.75, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Baraka Fatayer (جبن مع حبة البركة)',  @FatayerId, N'Fatayer per piece', 2.25, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Zater Fatayer (جبن مع زعتر)',  @FatayerId, N'Fatayer per piece', 2.25, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Olives Fatayer (لبنة مع زيتون)',  @FatayerId, N'Fatayer per piece', 2.25, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Zater Fatayer (لبنة مع زعتر)',  @FatayerId, N'Fatayer per piece', 2.25, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labna & Falafel Fatayer (لبنة مع فلافل)',  @FatayerId, N'Fatayer per piece', 2.25, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Kibbeh Maqli (كبة مقلي)',  @FatayerId, N'Fatayer per piece', 2.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Sambosa Vegetable (سمبوسة خضار)',  @FatayerId, N'Fatayer per piece', 2.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Meat Pizza (لحم) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Meat Pizza (لحم) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Meat Pizza (لحم) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Meat Pizza (لحم) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese Pizza (جبن) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese Pizza (جبن) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese Pizza (جبن) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese Pizza (جبن) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Meat Pizza (جبن مع لحم) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Meat Pizza (جبن مع لحم) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Meat Pizza (جبن مع لحم) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Meat Pizza (جبن مع لحم) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Chicken Pizza (جبن مع دجاج) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Chicken Pizza (جبن مع دجاج) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Chicken Pizza (جبن مع دجاج) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Chicken Pizza (جبن مع دجاج) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Hotdog Pizza (جبن مع نقانق) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Hotdog Pizza (جبن مع نقانق) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Hotdog Pizza (جبن مع نقانق) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Hotdog Pizza (جبن مع نقانق) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. Pizza (جبن مع خضار) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. Pizza (جبن مع خضار) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. Pizza (جبن مع خضار) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. Pizza (جبن مع خضار) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. & Chicken Pizza (جبن مع خضار و دجاج) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. & Chicken Pizza (جبن مع خضار و دجاج) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. & Chicken Pizza (جبن مع خضار و دجاج) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese & Veg. & Chicken Pizza (جبن مع خضار و دجاج) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Vegetables Pizza (خضار) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Vegetables Pizza (خضار) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Vegetables Pizza (خضار) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Vegetables Pizza (خضار) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Shrimp Pizza (روبيان) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Shrimp Pizza (روبيان) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Shrimp Pizza (روبيان) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Shrimp Pizza (روبيان) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Chicken Pizza (دجاج) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Chicken Pizza (دجاج) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Chicken Pizza (دجاج) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Chicken Pizza (دجاج) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Pepperoni Pizza (بيبروني) - Large',  @PizzaId, N'Pizza Large', 36.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Pepperoni Pizza (بيبروني) - Medium',  @PizzaId, N'Pizza Medium', 31.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Pepperoni Pizza (بيبروني) - Small',  @PizzaId, N'Pizza Small', 26.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Pepperoni Pizza (بيبروني) - XSmall',  @PizzaId, N'Pizza XS', 10.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Meat Shakhtoura (لحم)',  @ShakhtouraId, N'Shakhtoura', 9.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese Shakhtoura (جبن)',  @ShakhtouraId, N'Shakhtoura', 9.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Zater Shakhtoura (زعتر)',  @ShakhtouraId, N'Shakhtoura', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Muhamar Shakhtoura (محمر)',  @ShakhtouraId, N'Shakhtoura', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Sabanek Shakhtoura (سبانخ)',  @ShakhtouraId, N'Shakhtoura', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labneh Shakhtoura (لبنة)',  @ShakhtouraId, N'Shakhtoura', 8.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Mix Shakhtoura (مكس)',  @ShakhtouraId, N'Shakhtoura', 9.50, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Meat Farshouha (لحم)',  @FarshouhaId, N'Farshouha', 17.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cheese Farshouha (جبن)',  @FarshouhaId, N'Farshouha', 17.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Zater Farshouha (زعتر)',  @FarshouhaId, N'Farshouha', 17.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Muhamar Farshouha (محمر)',  @FarshouhaId, N'Farshouha', 17.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Sabanek Farshouha (سبانخ)',  @FarshouhaId, N'Farshouha', 17.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Labneh Farshouha (لبنة)',  @FarshouhaId, N'Farshouha', 17.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Mix Farshouha (مكس)',  @FarshouhaId, N'Farshouha', 18.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Orange Juice (برتقال) - Large',  @JuicesId2, N'Juice Large', 25.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Orange Juice (برتقال) - Medium',  @JuicesId2, N'Juice Medium', 11.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Orange Juice (برتقال) - Small',  @JuicesId2, N'Juice Small', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Mango Juice (مانجو) - Large',  @JuicesId2, N'Juice Large', 25.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Mango Juice (مانجو) - Medium',  @JuicesId2, N'Juice Medium', 11.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Mango Juice (مانجو) - Small',  @JuicesId2, N'Juice Small', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Pomegranate Juice (رمان) - Large',  @JuicesId2, N'Juice Large', 25.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Pomegranate Juice (رمان) - Medium',  @JuicesId2, N'Juice Medium', 11.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Pomegranate Juice (رمان) - Small',  @JuicesId2, N'Juice Small', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Strawberry Juice (فراولة) - Large',  @JuicesId2, N'Juice Large', 25.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Strawberry Juice (فراولة) - Medium',  @JuicesId2, N'Juice Medium', 11.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Strawberry Juice (فراولة) - Small',  @JuicesId2, N'Juice Small', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Lemon Juice (ليمون) - Large',  @JuicesId2, N'Juice Large', 25.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Lemon Juice (ليمون) - Medium',  @JuicesId2, N'Juice Medium', 11.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Lemon Juice (ليمون) - Small',  @JuicesId2, N'Juice Small', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Lemon with Mint Juice (ليمون و نعناع) - Large',  @JuicesId2, N'Juice Large', 25.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Lemon with Mint Juice (ليمون و نعناع) - Medium',  @JuicesId2, N'Juice Medium', 11.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Lemon with Mint Juice (ليمون و نعناع) - Small',  @JuicesId2, N'Juice Small', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cocktail Juice (كوكتيل) - Large',  @JuicesId2, N'Juice Large', 25.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cocktail Juice (كوكتيل) - Medium',  @JuicesId2, N'Juice Medium', 11.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Cocktail Juice (كوكتيل) - Small',  @JuicesId2, N'Juice Small', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Avocado Juice (افوكادو) - Large',  @JuicesId2, N'Juice Large', 25.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Avocado Juice (افوكادو) - Medium',  @JuicesId2, N'Juice Medium', 11.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Avocado Juice (افوكادو) - Small',  @JuicesId2, N'Juice Small', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Apple Juice (تفاح) - Large',  @JuicesId2, N'Juice Large', 25.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Apple Juice (تفاح) - Medium',  @JuicesId2, N'Juice Medium', 11.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Apple Juice (تفاح) - Small',  @JuicesId2, N'Juice Small', 8.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Soft Drinks (غازيات)',  @JuicesId2, N'Soft Drinks', 4.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Melco (ملكو)',  @JuicesId2, N'Melco', 2.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1),
    (N'Water (ماء)',  @JuicesId2, N'Water', 1.00, (SELECT TaxRateId FROM TaxRates WHERE Name = 'Standard'), 1);
GO

-- ============================================================
-- Procedure: GetRecentTransactionsByCashier
-- Params: @CashierId INT, @ShiftId INT, @Limit INT
-- ============================================================

GO

CREATE OR ALTER PROCEDURE GetRecentTransactionsByCashier
    @CashierId INT,
    @ShiftId INT,
    @Limit INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Limit)
        t.TransactionId,
        t.ReceiptNumber,
        t.TransactionDate,
        t.GrandTotal,
        t.Status,
        t.Notes,
        (SELECT TOP 1 p.PaymentMethod FROM Payments p WHERE p.TransactionId = t.TransactionId) AS PaymentMethod
    FROM Transactions t
    WHERE t.CashierId = @CashierId
      AND t.ShiftId = @ShiftId
    ORDER BY t.TransactionDate DESC;
END;
GO

CREATE VIEW dbo.vw_RecentTransactions
AS
SELECT
    t.TransactionId,
    t.ReceiptNumber,
    t.GrandTotal,
    p.PaymentMethod,
    t.TransactionDate,
    u.FullName AS CashierName,
    t.Status
FROM dbo.Transactions AS t
INNER JOIN dbo.Payments AS p
    ON p.TransactionId = t.TransactionId
INNER JOIN dbo.Users AS u
    ON t.CashierId = u.UserId;
GO

CREATE VIEW vw_ShiftSummary AS
SELECT 
    s.ShiftId,
    u.FullName AS CashierName,
    s.OpenedAt AS OpenTime,
    s.OpeningCash,
    s.ClosedAt AS CloseTime,
    s.ClosingCash,
    s.ExpectedCash,
    s.CashDifference,
    s.Status
FROM dbo.Shifts s
INNER JOIN dbo.Users u 
    ON s.UserId = u.UserId;
GO

CREATE VIEW vw_Transactions AS
SELECT 
    t.TransactionId,
    t.ReceiptNumber,
    t.GrandTotal,
    t.Notes AS TransNote,
    p.PaymentMethod,
    t.Status,
    t.CreatedAt AS TransactionDate
FROM dbo.Transactions t
LEFT JOIN dbo.Payments p 
    ON p.TransactionId = t.TransactionId;
GO