USE [POS_DB]
GO
/****** Object:  Table [dbo].[TransactionItemModifiers]    Script Date: 07/16/2026 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/*==========================================================
    TransactionItemModifiers
    Captures the modifier selections made at the point of
    sale for each transaction line item.
    Records the snapshot values (name, price) at transaction
    time so price changes later don't affect historical data.
==========================================================*/

CREATE TABLE [dbo].[TransactionItemModifiers](
    [TransactionItemModifierId] [int] IDENTITY(1,1) NOT NULL,
    [TransactionItemId]         [int] NOT NULL,
    [ModifierOptionId]          [int] NULL,
    [ModifierGroupId]           [int] NOT NULL,
    [GroupName]                 [nvarchar](100) NOT NULL,
    [OptionName]                [nvarchar](100) NOT NULL,
    [Quantity]                  [int] NOT NULL,
    [PriceAdd]                  [decimal](18, 2) NOT NULL,
    [LineTotal]                 [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_TransactionItemModifiers] PRIMARY KEY CLUSTERED
(
    [TransactionItemModifierId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TransactionItemModifiers] ADD CONSTRAINT [DF_TransactionItemModifiers_Quantity] DEFAULT ((1)) FOR [Quantity]
GO
ALTER TABLE [dbo].[TransactionItemModifiers] ADD CONSTRAINT [DF_TransactionItemModifiers_PriceAdd] DEFAULT ((0)) FOR [PriceAdd]
GO
ALTER TABLE [dbo].[TransactionItemModifiers] ADD CONSTRAINT [DF_TransactionItemModifiers_LineTotal] DEFAULT ((0)) FOR [LineTotal]
GO
ALTER TABLE [dbo].[TransactionItemModifiers] WITH CHECK ADD CONSTRAINT [FK_TransactionItemModifiers_TransactionItems] FOREIGN KEY([TransactionItemId])
REFERENCES [dbo].[TransactionItems] ([TransactionItemId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TransactionItemModifiers] CHECK CONSTRAINT [FK_TransactionItemModifiers_TransactionItems]
GO
ALTER TABLE [dbo].[TransactionItemModifiers] WITH CHECK ADD CONSTRAINT [FK_TransactionItemModifiers_ModifierOptions] FOREIGN KEY([ModifierOptionId])
REFERENCES [dbo].[ModifierOptions] ([ModifierOptionId])
ON DELETE NO ACTION
GO
ALTER TABLE [dbo].[TransactionItemModifiers] CHECK CONSTRAINT [FK_TransactionItemModifiers_ModifierOptions]
GO
ALTER TABLE [dbo].[TransactionItemModifiers] WITH CHECK ADD CONSTRAINT [FK_TransactionItemModifiers_ModifierGroups] FOREIGN KEY([ModifierGroupId])
REFERENCES [dbo].[ModifierGroups] ([ModifierGroupId])
ON DELETE NO ACTION
GO
ALTER TABLE [dbo].[TransactionItemModifiers] CHECK CONSTRAINT [FK_TransactionItemModifiers_ModifierGroups]
GO
ALTER TABLE [dbo].[TransactionItemModifiers] WITH CHECK ADD CONSTRAINT [CK_TransactionItemModifiers_Quantity] CHECK (([Quantity]>(0)))
GO
ALTER TABLE [dbo].[TransactionItemModifiers] CHECK CONSTRAINT [CK_TransactionItemModifiers_Quantity]
GO
ALTER TABLE [dbo].[TransactionItemModifiers] WITH CHECK ADD CONSTRAINT [CK_TransactionItemModifiers_PriceAdd] CHECK (([PriceAdd]>=(0)))
GO
ALTER TABLE [dbo].[TransactionItemModifiers] CHECK CONSTRAINT [CK_TransactionItemModifiers_PriceAdd]
GO
ALTER TABLE [dbo].[TransactionItemModifiers] WITH CHECK ADD CONSTRAINT [CK_TransactionItemModifiers_LineTotal] CHECK (([LineTotal]>=(0)))
GO
ALTER TABLE [dbo].[TransactionItemModifiers] CHECK CONSTRAINT [CK_TransactionItemModifiers_LineTotal]
GO