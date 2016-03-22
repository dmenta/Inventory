/****** Object:  Table [dbo].[Inventories]    Script Date: 21/03/2016 07:09:18 p.m. ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Inventories](
	[InventoryId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Inventories] PRIMARY KEY CLUSTERED 
(
	[InventoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[InventoriesItems](
	[InventoryId] [int] NOT NULL,
	[ItemId] [varchar](10) NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_InventoriesItems] PRIMARY KEY CLUSTERED 
(
	[InventoryId] ASC, [ItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[InventoriesItems]  WITH CHECK ADD CONSTRAINT [FK_InventoriesItems_Inventories] FOREIGN KEY([InventoryId])
REFERENCES [dbo].[Inventories] ([InventoryId])
GO

ALTER TABLE [dbo].[InventoriesItems] CHECK CONSTRAINT [FK_InventoriesItems_Inventories]
GO

INSERT INTO Inventories
SELECT DISTINCT IdUsuario FROM Inventarios
GO

INSERT INTO InventoriesItems
SELECT Inventories.InventoryId, ItemID, Cantidad  FROM
Inventarios
INNER JOIN Inventories ON
Inventarios.IdUsuario=Inventories.UserId

SET ANSI_PADDING OFF
GO


