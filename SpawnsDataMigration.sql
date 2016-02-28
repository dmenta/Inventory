DELETE FROM SpawnsCapsulesItems
GO
DELETE FROM SpawnsCapsules
GO
DELETE FROM Spawns
GO
DBCC CHECKIDENT (Spawns, RESEED, 0)
GO
INSERT INTO Spawns
SELECT DISTINCT IdUsuario UserID, Fecha Creation from Reproducciones
ORDER BY Fecha, IdUsuario
GO
INSERT INTO SpawnsCapsules
SELECT DISTINCT
S.SpawnID,
R.IdCapsula CapsuleID,
'CAPS_MUFG' ItemID
FROM Spawns S
INNER JOIN Reproducciones R ON
 R.Fecha=S.Creation
 AND R.IdUsuario=S.UserID
GO
INSERT INTO SpawnsCapsulesItems
SELECT 
S.SpawnID,
R.IdCapsula CapsuleID,
ItemID,
0 TotalQty,
SUM(Cantidad) Interests
FROM Spawns S
INNER JOIN Reproducciones R ON
 R.Fecha=S.Creation
 AND R.IdUsuario=S.UserID
INNER JOIN ReproduccionesItems RI ON
 R.IdReproduccion=RI.IdReproduccion
GROUP BY
	S.SpawnID,
	R.IdCapsula,
	ItemID
GO
