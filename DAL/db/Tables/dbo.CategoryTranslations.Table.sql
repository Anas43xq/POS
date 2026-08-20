USE [POS_DB]
GO
/****** Object:  Table [dbo].[CategoryTranslations]    Script Date: 07/10/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    CategoryTranslations
==========================================================*/

CREATE TABLE dbo.CategoryTranslations
(
    CategoryTranslationId INT IDENTITY(1,1) NOT NULL,
    CategoryId            INT NOT NULL,
    LanguageCode          NVARCHAR(10) NOT NULL,
    Name                  NVARCHAR(100) NOT NULL,
    CreatedAt             DATETIME2(0) NOT NULL
        CONSTRAINT DF_CategoryTranslations_CreatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_CategoryTranslations
        PRIMARY KEY CLUSTERED (CategoryTranslationId)
);
GO

/*==========================================================
    Foreign Keys
==========================================================*/

ALTER TABLE dbo.CategoryTranslations
ADD CONSTRAINT FK_CategoryTranslations_Categories
FOREIGN KEY (CategoryId)
REFERENCES dbo.Categories(CategoryId)
ON DELETE CASCADE;
GO

/*==========================================================
    Unique Constraints
==========================================================*/

ALTER TABLE dbo.CategoryTranslations
ADD CONSTRAINT UQ_CategoryTranslations_Category_Language
UNIQUE (CategoryId, LanguageCode);
GO
