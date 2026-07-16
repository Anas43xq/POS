USE [POS_DB]
GO
/****** Object:  Table [dbo].[ModifierOptionTranslations]    Script Date: 07/16/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    ModifierOptionTranslations
    Stores localized names for modifier options.
==========================================================*/

CREATE TABLE [dbo].[ModifierOptionTranslations](
    [ModifierOptionTranslationId] [int] IDENTITY(1,1) NOT NULL,
    [ModifierOptionId]            [int] NOT NULL,
    [LanguageCode]                [nvarchar](10) NOT NULL,
    [Name]                        [nvarchar](100) NOT NULL,
    [CreatedAt]                   [datetime2](0) NOT NULL,
 CONSTRAINT [PK_ModifierOptionTranslations] PRIMARY KEY CLUSTERED
(
    [ModifierOptionTranslationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ModifierOptionTranslations] ADD CONSTRAINT [DF_ModifierOptionTranslations_CreatedAt] DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ModifierOptionTranslations] WITH CHECK ADD CONSTRAINT [FK_ModifierOptionTranslations_ModifierOptions] FOREIGN KEY([ModifierOptionId])
REFERENCES [dbo].[ModifierOptions] ([ModifierOptionId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ModifierOptionTranslations] CHECK CONSTRAINT [FK_ModifierOptionTranslations_ModifierOptions]
GO
ALTER TABLE [dbo].[ModifierOptionTranslations] ADD CONSTRAINT [UQ_ModifierOptionTranslations_Option_Language] UNIQUE ([ModifierOptionId], [LanguageCode])
GO