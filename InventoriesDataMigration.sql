DELETE FROM InventoriesItems
GO
DELETE FROM Inventories
GO
DBCC CHECKIDENT (Inventories, RESEED, 0)
GO
INSERT INTO Inventories
SELECT DISTINCT IdUsuario FROM Inventarios
GO

INSERT INTO InventoriesItems
SELECT Inventories.InventoryId, ItemID, Cantidad  FROM
Inventarios
INNER JOIN Inventories ON
Inventarios.IdUsuario=Inventories.UserId
GO


