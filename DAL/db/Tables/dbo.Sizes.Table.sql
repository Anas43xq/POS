USE [POS_DB]
GO
/****** Object:  Table [dbo].[Sizes]    Script Date: 07/10/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    Sizes
==========================================================*/

CREATE TABLE dbo.Sizes
(
    SizeId        INT IDENTITY(1,1) NOT NULL,
    Name          NVARCHAR(50) NOT NULL,
    DisplayOrder  INT NOT NULL,
    IsActive      BIT NOT NULL
        CONSTRAINT DF_Sizes_IsActive
        DEFAULT (1),
    CreatedAt     DATETIME2(0) NOT NULL
        CONSTRAINT DF_Sizes_CreatedAt
        DEFAULT (SYSUTCDATETIME()),
    UpdatedAt     DATETIME2(0) NOT NULL
        CONSTRAINT DF_Sizes_UpdatedAt
        DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Sizes
        PRIMARY KEY CLUSTERED (SizeId)
);
GO
