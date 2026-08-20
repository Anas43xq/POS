USE [POS_DB]
GO
/****** Object:  Table [dbo].[CategoryModifierGroups]    Script Date: 07/16/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    CategoryModifierGroups
    Links modifier groups to categories.
    All products in the category inherit these modifiers.
==========================================================*/

CREATE TABLE [dbo].[CategoryModifierGroups](
    [CategoryId]      [int] NOT NULL,
    [ModifierGroupId] [int] NOT NULL,
 CONSTRAINT [PK_CategoryModifierGroups] PRIMARY KEY CLUSTERED
(
    [CategoryId] ASC,
    [ModifierGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CategoryModifierGroups] WITH CHECK ADD CONSTRAINT [FK_CategoryModifierGroups_Categories] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([CategoryId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CategoryModifierGroups] CHECK CONSTRAINT [FK_CategoryModifierGroups_Categories]
GO
ALTER TABLE [dbo].[CategoryModifierGroups] WITH CHECK ADD CONSTRAINT [FK_CategoryModifierGroups_ModifierGroups] FOREIGN KEY([ModifierGroupId])
REFERENCES [dbo].[ModifierGroups] ([ModifierGroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CategoryModifierGroups] CHECK CONSTRAINT [FK_CategoryModifierGroups_ModifierGroups]
GO