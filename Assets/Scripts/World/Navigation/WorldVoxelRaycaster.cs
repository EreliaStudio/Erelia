using System;
using UnityEngine;

public static class WorldVoxelRaycaster
{
	public readonly struct Hit
	{
		public readonly Vector3Int WorldPosition;
		public readonly VoxelCell Cell;
		public readonly float Distance;
		public readonly Vector3 Point;

		public Hit(Vector3Int p_worldPosition, VoxelCell p_cell, float p_distance, Vector3 p_point)
		{
			WorldPosition = p_worldPosition;
			Cell = p_cell;
			Distance = p_distance;
			Point = p_point;
		}
	}

	public static bool TryRaycast(WorldData p_worldData, Ray p_ray, float p_maxDistance, float p_stepDistance, out Hit p_hit)
	{
		p_hit = default;

		if (p_worldData == null || p_stepDistance <= 0f || p_maxDistance <= 0f)
		{
			return false;
		}

		Vector3Int? previousCell = null;
		float clampedMaxDistance = Mathf.Max(p_stepDistance, p_maxDistance);

		for (float distance = 0f; distance <= clampedMaxDistance; distance += p_stepDistance)
		{
			Vector3 point = p_ray.origin + (p_ray.direction * distance);
			Vector3Int worldPosition = Vector3Int.FloorToInt(point);

			if (previousCell.HasValue && previousCell.Value == worldPosition)
			{
				continue;
			}

			previousCell = worldPosition;

			if (!p_worldData.TryGetCell(worldPosition, out VoxelCell cell))
			{
				continue;
			}

			p_hit = new Hit(worldPosition, cell, distance, point);
			return true;
		}

		return false;
	}
}
