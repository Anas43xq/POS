USE [POS_DB]
GO
/****** Object:  Table [dbo].[ProductTranslations]    Script Date: 07/10/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    ProductTranslations
==========================================================*/

CREATE TABLE dbo.ProductTranslations
(
    ProductTranslationId INT IDENTITY(1,1) NOT NULL,
    ProductId            INT NOT NULL,
    LanguageCode         NVARCHAR(10) NOT NULL,
    Name                 NVARCHAR(150) NOT NULL,
    Description          NVARCHAR(255) NULL,
    CreatedAt            DATETIME2(0) NOT NULL
        CONSTRAINT DF_ProductTranslations_CreatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_ProductTranslations
        PRIMARY KEY CLUSTERED (ProductTranslationId)
);
GO

/*==========================================================
    Foreign Keys
==========================================================*/

ALTER TABLE dbo.ProductTranslations
ADD CONSTRAINT FK_ProductTranslations_Products
FOREIGN KEY (ProductId)
REFERENCES dbo.Products(ProductId)
ON DELETE CASCADE;
GO

/*==========================================================
    Unique Constraints
==========================================================*/

ALTER TABLE dbo.ProductTranslations
ADD CONSTRAINT UQ_ProductTranslations_Product_Language
UNIQUE (ProductId, LanguageCode);
GO
