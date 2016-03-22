SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Spawns](
	[SpawnId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [varchar](100) NOT NULL,
	[Date] [datetime] NOT NULL,
 CONSTRAINT [PK_Spawns] PRIMARY KEY CLUSTERED 
(
	[SpawnId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[SpawnsCapsules](
	[SpawnId] [int] NOT NULL,
	[CapsuleCode] [varchar](8) NOT NULL,
 CONSTRAINT [PK_SpawnsCapsules] PRIMARY KEY CLUSTERED 
(
	[SpawnId] ASC,
	[CapsuleCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[SpawnsCapsules]  WITH CHECK ADD  CONSTRAINT [FK_SpawnsCapsules_Spawns] FOREIGN KEY([SpawnId])
REFERENCES [dbo].[Spawns] ([SpawnId])
GO

ALTER TABLE [dbo].[SpawnsCapsules] CHECK CONSTRAINT [FK_SpawnsCapsules_Spawns]
GO


CREATE TABLE [dbo].[SpawnsCapsulesItems](
	[SpawnId] [int] NOT NULL,
	[CapsuleCode] [varchar](8) NOT NULL,
	[ItemId] [varchar](10) NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_SpawnsCapsulesItems] PRIMARY KEY CLUSTERED 
(
	[SpawnId] ASC,
	[CapsuleCode] ASC,
	[ItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[SpawnsCapsulesItems]  WITH CHECK ADD  CONSTRAINT [FK_SpawnsCapsulesItems_SpawnsCapsules] FOREIGN KEY([SpawnId], [CapsuleCode])
REFERENCES [dbo].[SpawnsCapsules] ([SpawnId], [CapsuleCode])
GO

ALTER TABLE [dbo].[SpawnsCapsulesItems] CHECK CONSTRAINT [FK_SpawnsCapsulesItems_SpawnsCapsules]
GO


SET ANSI_PADDING OFF
GO


