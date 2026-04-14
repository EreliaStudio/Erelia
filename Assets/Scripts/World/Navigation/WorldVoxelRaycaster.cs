using System;
using UnityEngine;

public static class WorldVoxelRaycaster
{
	private const float SurfaceResolveEpsilon = 0.01f;

	public readonly struct Hit
	{
		public readonly Vector3Int WorldPosition;
		public readonly VoxelCell Cell;
		public readonly VoxelDefinition Definition;
		public readonly float Distance;
		public readonly Vector3 Point;

		public Hit(Vector3Int p_worldPosition, VoxelCell p_cell, VoxelDefinition p_definition, float p_distance, Vector3 p_point)
		{
			WorldPosition = p_worldPosition;
			Cell = p_cell;
			Definition = p_definition;
			Distance = p_distance;
			Point = p_point;
		}
	}

	public static bool TryRaycast(WorldData p_worldData, Ray p_ray, float p_maxDistance, float p_stepDistance, out Hit p_hit)
	{
		return TryRaycast(p_worldData, null, p_ray, p_maxDistance, p_stepDistance, null, out p_hit);
	}

	public static bool TryRaycast(
		WorldData p_worldData,
		VoxelRegistry p_voxelRegistry,
		Ray p_ray,
		float p_maxDistance,
		float p_stepDistance,
		Predicate<Hit> p_hitPredicate,
		out Hit p_hit)
	{
		p_hit = default;

		if (p_worldData == null || p_maxDistance <= 0f)
		{
			return false;
		}

		if (!Physics.Raycast(p_ray, out RaycastHit physicsHit, p_maxDistance))
		{
			return false;
		}

		ChunkPresenter chunkPresenter = physicsHit.collider != null ? physicsHit.collider.GetComponentInParent<ChunkPresenter>() : null;
		if (chunkPresenter == null)
		{
			return false;
		}

		if (TryResolveHit(chunkPresenter, p_worldData, p_voxelRegistry, physicsHit.point, physicsHit.normal, physicsHit.distance, p_hitPredicate, out p_hit))
		{
			return true;
		}

		return TryResolveHit(chunkPresenter, p_worldData, p_voxelRegistry, physicsHit.point, -physicsHit.normal, physicsHit.distance, p_hitPredicate, out p_hit);
	}

	public static Predicate<Hit> ByTraversal(VoxelTraversal p_traversal)
	{
		return p_hit =>
			p_hit.Definition != null &&
			p_hit.Definition.Data != null &&
			p_hit.Definition.Data.Traversal == p_traversal;
	}

	private static bool TryResolveHit(
		ChunkPresenter p_chunkPresenter,
		WorldData p_worldData,
		VoxelRegistry p_voxelRegistry,
		Vector3 p_surfacePoint,
		Vector3 p_surfaceNormal,
		float p_distance,
		Predicate<Hit> p_hitPredicate,
		out Hit p_hit)
	{
		p_hit = default;

		Vector3 inwardPoint = p_surfacePoint - (p_surfaceNormal.normalized * SurfaceResolveEpsilon);
		Vector3 localPoint = p_chunkPresenter.transform.InverseTransformPoint(inwardPoint);
		Vector3Int localCell = Vector3Int.FloorToInt(localPoint);

		if (!TryGetHitFromLocalCell(p_chunkPresenter, p_worldData, p_voxelRegistry, localCell, p_distance, p_surfacePoint, p_hitPredicate, out p_hit))
		{
			Vector3 unshiftedLocalPoint = p_chunkPresenter.transform.InverseTransformPoint(p_surfacePoint);
			Vector3Int fallbackLocalCell = Vector3Int.FloorToInt(unshiftedLocalPoint);
			if (!TryGetHitFromLocalCell(p_chunkPresenter, p_worldData, p_voxelRegistry, fallbackLocalCell, p_distance, p_surfacePoint, p_hitPredicate, out p_hit))
			{
				return false;
			}
		}

		return true;
	}

	private static bool TryGetHitFromLocalCell(
		ChunkPresenter p_chunkPresenter,
		WorldData p_worldData,
		VoxelRegistry p_voxelRegistry,
		Vector3Int p_localCell,
		float p_distance,
		Vector3 p_surfacePoint,
		Predicate<Hit> p_hitPredicate,
		out Hit p_hit)
	{
		p_hit = default;

		ChunkData chunkData = p_chunkPresenter.ChunkData;
		if (chunkData == null || p_localCell.x < 0 || p_localCell.x >= chunkData.SizeX ||
			p_localCell.y < 0 || p_localCell.y >= chunkData.SizeY ||
			p_localCell.z < 0 || p_localCell.z >= chunkData.SizeZ)
		{
			return false;
		}

		VoxelCell cell = chunkData.Cells[p_localCell.x, p_localCell.y, p_localCell.z];
		if (cell == null || cell.IsEmpty)
		{
			return false;
		}

		Vector3Int worldPosition = Vector3Int.FloorToInt(p_chunkPresenter.transform.TransformPoint(p_localCell));
		if (!p_worldData.TryGetCell(worldPosition, out VoxelCell worldCell))
		{
			return false;
		}

		VoxelDefinition definition = null;
		if (p_voxelRegistry != null)
		{
			p_voxelRegistry.TryGetVoxel(worldCell.Id, out definition);
		}

		Hit candidateHit = new Hit(worldPosition, worldCell, definition, p_distance, p_surfacePoint);
		if (p_hitPredicate != null && !p_hitPredicate(candidateHit))
		{
			return false;
		}

		p_hit = candidateHit;
		return true;
	}
}
