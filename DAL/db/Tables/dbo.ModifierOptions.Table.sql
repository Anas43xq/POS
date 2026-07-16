USE [POS_DB]
GO
/****** Object:  Table [dbo].[ModifierOptions]    Script Date: 07/16/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    ModifierOptions
    Individual options within a modifier group
    (e.g. "Whole Milk", "Oat Milk" within "Milk Type").
    PriceAdd can be 0 for no-cost options.
==========================================================*/

CREATE TABLE [dbo].[ModifierOptions](
    [ModifierOptionId] [int] IDENTITY(1,1) NOT NULL,
    [ModifierGroupId]  [int] NOT NULL,
    [Name]             [nvarchar](100) NOT NULL,
    [PriceAdd]         [decimal](18, 2) NOT NULL,
    [IsActive]         [bit] NOT NULL,
    [AllowQuantity]    [bit] NOT NULL,
    [IsDefault]        [bit] NOT NULL,
    [SortOrder]        [int] NOT NULL,
    [CreatedAt]        [datetime2](0) NOT NULL,
    [UpdatedAt]        [datetime2](0) NOT NULL,
 CONSTRAINT [PK_ModifierOptions] PRIMARY KEY CLUSTERED
(
    [ModifierOptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ModifierOptions] ADD CONSTRAINT [DF_ModifierOptions_PriceAdd] DEFAULT ((0)) FOR [PriceAdd]
GO
ALTER TABLE [dbo].[ModifierOptions] ADD CONSTRAINT [DF_ModifierOptions_IsActive] DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[ModifierOptions] ADD CONSTRAINT [DF_ModifierOptions_AllowQuantity] DEFAULT ((0)) FOR [AllowQuantity]
GO
ALTER TABLE [dbo].[ModifierOptions] ADD CONSTRAINT [DF_ModifierOptions_IsDefault] DEFAULT ((0)) FOR [IsDefault]
GO
ALTER TABLE [dbo].[ModifierOptions] ADD CONSTRAINT [DF_ModifierOptions_SortOrder] DEFAULT ((0)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[ModifierOptions] ADD CONSTRAINT [DF_ModifierOptions_CreatedAt] DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[ModifierOptions] ADD CONSTRAINT [DF_ModifierOptions_UpdatedAt] DEFAULT (sysutcdatetime()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[ModifierOptions] WITH CHECK ADD CONSTRAINT [FK_ModifierOptions_ModifierGroups] FOREIGN KEY([ModifierGroupId])
REFERENCES [dbo].[ModifierGroups] ([ModifierGroupId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ModifierOptions] CHECK CONSTRAINT [FK_ModifierOptions_ModifierGroups]
GO
ALTER TABLE [dbo].[ModifierOptions] WITH CHECK ADD CONSTRAINT [CK_ModifierOptions_IsActive] CHECK (([IsActive]=(1) OR [IsActive]=(0)))
GO
ALTER TABLE [dbo].[ModifierOptions] CHECK CONSTRAINT [CK_ModifierOptions_IsActive]
GO
ALTER TABLE [dbo].[ModifierOptions] WITH CHECK ADD CONSTRAINT [CK_ModifierOptions_PriceAdd] CHECK (([PriceAdd]>=(0)))
GO
ALTER TABLE [dbo].[ModifierOptions] CHECK CONSTRAINT [CK_ModifierOptions_PriceAdd]
GO