using System;
using UnityEngine;

public static class BattleLineOfSightRules
{
	public static bool HasLineOfSight(BattleContext battleContext, Vector3Int sourceCell, Vector3Int targetCell)
	{
		if (battleContext?.Board == null || !battleContext.Board.IsInside(sourceCell) || !battleContext.Board.IsInside(targetCell))
		{
			return false;
		}

		if (sourceCell == targetCell)
		{
			return true;
		}

		// Cast ray one cell above the floor node positions (the standing/passable space).
		// Navigation nodes sit at the solid floor voxel (Y), which is an obstacle. Tracing
		// through Y would hit those floor voxels and always block LOS. Y+1 is the open
		// walkable cell where creatures actually stand.
		Vector3Int losSource = new Vector3Int(sourceCell.x, sourceCell.y + 1, sourceCell.z);
		Vector3Int losTarget = new Vector3Int(targetCell.x, targetCell.y + 1, targetCell.z);

		Vector3 start = GetCellCenter(losSource) + new Vector3(0.0f, 0.5f, 0.0f);
		Vector3 end = GetCellCenter(losTarget) + new Vector3(0.0f, 0.5f, 0.0f);
		Vector3 direction = end - start;
		float length = direction.magnitude;
		if (length <= Mathf.Epsilon)
		{
			return true;
		}

		direction /= length;

		Vector3Int currentCell = losSource;
		Vector3Int step = new Vector3Int(
			Math.Sign(direction.x),
			Math.Sign(direction.y),
			Math.Sign(direction.z));

		float tMaxX = GetInitialTMax(start.x, direction.x, currentCell.x, step.x);
		float tMaxY = GetInitialTMax(start.y, direction.y, currentCell.y, step.y);
		float tMaxZ = GetInitialTMax(start.z, direction.z, currentCell.z, step.z);

		float tDeltaX = GetTDelta(direction.x);
		float tDeltaY = GetTDelta(direction.y);
		float tDeltaZ = GetTDelta(direction.z);

		while (currentCell != losTarget)
		{
			if (tMaxX <= tMaxY && tMaxX <= tMaxZ)
			{
				currentCell.x += step.x;
				tMaxX += tDeltaX;
			}
			else if (tMaxY <= tMaxZ)
			{
				currentCell.y += step.y;
				tMaxY += tDeltaY;
			}
			else
			{
				currentCell.z += step.z;
				tMaxZ += tDeltaZ;
			}

			if (currentCell == losSource || currentCell == losTarget)
			{
				continue;
			}

			if (!battleContext.Board.IsInside(currentCell))
			{
				return false;
			}

			if (IsBlockingCell(battleContext, currentCell))
			{
				return false;
			}
		}

		return true;
	}

	private static bool IsBlockingCell(BattleContext battleContext, Vector3Int cell)
	{
		if (battleContext?.Board?.Terrain == null || !battleContext.Board.Terrain.TryGetCell(cell, out VoxelCell voxelCell) || voxelCell == null || voxelCell.IsEmpty)
		{
			return false;
		}

		if (!battleContext.Board.Terrain.VoxelRegistry.TryGetVoxel(voxelCell.Id, out VoxelDefinition voxelDefinition) ||
			voxelDefinition?.Data == null)
		{
			return false;
		}

		return voxelDefinition.Data.Traversal == VoxelTraversal.Obstacle;
	}

	private static Vector3 GetCellCenter(Vector3Int cell)
	{
		return new Vector3(cell.x + 0.5f, cell.y + 0.5f, cell.z + 0.5f);
	}

	private static float GetInitialTMax(float startAxis, float directionAxis, int cellAxis, int stepAxis)
	{
		if (stepAxis == 0 || Mathf.Approximately(directionAxis, 0f))
		{
			return float.PositiveInfinity;
		}

		float nextBoundary = stepAxis > 0 ? cellAxis + 1f : cellAxis;
		return (nextBoundary - startAxis) / directionAxis;
	}

	private static float GetTDelta(float directionAxis)
	{
		return Mathf.Approximately(directionAxis, 0f)
			? float.PositiveInfinity
			: Mathf.Abs(1f / directionAxis);
	}
}
