using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Voxel.Utils
{
	public static class Geometry
	{
		public const float NormalEpsilon = 0.001f;

		public const float PointEpsilon = 0.001f;

		public static readonly Erelia.Core.Voxel.Face[] FullOuterFaces =
		{
			Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 0f), TileUV = Vector2.zero }),

			Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = Vector2.zero }),

			Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = Vector2.zero }),

			Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = Vector2.zero }),

			Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = Vector2.zero }),

			Erelia.Core.Voxel.Utils.Geometry.CreateRectangle(
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(1f, 1f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.Voxel.Face.Vertex { Position = new Vector3(0f, 1f, 0f), TileUV = Vector2.zero }),
		};

		public static Erelia.Core.Voxel.Face CreateRectangle(
			Erelia.Core.Voxel.Face.Vertex a,
			Erelia.Core.Voxel.Face.Vertex b,
			Erelia.Core.Voxel.Face.Vertex c,
			Erelia.Core.Voxel.Face.Vertex d)
		{
			var face = new Erelia.Core.Voxel.Face();

			var verts = new List<Erelia.Core.Voxel.Face.Vertex>(4)
			{
				new Erelia.Core.Voxel.Face.Vertex { Position = a.Position, TileUV = a.TileUV },
				new Erelia.Core.Voxel.Face.Vertex { Position = b.Position, TileUV = b.TileUV },
				new Erelia.Core.Voxel.Face.Vertex { Position = c.Position, TileUV = c.TileUV },
				new Erelia.Core.Voxel.Face.Vertex { Position = d.Position, TileUV = d.TileUV }
			};

			face.AddPolygon(verts);
			return face;
		}

		public static Erelia.Core.Voxel.Face CreateTriangle(
			Erelia.Core.Voxel.Face.Vertex a,
			Erelia.Core.Voxel.Face.Vertex b,
			Erelia.Core.Voxel.Face.Vertex c)
		{
			var face = new Erelia.Core.Voxel.Face();

			var verts = new List<Erelia.Core.Voxel.Face.Vertex>(3)
			{
				new Erelia.Core.Voxel.Face.Vertex { Position = a.Position, TileUV = a.TileUV },
				new Erelia.Core.Voxel.Face.Vertex { Position = b.Position, TileUV = b.TileUV },
				new Erelia.Core.Voxel.Face.Vertex { Position = c.Position, TileUV = c.TileUV }
			};

			face.AddPolygon(verts);
			return face;
		}

		public static Vector3 GetNormal(List<Erelia.Core.Voxel.Face.Vertex> verts)
		{
			Vector3 a = verts[0].Position;
			Vector3 b = verts[1].Position;
			Vector3 c = verts[2].Position;

			return Vector3.Cross(b - a, c - a);
		}

		public static bool IsPolygonContained(
			List<Erelia.Core.Voxel.Face.Vertex> polygon,
			List<Erelia.Core.Voxel.Face.Vertex> container,
			Vector3 normal)
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

		public static bool IsPolygonContainedInUnion(
			List<Erelia.Core.Voxel.Face.Vertex> polygon,
			List<List<Erelia.Core.Voxel.Face.Vertex>> containers,
			Vector3 normal)
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
				List<Erelia.Core.Voxel.Face.Vertex> container = containers[i];
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

		public static bool TryBuildBasis(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
		{
			if (normal.sqrMagnitude < NormalEpsilon)
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

		public static float Cross(Vector2 a, Vector2 b)
		{
			return a.x * b.y - a.y * b.x;
		}

		public static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
		{
			float area = Cross(b - a, c - a);

			float area1 = Cross(b - a, p - a);
			float area2 = Cross(c - b, p - b);
			float area3 = Cross(a - c, p - c);

			bool hasNeg = (area1 < 0f) || (area2 < 0f) || (area3 < 0f);
			bool hasPos = (area1 > 0f) || (area2 > 0f) || (area3 > 0f);

			if (area < 0f)
			{
				hasNeg = (area1 > 0f) || (area2 > 0f) || (area3 > 0f);
				hasPos = (area1 < 0f) || (area2 < 0f) || (area3 < 0f);
			}

			return !(hasNeg && hasPos);
		}

		public static bool TryFromNormal(Vector3 normal, out Erelia.Core.Voxel.AxisPlane plane)
		{
			if (normal.sqrMagnitude < NormalEpsilon)
			{
				plane = Erelia.Core.Voxel.AxisPlane.PosX;
				return false;
			}

			Vector3 n = normal.normalized;

			float ax = Mathf.Abs(n.x);
			float ay = Mathf.Abs(n.y);
			float az = Mathf.Abs(n.z);

			if (ax >= 1f - NormalEpsilon && ay <= NormalEpsilon && az <= NormalEpsilon)
			{
				plane = n.x >= 0f ? Erelia.Core.Voxel.AxisPlane.PosX : Erelia.Core.Voxel.AxisPlane.NegX;
				return true;
			}

			if (ay >= 1f - NormalEpsilon && ax <= NormalEpsilon && az <= NormalEpsilon)
			{
				plane = n.y >= 0f ? Erelia.Core.Voxel.AxisPlane.PosY : Erelia.Core.Voxel.AxisPlane.NegY;
				return true;
			}

			if (az >= 1f - NormalEpsilon && ax <= NormalEpsilon && ay <= NormalEpsilon)
			{
				plane = n.z >= 0f ? Erelia.Core.Voxel.AxisPlane.PosZ : Erelia.Core.Voxel.AxisPlane.NegZ;
				return true;
			}

			plane = Erelia.Core.Voxel.AxisPlane.PosX;
			return false;
		}

		public static Vector3 PlaneToNormal(Erelia.Core.Voxel.AxisPlane plane)
		{
			switch (plane)
			{
				case Erelia.Core.Voxel.AxisPlane.PosX:
					return Vector3.right;
				case Erelia.Core.Voxel.AxisPlane.NegX:
					return Vector3.left;
				case Erelia.Core.Voxel.AxisPlane.PosY:
					return Vector3.up;
				case Erelia.Core.Voxel.AxisPlane.NegY:
					return Vector3.down;
				case Erelia.Core.Voxel.AxisPlane.PosZ:
					return Vector3.forward;
				case Erelia.Core.Voxel.AxisPlane.NegZ:
					return Vector3.back;
				default:
					return Vector3.zero;
			}
		}

		public static Vector3Int PlaneToOffset(Erelia.Core.Voxel.AxisPlane plane)
		{
			switch (plane)
			{
				case Erelia.Core.Voxel.AxisPlane.PosX:
					return new Vector3Int(1, 0, 0);
				case Erelia.Core.Voxel.AxisPlane.NegX:
					return new Vector3Int(-1, 0, 0);
				case Erelia.Core.Voxel.AxisPlane.PosY:
					return new Vector3Int(0, 1, 0);
				case Erelia.Core.Voxel.AxisPlane.NegY:
					return new Vector3Int(0, -1, 0);
				case Erelia.Core.Voxel.AxisPlane.PosZ:
					return new Vector3Int(0, 0, 1);
				case Erelia.Core.Voxel.AxisPlane.NegZ:
					return new Vector3Int(0, 0, -1);
				default:
					return Vector3Int.zero;
			}
		}

		public static Erelia.Core.Voxel.AxisPlane GetOppositePlane(Erelia.Core.Voxel.AxisPlane plane)
		{
			switch (plane)
			{
				case Erelia.Core.Voxel.AxisPlane.PosX:
					return Erelia.Core.Voxel.AxisPlane.NegX;
				case Erelia.Core.Voxel.AxisPlane.NegX:
					return Erelia.Core.Voxel.AxisPlane.PosX;
				case Erelia.Core.Voxel.AxisPlane.PosY:
					return Erelia.Core.Voxel.AxisPlane.NegY;
				case Erelia.Core.Voxel.AxisPlane.NegY:
					return Erelia.Core.Voxel.AxisPlane.PosY;
				case Erelia.Core.Voxel.AxisPlane.PosZ:
					return Erelia.Core.Voxel.AxisPlane.NegZ;
				case Erelia.Core.Voxel.AxisPlane.NegZ:
					return Erelia.Core.Voxel.AxisPlane.PosZ;
				default:
					return plane;
			}
		}

		public static Erelia.Core.Voxel.AxisPlane MapWorldPlaneToLocal(
			Erelia.Core.Voxel.AxisPlane plane,
			Erelia.Core.Voxel.Orientation orientation)
		{
			return RotatePlane(plane, -(int)orientation);
		}

		public static Erelia.Core.Voxel.AxisPlane MapWorldPlaneToLocal(
			Erelia.Core.Voxel.AxisPlane plane,
			Erelia.Core.Voxel.Orientation orientation,
			Erelia.Core.Voxel.FlipOrientation flipOrientation)
		{
			Erelia.Core.Voxel.AxisPlane rotated = RotatePlane(plane, -(int)orientation);

			if (flipOrientation == Erelia.Core.Voxel.FlipOrientation.NegativeY)
			{
				return FlipPlaneY(rotated);
			}

			return rotated;
		}

		public static Erelia.Core.Voxel.AxisPlane RotatePlane(Erelia.Core.Voxel.AxisPlane plane, int steps)
		{
			int normalized = ((steps % 4) + 4) % 4;
			if (normalized == 0)
			{
				return plane;
			}

			Vector3 normal = PlaneToNormal(plane);
			Quaternion rotation = Quaternion.AngleAxis(-normalized * 90f, Vector3.up);
			Vector3 rotatedNormal = rotation * normal;

			if (TryFromNormal(rotatedNormal, out Erelia.Core.Voxel.AxisPlane rotatedPlane))
			{
				return rotatedPlane;
			}

			return plane;
		}

		public static Erelia.Core.Voxel.AxisPlane FlipPlaneY(Erelia.Core.Voxel.AxisPlane plane)
		{
			switch (plane)
			{
				case Erelia.Core.Voxel.AxisPlane.PosY:
					return Erelia.Core.Voxel.AxisPlane.NegY;
				case Erelia.Core.Voxel.AxisPlane.NegY:
					return Erelia.Core.Voxel.AxisPlane.PosY;
				default:
					return plane;
			}
		}

		public static Erelia.Core.Voxel.Face TransformFace(
			Erelia.Core.Voxel.Face face,
			Erelia.Core.Voxel.Orientation orientation,
			Erelia.Core.Voxel.FlipOrientation flipOrientation)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return face;
			}

			int steps = (int)orientation;

			var rotated = new Erelia.Core.Voxel.Face();

			Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);

			List<List<Erelia.Core.Voxel.Face.Vertex>> sourcePolygons = face.Polygons;
			for (int p = 0; p < sourcePolygons.Count; p++)
			{
				List<Erelia.Core.Voxel.Face.Vertex> sourceVertices = sourcePolygons[p];
				if (sourceVertices == null || sourceVertices.Count == 0)
				{
					continue;
				}

				var rotatedPolygon = new List<Erelia.Core.Voxel.Face.Vertex>(sourceVertices.Count);
				for (int i = 0; i < sourceVertices.Count; i++)
				{
					Erelia.Core.Voxel.Face.Vertex vertex = sourceVertices[i];
					Vector3 local = vertex.Position;

					if (steps != 0)
					{
						Vector3 offset = local - pivot;
						Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
						local = rotation * offset + pivot;
					}

					if (flipOrientation == Erelia.Core.Voxel.FlipOrientation.NegativeY)
					{
						local.y = 1f - local.y;
					}

					vertex.Position = local;
					rotatedPolygon.Add(vertex);
				}

				if (flipOrientation == Erelia.Core.Voxel.FlipOrientation.NegativeY)
				{
					rotatedPolygon.Reverse();
				}

				rotated.Polygons.Add(rotatedPolygon);
			}

			return rotated;
		}

		public static Vector3 TransformPoint(
			Vector3 point,
			Erelia.Core.Voxel.Orientation orientation,
			Erelia.Core.Voxel.FlipOrientation flipOrientation)
		{
			int steps = (int)orientation;

			Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);

			Vector3 local = point;

			if (steps != 0)
			{
				Vector3 offset = local - pivot;
				Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
				local = rotation * offset + pivot;
			}

			if (flipOrientation == Erelia.Core.Voxel.FlipOrientation.NegativeY)
			{
				local.y = 1f - local.y;
			}

			return local;
		}

		public static bool IsFaceCoplanarWithPlane(Erelia.Core.Voxel.Face face, Erelia.Core.Voxel.AxisPlane plane)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return false;
			}

			float target = 0f;
			int axis = 0;
			switch (plane)
			{
				case Erelia.Core.Voxel.AxisPlane.PosX:
					axis = 0; target = 1f; break;
				case Erelia.Core.Voxel.AxisPlane.NegX:
					axis = 0; target = 0f; break;
				case Erelia.Core.Voxel.AxisPlane.PosY:
					axis = 1; target = 1f; break;
				case Erelia.Core.Voxel.AxisPlane.NegY:
					axis = 1; target = 0f; break;
				case Erelia.Core.Voxel.AxisPlane.PosZ:
					axis = 2; target = 1f; break;
				case Erelia.Core.Voxel.AxisPlane.NegZ:
					axis = 2; target = 0f; break;
			}

			List<List<Erelia.Core.Voxel.Face.Vertex>> polygons = face.Polygons;
			for (int p = 0; p < polygons.Count; p++)
			{
				List<Erelia.Core.Voxel.Face.Vertex> polygon = polygons[p];
				if (polygon == null)
				{
					continue;
				}

				for (int i = 0; i < polygon.Count; i++)
				{
					Vector3 pos = polygon[i].Position;
					float value = axis == 0 ? pos.x : axis == 1 ? pos.y : pos.z;

					if (Mathf.Abs(value - target) > Erelia.Core.Voxel.Utils.Geometry.PointEpsilon)
					{
						return false;
					}
				}
			}

			return true;
		}

		public static bool IsFullFace(Erelia.Core.Voxel.Face face, Erelia.Core.Voxel.AxisPlane plane)
		{
			if (face == null || !face.HasRenderablePolygons)
			{
				return false;
			}

			Erelia.Core.Voxel.Face fullFace = FullOuterFaces[(int)plane];
			if (fullFace == null)
			{
				return false;
			}

			if (!IsFaceCoplanarWithPlane(face, plane))
			{
				return false;
			}

			return fullFace.IsOccludedBy(face) && face.IsOccludedBy(fullFace);
		}
	}

	public static class SpriteUv
	{
		public static void GetSpriteUvRect(Sprite sprite, out Vector2 uvAnchor, out Vector2 uvSize)
		{
			if (sprite == null || sprite.uv == null || sprite.uv.Length == 0)
			{
				uvAnchor = Vector2.zero;
				uvSize = Vector2.one;
				return;
			}

			Vector2 min = sprite.uv[0];
			Vector2 max = sprite.uv[0];

			for (int i = 1; i < sprite.uv.Length; i++)
			{
				Vector2 uv = sprite.uv[i];
				min = Vector2.Min(min, uv);
				max = Vector2.Max(max, uv);
			}

			uvAnchor = min;
			uvSize = max - min;
		}
	}
}
