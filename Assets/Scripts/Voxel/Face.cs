using System.Collections.Generic;
using UnityEngine;

namespace VoxelKit
{
	public class Face
	{
		public struct Vertex
		{
			public Vector3 Position;
			public Vector2 TileUV;
		}

		public List<List<Vertex>> Polygons = new List<List<Vertex>>();

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
					List<Vertex> polygon = Polygons[i];
					if (polygon != null && polygon.Count >= 3)
					{
						return true;
					}
				}

				return false;
			}
		}

		public void AddPolygon(List<Vertex> polygon)
		{
			if (polygon == null || polygon.Count == 0)
			{
				return;
			}

			Polygons.Add(polygon);
		}

		public void ApplyOffset(Vector2 tileOffset)
		{
			for (int p = 0; p < Polygons.Count; p++)
			{
				List<Vertex> polygon = Polygons[p];

				for (int i = 0; i < polygon.Count; i++)
				{
					Vertex vertex = polygon[i];
					vertex.TileUV += tileOffset;
					polygon[i] = vertex;
				}
			}
		}

		public bool IsOccludedBy(Face other)
		{
			if (other == null || !HasRenderablePolygons || !other.HasRenderablePolygons)
			{
				return false;
			}

			Vector3 normal = Vector3.zero;
			for (int p = 0; p < Polygons.Count; p++)
			{
				List<Vertex> polygon = Polygons[p];
				if (polygon != null && polygon.Count >= 3)
				{
					normal = VoxelKit.Utils.Geometry.GetNormal(polygon);
					if (normal.sqrMagnitude >= VoxelKit.Utils.Geometry.NormalEpsilon)
					{
						break;
					}
				}
			}

			if (normal.sqrMagnitude < VoxelKit.Utils.Geometry.NormalEpsilon)
			{
				return false;
			}

			for (int p = 0; p < Polygons.Count; p++)
			{
				List<Vertex> polygon = Polygons[p];
				if (polygon == null || polygon.Count < 3)
				{
					continue;
				}

				if (!VoxelKit.Utils.Geometry.IsPolygonContainedInUnion(polygon, other.Polygons, normal))
				{
					return false;
				}
			}

			return true;
		}
	}
}



