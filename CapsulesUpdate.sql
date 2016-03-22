USE [Inventory]
GO

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT

BEGIN TRANSACTION
GO

ALTER TABLE dbo.Capsulas
	DROP CONSTRAINT DF_Capsulas_Descripcion
GO

CREATE TABLE dbo.Tmp_Capsulas
	(
	CapsuleId int NOT NULL IDENTITY (1, 1),
	Code varchar(8) NOT NULL,
	Name varchar(40) NULL,
	UserId varchar(100) NOT NULL,
	ItemId varchar(10) NOT NULL
	)  ON [PRIMARY]
GO

ALTER TABLE dbo.Tmp_Capsulas ADD CONSTRAINT
	DF_Capsules_Name DEFAULT ('') FOR Name
GO
SET IDENTITY_INSERT dbo.Tmp_Capsulas OFF
GO
IF EXISTS(SELECT * FROM dbo.Capsulas)
	 EXEC('INSERT INTO dbo.Tmp_Capsulas (Code, Name, UserId, ItemId)
		SELECT IdCapsula, Descripcion, IdUsuario, ItemID FROM dbo.Capsulas WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE dbo.CapsulasItems
	DROP CONSTRAINT FK_CapsulasItems_Capsulas
GO
DROP TABLE dbo.Capsulas
GO
EXECUTE sp_rename N'dbo.Tmp_Capsulas', N'Capsules', 'OBJECT' 
GO
ALTER TABLE dbo.Capsules ADD CONSTRAINT
	PK_Capsules PRIMARY KEY CLUSTERED 
	(
	CapsuleId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE TABLE dbo.Tmp_CapsulasItems
	(
	CapsuleId int NOT NULL,
	ItemId varchar(10) NOT NULL,
	Quantity int NOT NULL,
	IdCapsula varchar(8) NOT NULL
	)  ON [PRIMARY]
GO

INSERT INTO dbo.Tmp_CapsulasItems (CapsuleId, ItemId, Quantity, IdCapsula)
SELECT Capsules.CapsuleId, CapsulasItems.ItemID, Cantidad, IdCapsula
FROM 
dbo.CapsulasItems WITH (HOLDLOCK TABLOCKX)
INNER JOIN Capsules ON
Capsules.Code=CapsulasItems.IdCapsula
GO

ALTER TABLE Tmp_CapsulasItems
	DROP COLUMN IdCapsula
GO

DROP TABLE dbo.CapsulasItems
GO
EXECUTE sp_rename N'dbo.Tmp_CapsulasItems', N'CapsulesItems', 'OBJECT' 
GO
ALTER TABLE dbo.CapsulesItems ADD CONSTRAINT
	PK_CapsulesItems PRIMARY KEY CLUSTERED 
	(
	CapsuleId,
	ItemId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.CapsulesItems ADD CONSTRAINT
	FK_CapsulasItems_Capsulas FOREIGN KEY
	(
	CapsuleId
	) REFERENCES dbo.Capsules
	(
	CapsuleId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

SELECT * FROM Capsules
GO
SELECT * FROM CapsulesItems
GO

ROLLBACK
--COMMIT

