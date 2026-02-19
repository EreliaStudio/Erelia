using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Battle.Agent.Model
{
	public static class PlacementAreaBuilder
	{
		private const int RequiredVerticalSpace = 2;

		public static PlacementAreas BuildForSide(Battle.Board.Model.Data data, Battle.Context.Model.Side side)
		{
			if (data == null)
			{
				return new PlacementAreas(null, null, null);
			}

			GetSideRange(data.SizeZ, side, out int startZ, out int endZ);
			if (startZ > endZ)
			{
				return new PlacementAreas(null, null, null);
			}

			SplitRange(startZ, endZ, out int firstStart, out int firstEnd, out int secondStart, out int secondEnd, out int thirdStart, out int thirdEnd);

			var front = new List<Vector2Int>();
			var middle = new List<Vector2Int>();
			var back = new List<Vector2Int>();

			for (int x = 0; x < data.SizeX; x++)
			{
				for (int z = startZ; z <= endZ; z++)
				{
					int topY = FindTopmostSolidY(data, x, z);
					if (topY < 0)
					{
						continue;
					}

					var pos = new Vector2Int(x, z);
					if (IsInRange(z, firstStart, firstEnd))
					{
						AddToLine(side, 0, pos, front, middle, back);
					}
					else if (IsInRange(z, secondStart, secondEnd))
					{
						AddToLine(side, 1, pos, front, middle, back);
					}
					else if (IsInRange(z, thirdStart, thirdEnd))
					{
						AddToLine(side, 2, pos, front, middle, back);
					}
				}
			}

			return new PlacementAreas(front, middle, back);
		}

		private static void GetSideRange(int sizeZ, Battle.Context.Model.Side side, out int startZ, out int endZ)
		{
			int halfZ = sizeZ / 2;
			if (side == Battle.Context.Model.Side.Player)
			{
				startZ = 0;
				endZ = halfZ - 1;
			}
			else
			{
				startZ = halfZ;
				endZ = sizeZ - 1;
			}
		}

		private static void SplitRange(int startZ, int endZ, out int firstStart, out int firstEnd, out int secondStart, out int secondEnd, out int thirdStart, out int thirdEnd)
		{
			int length = endZ - startZ + 1;
			if (length <= 0)
			{
				firstStart = firstEnd = secondStart = secondEnd = thirdStart = thirdEnd = 0;
				return;
			}

			int oneThird = length / 3;
			int twoThird = (length * 2) / 3;

			firstStart = startZ;
			firstEnd = startZ + Mathf.Max(0, oneThird - 1);

			secondStart = firstEnd + 1;
			secondEnd = startZ + Mathf.Max(0, twoThird - 1);

			thirdStart = secondEnd + 1;
			thirdEnd = endZ;
		}

		private static bool IsInRange(int z, int start, int end)
		{
			return z >= start && z <= end;
		}

		private static void AddToLine(Battle.Context.Model.Side side, int segmentIndex, Vector2Int pos, List<Vector2Int> front, List<Vector2Int> middle, List<Vector2Int> back)
		{
			if (side == Battle.Context.Model.Side.Player)
			{
				if (segmentIndex == 0)
				{
					back.Add(pos);
				}
				else if (segmentIndex == 1)
				{
					middle.Add(pos);
				}
				else
				{
					front.Add(pos);
				}
			}
			else
			{
				if (segmentIndex == 0)
				{
					front.Add(pos);
				}
				else if (segmentIndex == 1)
				{
					middle.Add(pos);
				}
				else
				{
					back.Add(pos);
				}
			}
		}

		private static int FindTopmostSolidY(Battle.Board.Model.Data data, int x, int z)
		{
			for (int y = data.SizeY - 1; y >= 0; y--)
			{
				Core.Voxel.Model.Cell cell = data.Cells[x, y, z];
				if (!IsSolidCell(cell))
				{
					continue;
				}

				if (!HasVerticalSpace(data, x, y, z, RequiredVerticalSpace))
				{
					continue;
				}

				return y;
			}

			return -1;
		}

		private static bool HasVerticalSpace(Battle.Board.Model.Data data, int x, int y, int z, int required)
		{
			for (int offset = 1; offset <= required; offset++)
			{
				int checkY = y + offset;
				if (checkY >= data.SizeY)
				{
					return false;
				}

				Core.Voxel.Model.Cell cell = data.Cells[x, checkY, z];
				if (!IsAirOrWalkableCell(cell))
				{
					return false;
				}
			}

			return true;
		}

		private static bool IsSolidCell(Core.Voxel.Model.Cell cell)
		{
			if (cell == null || cell.Id == Core.Voxel.Service.AirID)
			{
				return false;
			}

			if (!ServiceLocator.Instance.VoxelService.TryGetDefinition(cell.Id, out Core.Voxel.Model.Definition voxelDefinition))
			{
				return false;
			}

			return voxelDefinition.Data.Traversal == Core.Voxel.Model.Traversal.Obstacle;
		}

		private static bool IsAirOrWalkableCell(Core.Voxel.Model.Cell cell)
		{
			if (cell == null || cell.Id == Core.Voxel.Service.AirID)
			{
				return true;
			}

			if (!ServiceLocator.Instance.VoxelService.TryGetDefinition(cell.Id, out Core.Voxel.Model.Definition voxelDefinition))
			{
				return false;
			}

			Core.Voxel.Model.Traversal traversal = voxelDefinition.Data.Traversal;
			return traversal == Core.Voxel.Model.Traversal.Air || traversal == Core.Voxel.Model.Traversal.Walkable;
		}
	}
}
