using System.Collections.Generic;
using UnityEngine;

public struct FaceVertex
{
	public Vector3 Position;
	public Vector2 TileUV;
}

public class VoxelFace
{
	public List<List<FaceVertex>> Polygons = new List<List<FaceVertex>>();

	public bool HasRenderablePolygons
	{
		get
		{
			if (Polygons == null)
			{
				return false;
			}

			for (int i = 0; i < Polygons.Count; i++)
			{
				List<FaceVertex> polygon = Polygons[i];
				if (polygon != null && polygon.Count >= 3)
				{
					return true;
				}
			}

			return false;
		}
	}

	public void AddPolygon(List<FaceVertex> polygon)
	{
		if (polygon == null || polygon.Count == 0)
		{
			return;
		}

		if (Polygons == null)
		{
			Polygons = new List<List<FaceVertex>>();
		}

		Polygons.Add(polygon);
	}

	public void ApplyOffset(Vector2 tileOffset)
	{
		if (Polygons == null)
		{
			return;
		}

		for (int p = 0; p < Polygons.Count; p++)
		{
			List<FaceVertex> polygon = Polygons[p];
			if (polygon == null)
			{
				continue;
			}

			for (int i = 0; i < polygon.Count; i++)
			{
				FaceVertex vertex = polygon[i];
				vertex.TileUV += tileOffset;
				polygon[i] = vertex;
			}
		}
	}

	public bool IsOccludedBy(VoxelFace other)
	{
		if (other == null || Polygons == null || other.Polygons == null || !HasRenderablePolygons || !other.HasRenderablePolygons)
		{
			return false;
		}

		Vector3 normal = Vector3.zero;
		for (int p = 0; p < Polygons.Count; p++)
		{
			List<FaceVertex> polygon = Polygons[p];
			if (polygon != null && polygon.Count >= 3)
			{
				normal = GeometryUtils.GetNormal(polygon);
				if (normal.sqrMagnitude >= OuterShellPlaneUtil.NormalEpsilon)
				{
					break;
				}
			}
		}

		if (normal.sqrMagnitude < OuterShellPlaneUtil.NormalEpsilon)
		{
			return false;
		}

		for (int p = 0; p < Polygons.Count; p++)
		{
			List<FaceVertex> polygon = Polygons[p];
			if (polygon == null || polygon.Count < 3)
			{
				continue;
			}

			if (!GeometryUtils.IsPolygonContainedInUnion(polygon, other.Polygons, normal))
			{
				return false;
			}
		}

		return true;
	}
}
