USE [POS_DB]
GO
/****** Object:  Table [dbo].[ModifierGroups]    Script Date: 07/16/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    ModifierGroups
    Defines groups of modifiers that can be applied to
    products (e.g. "Milk Type", "Extra Toppings", "Quantity").
    GroupType: 1 = SingleSelect, 2 = MultiSelect, 3 = Quantity
==========================================================*/

CREATE TABLE [dbo].[ModifierGroups](
    [ModifierGroupId] [int] IDENTITY(1,1) NOT NULL,
    [Name]            [nvarchar](100) NOT NULL,
    [GroupType]       [tinyint] NOT NULL,
    [IsRequired]      [bit] NOT NULL,
    [MinSelections]   [int] NOT NULL,
    [MaxSelections]   [int] NOT NULL,
    [SortOrder]       [int] NOT NULL,
    [CreatedAt]       [datetime2](0) NOT NULL,
    [UpdatedAt]       [datetime2](0) NOT NULL,
 CONSTRAINT [PK_ModifierGroups] PRIMARY KEY CLUSTERED
(
    [ModifierGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ModifierGroups] ADD CONSTRAINT [DF_ModifierGroups_IsRequired] DEFAULT ((0)) FOR [IsRequired]
GO
ALTER TABLE [dbo].[ModifierGroups] ADD CONSTRAINT [DF_ModifierGroups_MinSelections] DEFAULT ((0)) FOR [MinSelections]
GO
ALTER TABLE [dbo].[ModifierGroups] ADD CONSTRAINT [DF_ModifierGroups_MaxSelections] DEFAULT ((1)) FOR [MaxSelections]
GO
ALTER TABLE [dbo].[ModifierGroups] ADD CONSTRAINT [DF_ModifierGroups_SortOrder] DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[ModifierGroups] ADD CONSTRAINT [DF_ModifierGroups_CreatedAt] DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ModifierGroups] ADD CONSTRAINT [DF_ModifierGroups_UpdatedAt] DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[ModifierGroups] WITH CHECK ADD CONSTRAINT [CK_ModifierGroups_GroupType] CHECK (([GroupType] IN (1,2,3)))
GO
ALTER TABLE [dbo].[ModifierGroups] CHECK CONSTRAINT [CK_ModifierGroups_GroupType]
GO
ALTER TABLE [dbo].[ModifierGroups] WITH CHECK ADD CONSTRAINT [CK_ModifierGroups_MinSelections] CHECK (([MinSelections]>=(0)))
GO
ALTER TABLE [dbo].[ModifierGroups] CHECK CONSTRAINT [CK_ModifierGroups_MinSelections]
GO
ALTER TABLE [dbo].[ModifierGroups] WITH CHECK ADD CONSTRAINT [CK_ModifierGroups_MaxSelections] CHECK (([MaxSelections]>=(0)))
GO
ALTER TABLE [dbo].[ModifierGroups] CHECK CONSTRAINT [CK_ModifierGroups_MaxSelections]
GO
