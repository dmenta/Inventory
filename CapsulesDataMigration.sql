DELETE FROM CapsulesItems
GO
DELETE FROM Capsules
GO
DBCC CHECKIDENT (Capsules, RESEED, 0)
GO
INSERT INTO dbo.Capsules (Code, Name, UserId, ItemId)
SELECT IdCapsula, Descripcion, IdUsuario, ItemID FROM dbo.Capsulas
GO

INSERT INTO dbo.CapsulesItems (CapsuleId, ItemId, Quantity)
SELECT Capsules.CapsuleId, CapsulasItems.ItemID, Cantidad
FROM 
dbo.CapsulasItems
INNER JOIN Capsules ON
Capsules.Code=CapsulasItems.IdCapsula
GO
