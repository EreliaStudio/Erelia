using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{
	public struct Vertex
	{
		public Vector3 Position;
		public Vector2 UV;
	}

	public static VoxelFace CreateRectangle(Vertex a, Vertex b, Vertex c, Vertex d)
	{
		var face = new VoxelFace();
		face.AddPolygon(CreateRectanglePolygon(a, b, c, d));
		return face;
	}

	public static VoxelFace CreateTriangle(Vertex a, Vertex b, Vertex c)
	{
		var face = new VoxelFace();
		face.AddPolygon(CreateTrianglePolygon(a, b, c));
		return face;
	}

	public static List<FaceVertex> CreateRectanglePolygon(Vertex a, Vertex b, Vertex c, Vertex d)
	{
		var verts = new List<FaceVertex>(4)
		{
			new FaceVertex { Position = a.Position, TileUV = a.UV },
			new FaceVertex { Position = b.Position, TileUV = b.UV },
			new FaceVertex { Position = c.Position, TileUV = c.UV },
			new FaceVertex { Position = d.Position, TileUV = d.UV }
		};
		return verts;
	}

	public static List<FaceVertex> CreateTrianglePolygon(Vertex a, Vertex b, Vertex c)
	{
		var verts = new List<FaceVertex>(3)
		{
			new FaceVertex { Position = a.Position, TileUV = a.UV },
			new FaceVertex { Position = b.Position, TileUV = b.UV },
			new FaceVertex { Position = c.Position, TileUV = c.UV }
		};
		return verts;
	}

	public static Vector3 GetNormal(List<FaceVertex> verts)
	{
		Vector3 a = verts[0].Position;
		Vector3 b = verts[1].Position;
		Vector3 c = verts[2].Position;
		return Vector3.Cross(b - a, c - a);
	}

	public static bool IsPolygonContained(List<FaceVertex> polygon, List<FaceVertex> container, Vector3 normal)
	{
		if (polygon == null || container == null || polygon.Count < 3 || container.Count < 3)
		{
			return false;
		}

		if (!TryBuildBasis(normal, out Vector3 tangent, out Vector3 bitangent))
		{
			return false;
		}

		var poly2D = new List<Vector2>(polygon.Count);
		var container2D = new List<Vector2>(container.Count);

		for (int i = 0; i < polygon.Count; i++)
		{
			Vector3 p = polygon[i].Position;
			poly2D.Add(new Vector2(Vector3.Dot(p, tangent), Vector3.Dot(p, bitangent)));
		}

		for (int i = 0; i < container.Count; i++)
		{
			Vector3 p = container[i].Position;
			container2D.Add(new Vector2(Vector3.Dot(p, tangent), Vector3.Dot(p, bitangent)));
		}

		for (int i = 0; i < poly2D.Count; i++)
		{
			if (!IsPointInPolygon(poly2D[i], container2D))
			{
				return false;
			}
		}

		return true;
	}

	public static bool IsPolygonContainedInUnion(List<FaceVertex> polygon, List<List<FaceVertex>> containers, Vector3 normal)
	{
		if (polygon == null || polygon.Count < 3 || containers == null)
		{
			return false;
		}

		if (!TryBuildBasis(normal, out Vector3 tangent, out Vector3 bitangent))
		{
			return false;
		}

		var poly2D = new List<Vector2>(polygon.Count);
		for (int i = 0; i < polygon.Count; i++)
		{
			Vector3 p = polygon[i].Position;
			poly2D.Add(new Vector2(Vector3.Dot(p, tangent), Vector3.Dot(p, bitangent)));
		}

		var container2Ds = new List<List<Vector2>>(containers.Count);
		for (int i = 0; i < containers.Count; i++)
		{
			List<FaceVertex> container = containers[i];
			if (container == null || container.Count < 3)
			{
				continue;
			}

			var container2D = new List<Vector2>(container.Count);
			for (int j = 0; j < container.Count; j++)
			{
				Vector3 p = container[j].Position;
				container2D.Add(new Vector2(Vector3.Dot(p, tangent), Vector3.Dot(p, bitangent)));
			}
			container2Ds.Add(container2D);
		}

		if (container2Ds.Count == 0)
		{
			return false;
		}

		if (!AreSamplePointsContained(poly2D, container2Ds))
		{
			return false;
		}

		return true;
	}

	private static bool AreSamplePointsContained(List<Vector2> polygon, List<List<Vector2>> containers)
	{
		if (polygon == null || polygon.Count < 3)
		{
			return false;
		}

		Vector2 centroid = Vector2.zero;
		for (int i = 0; i < polygon.Count; i++)
		{
			centroid += polygon[i];
		}
		centroid /= polygon.Count;

		if (!IsPointInUnion(centroid, containers))
		{
			return false;
		}

		for (int i = 0; i < polygon.Count; i++)
		{
			Vector2 a = polygon[i];
			Vector2 b = polygon[(i + 1) % polygon.Count];
			Vector2 mid = (a + b) * 0.5f;
			if (!IsPointInUnion(a, containers) || !IsPointInUnion(mid, containers))
			{
				return false;
			}
		}

		return true;
	}

	private static bool IsPointInUnion(Vector2 point, List<List<Vector2>> polygons)
	{
		for (int i = 0; i < polygons.Count; i++)
		{
			if (IsPointInPolygon(point, polygons[i]))
			{
				return true;
			}
		}

		return false;
	}

	private static bool TryBuildBasis(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
	{
		if (normal.sqrMagnitude < OuterShellPlaneUtil.NormalEpsilon)
		{
			tangent = Vector3.zero;
			bitangent = Vector3.zero;
			return false;
		}

		Vector3 n = normal.normalized;
		Vector3 up = Mathf.Abs(n.y) < 0.99f ? Vector3.up : Vector3.right;
		tangent = Vector3.Cross(up, n).normalized;
		bitangent = Vector3.Cross(n, tangent);
		return true;
	}

	private static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
	{
		bool inside = false;
		for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
		{
			Vector2 pi = polygon[i];
			Vector2 pj = polygon[j];

			if (IsPointOnSegment(point, pj, pi))
			{
				return true;
			}

			bool intersect = (pi.y > point.y) != (pj.y > point.y)
				&& point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y + 0.000001f) + pi.x;
			if (intersect)
			{
				inside = !inside;
			}
		}

		return inside;
	}

	private static bool IsPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
	{
		float cross = (point.y - a.y) * (b.x - a.x) - (point.x - a.x) * (b.y - a.y);
		if (Mathf.Abs(cross) > 0.0001f)
		{
			return false;
		}

		float dot = (point.x - a.x) * (b.x - a.x) + (point.y - a.y) * (b.y - a.y);
		if (dot < -0.0001f)
		{
			return false;
		}

		float lenSq = (b - a).sqrMagnitude;
		return dot <= lenSq + 0.0001f;
	}
}
