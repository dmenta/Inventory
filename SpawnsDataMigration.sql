DELETE FROM SpawnsCapsulesItems
GO
DELETE FROM SpawnsCapsules
GO
DELETE FROM Spawns
GO
DBCC CHECKIDENT (Spawns, RESEED, 0)
GO
INSERT INTO Spawns
SELECT DISTINCT IdUsuario UserId, Fecha [Date] from Reproducciones
ORDER BY Fecha, IdUsuario
GO
INSERT INTO SpawnsCapsules
SELECT DISTINCT
S.SpawnId,
R.IdCapsula CapsuleCode
FROM Spawns S
INNER JOIN Reproducciones R ON
 R.Fecha=S.[Date]
 AND R.IdUsuario=S.UserId
GO
INSERT INTO SpawnsCapsulesItems
SELECT 
S.SpawnId,
R.IdCapsula CapsuleCode,
ItemId,
SUM(Cantidad) Quantity
FROM Spawns S
INNER JOIN Reproducciones R ON
 R.Fecha=S.Date
 AND R.IdUsuario=S.UserID
INNER JOIN ReproduccionesItems RI ON
 R.IdReproduccion=RI.IdReproduccion
GROUP BY
	S.SpawnID,
	R.IdCapsula,
	ItemID
GO
