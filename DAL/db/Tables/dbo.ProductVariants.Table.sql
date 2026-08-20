USE [POS_DB]
GO
/****** Object:  Table [dbo].[ProductVariants]    Script Date: 07/10/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    ProductVariants
==========================================================*/

CREATE TABLE dbo.ProductVariants
(
    VariantId     INT IDENTITY(1,1) NOT NULL,
    ProductId     INT NOT NULL,
    SizeId        INT NOT NULL,
    UnitPrice     DECIMAL(18,2) NOT NULL,
    IsActive      BIT NOT NULL
        CONSTRAINT DF_ProductVariants_IsActive
        DEFAULT (1),
    CreatedAt     DATETIME2(0) NOT NULL
        CONSTRAINT DF_ProductVariants_CreatedAt
        DEFAULT (SYSUTCDATETIME()),
    UpdatedAt     DATETIME2(0) NOT NULL
        CONSTRAINT DF_ProductVariants_UpdatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_ProductVariants
        PRIMARY KEY CLUSTERED (VariantId)
);
GO

/*==========================================================
    Foreign Keys
==========================================================*/

ALTER TABLE dbo.ProductVariants
ADD CONSTRAINT FK_ProductVariants_Products
FOREIGN KEY (ProductId)
REFERENCES dbo.Products(ProductId)
ON DELETE NO ACTION;
GO

ALTER TABLE dbo.ProductVariants
ADD CONSTRAINT FK_ProductVariants_Sizes
FOREIGN KEY (SizeId)
REFERENCES dbo.Sizes(SizeId)
ON DELETE NO ACTION;
GO
