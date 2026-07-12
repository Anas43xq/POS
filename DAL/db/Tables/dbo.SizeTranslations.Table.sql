USE [POS_DB]
GO
/****** Object:  Table [dbo].[SizeTranslations]    Script Date: 07/10/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    SizeTranslations
==========================================================*/

CREATE TABLE dbo.SizeTranslations
(
    SizeTranslationId INT IDENTITY(1,1) NOT NULL,
    SizeId            INT NOT NULL,
    LanguageCode      NVARCHAR(10) NOT NULL,
    Name              NVARCHAR(50) NOT NULL,
    CreatedAt         DATETIME2(0) NOT NULL
        CONSTRAINT DF_SizeTranslations_CreatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_SizeTranslations
        PRIMARY KEY CLUSTERED (SizeTranslationId)
);
GO

/*==========================================================
    Foreign Keys
==========================================================*/

ALTER TABLE dbo.SizeTranslations
ADD CONSTRAINT FK_SizeTranslations_Sizes
FOREIGN KEY (SizeId)
REFERENCES dbo.Sizes(SizeId)
ON DELETE CASCADE;
GO

/*==========================================================
    Unique Constraints
==========================================================*/

ALTER TABLE dbo.SizeTranslations
ADD CONSTRAINT UQ_SizeTranslations_Size_Language
UNIQUE (SizeId, LanguageCode);
GO
