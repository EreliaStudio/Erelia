using UnityEngine;

public static class BoardVoxelRaycaster
{
	private const float SurfaceResolveEpsilon = 0.01f;
	private static readonly int WorldLayerMask = LayerMask.GetMask("World");

	public readonly struct Hit
	{
		public readonly Vector3Int LocalPosition;
		public readonly float Distance;
		public readonly Vector3 Point;

		public Hit(Vector3Int p_localPosition, float p_distance, Vector3 p_point)
		{
			LocalPosition = p_localPosition;
			Distance = p_distance;
			Point = p_point;
		}
	}

	public static bool TryRaycast(BoardPresenter p_boardPresenter, Ray p_ray, float p_maxDistance, out Hit p_hit)
	{
		if (p_boardPresenter == null || p_boardPresenter.BoardData == null || p_maxDistance <= 0f)
		{
			p_hit = default;
			return false;
		}

		if (!Physics.Raycast(p_ray, out RaycastHit physicsHit, p_maxDistance, WorldLayerMask))
		{
			p_hit = default;
			return false;
		}

		return TryResolveHit(p_boardPresenter, physicsHit.point, physicsHit.normal, physicsHit.distance, out p_hit) ||
			TryResolveHit(p_boardPresenter, physicsHit.point, -physicsHit.normal, physicsHit.distance, out p_hit);
	}

	private static bool TryResolveHit(BoardPresenter p_boardPresenter, Vector3 p_surfacePoint, Vector3 p_surfaceNormal, float p_distance, out Hit p_hit)
	{
		p_hit = default;

		Vector3 inwardPoint = p_surfacePoint - (p_surfaceNormal.normalized * SurfaceResolveEpsilon);
		Vector3 localPoint = p_boardPresenter.transform.InverseTransformPoint(inwardPoint);
		Vector3Int localCell = Vector3Int.FloorToInt(localPoint);

		if (!TryGetHitFromLocalCell(p_boardPresenter.BoardData, localCell, p_distance, p_surfacePoint, out p_hit))
		{
			Vector3 unshiftedLocalPoint = p_boardPresenter.transform.InverseTransformPoint(p_surfacePoint);
			Vector3Int fallbackLocalCell = Vector3Int.FloorToInt(unshiftedLocalPoint);
			if (!TryGetHitFromLocalCell(p_boardPresenter.BoardData, fallbackLocalCell, p_distance, p_surfacePoint, out p_hit))
			{
				return false;
			}
		}

		return true;
	}

	private static bool TryGetHitFromLocalCell(BoardData p_boardData, Vector3Int p_localCell, float p_distance, Vector3 p_surfacePoint, out Hit p_hit)
	{
		p_hit = default;

		if (p_boardData == null || !p_boardData.IsInside(p_localCell) || p_boardData.Terrain.IsEmpty(p_localCell))
		{
			return false;
		}

		p_hit = new Hit(p_localCell, p_distance, p_surfacePoint);
		return true;
	}
}
