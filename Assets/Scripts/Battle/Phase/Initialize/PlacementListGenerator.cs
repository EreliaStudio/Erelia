using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Phase.Initialize
{
	public static class PlacementListGenerator
	{
		public static bool TryGenerate(
			Erelia.Battle.Board.Model board,
			Erelia.Battle.Phase.Initialize.PlacementMode mode,
			out List<Vector3Int> playerCoordinates,
			out List<Vector3Int> enemyCoordinates)
		{
			playerCoordinates = new List<Vector3Int>();
			enemyCoordinates = new List<Vector3Int>();

			if (board == null)
			{
				return false;
			}

			switch (mode)
			{
				case Erelia.Battle.Phase.Initialize.PlacementMode.HalfBoard:
					GenerateHalfBoardLists(board, playerCoordinates, enemyCoordinates);
					break;
				default:
					return false;
			}

			return playerCoordinates.Count > 0 && enemyCoordinates.Count > 0;
		}

		private static void GenerateHalfBoardLists(
			Erelia.Battle.Board.Model board,
			List<Vector3Int> playerCoordinates,
			List<Vector3Int> enemyCoordinates)
		{
			int splitZ = board.SizeZ / 2;

			for (int x = 0; x < board.SizeX; x++)
			{
				for (int y = 0; y < board.SizeY; y++)
				{
					for (int z = 0; z < board.SizeZ; z++)
					{
						if (!IsAcceptableCoordinate(board, x, y, z))
						{
							continue;
						}

						Vector3Int coordinate = new Vector3Int(x, y, z);
						if (z < splitZ)
						{
							playerCoordinates.Add(coordinate);
						}
						else
						{
							enemyCoordinates.Add(coordinate);
						}
					}
				}
			}
		}

		private static bool IsAcceptableCoordinate(Erelia.Battle.Board.Model board, int x, int y, int z)
		{
			Erelia.Battle.Voxel.Cell cell = board.Cells[x, y, z];
			if (!Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition))
			{
				return false;
			}

			return IsAcceptableAsFloor(definition) &&
				HasAirOrWalkableBlockOnTop(board, x, y, z) &&
				HasAvailableSpace(board, x, y, z);
		}

		private static bool IsAcceptableAsFloor(Erelia.Core.VoxelKit.Definition definition)
		{
			return definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Obstacle;
		}

		private static bool HasAirOrWalkableBlockOnTop(Erelia.Battle.Board.Model board, int x, int y, int z)
		{
			int targetY = y + 1;
			if (targetY >= board.SizeY)
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cell = board.Cells[x, targetY, z];
			if (!Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition))
			{
				return true;
			}

			return definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Walkable;
		}

		private static bool HasAvailableSpace(Erelia.Battle.Board.Model board, int x, int y, int z)
		{
			const int UnitHeight = 2;
			for (int deltaY = 1; deltaY < UnitHeight; deltaY++)
			{
				int targetY = y + deltaY;
				if (targetY >= board.SizeY)
				{
					return false;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[x, targetY, z];
				if (Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition) &&
					definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Obstacle)
				{
					return false;
				}
			}

			return true;
		}
	}
}
