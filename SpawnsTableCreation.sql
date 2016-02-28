CREATE TABLE [dbo].[Spawns] (
    [SpawnID] INT           IDENTITY (1, 1) NOT NULL,
    [UserID]      VARCHAR (100) NOT NULL,
    [Creation]          DATETIME      NOT NULL,
    CONSTRAINT [PK_Spawns] PRIMARY KEY CLUSTERED ([SpawnID] ASC)
);

CREATE TABLE [dbo].[SpawnsCapsules] (
    [SpawnID] INT         NOT NULL,
    [CapsuleID]      VARCHAR (8) NOT NULL,
    [ItemID]           VARCHAR (10) NOT NULL,
    CONSTRAINT [PK_SpawnsCapsules] PRIMARY KEY CLUSTERED ([SpawnID] ASC, [CapsuleID] ASC),
    CONSTRAINT [FK_SpawnsCapsules_Spawns] FOREIGN KEY ([SpawnID]) REFERENCES [dbo].[Spawns] ([SpawnID])
);

CREATE TABLE [dbo].[SpawnsCapsulesItems] (
    [SpawnID] INT         NOT NULL,
    [CapsuleID]      VARCHAR (8) NOT NULL,
    [ItemID]           VARCHAR (10) NOT NULL,
    [InitialQty] INT          NOT NULL,
    [Interests]        INT          NOT NULL,
    CONSTRAINT [PK_SpawnsCapsulesItems] PRIMARY KEY CLUSTERED ([SpawnID] ASC, [CapsuleID] ASC, [ItemID] ASC),
    CONSTRAINT [FK_SpawnsCapsulesItems_SpawnsCapsules] FOREIGN KEY ([SpawnID], [CapsuleID]) REFERENCES [dbo].[SpawnsCapsules] ([SpawnID], [CapsuleID])
);

