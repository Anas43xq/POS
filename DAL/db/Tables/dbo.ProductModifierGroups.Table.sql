USE [POS_DB]
GO
/****** Object:  Table [dbo].[ProductModifierGroups]    Script Date: 07/16/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    ProductModifierGroups
    Links modifier groups directly to specific products.
    Overrides/extends category-level modifier assignments.
==========================================================*/

CREATE TABLE [dbo].[ProductModifierGroups](
    [ProductId]       [int] NOT NULL,
    [ModifierGroupId] [int] NOT NULL,
 CONSTRAINT [PK_ProductModifierGroups] PRIMARY KEY CLUSTERED
(
    [ProductId] ASC,
    [ModifierGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProductModifierGroups] WITH CHECK ADD CONSTRAINT [FK_ProductModifierGroups_Products] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([ProductId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProductModifierGroups] CHECK CONSTRAINT [FK_ProductModifierGroups_Products]
GO
ALTER TABLE [dbo].[ProductModifierGroups] WITH CHECK ADD CONSTRAINT [FK_ProductModifierGroups_ModifierGroups] FOREIGN KEY([ModifierGroupId])
REFERENCES [dbo].[ModifierGroups] ([ModifierGroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ProductModifierGroups] CHECK CONSTRAINT [FK_ProductModifierGroups_ModifierGroups]
GO