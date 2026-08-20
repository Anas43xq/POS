USE [POS_DB]
GO
/****** Object:  Table [dbo].[ModifierGroupTranslations]    Script Date: 07/16/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    ModifierGroupTranslations
    Stores localized names for modifier groups.
==========================================================*/

CREATE TABLE [dbo].[ModifierGroupTranslations](
    [ModifierGroupTranslationId] [int] IDENTITY(1,1) NOT NULL,
    [ModifierGroupId]            [int] NOT NULL,
    [LanguageCode]               [nvarchar](10) NOT NULL,
    [Name]                       [nvarchar](100) NOT NULL,
    [CreatedAt]                  [datetime2](0) NOT NULL,
 CONSTRAINT [PK_ModifierGroupTranslations] PRIMARY KEY CLUSTERED
(
    [ModifierGroupTranslationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ModifierGroupTranslations] ADD CONSTRAINT [DF_ModifierGroupTranslations_CreatedAt] DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ModifierGroupTranslations] WITH CHECK ADD CONSTRAINT [FK_ModifierGroupTranslations_ModifierGroups] FOREIGN KEY([ModifierGroupId])
REFERENCES [dbo].[ModifierGroups] ([ModifierGroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ModifierGroupTranslations] CHECK CONSTRAINT [FK_ModifierGroupTranslations_ModifierGroups]
GO
ALTER TABLE [dbo].[ModifierGroupTranslations] ADD CONSTRAINT [UQ_ModifierGroupTranslations_Group_Language] UNIQUE ([ModifierGroupId], [LanguageCode])
GO