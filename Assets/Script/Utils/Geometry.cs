using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public static class Geometry
	{
		public const float NormalEpsilon = 0.001f;
		public const float PointEpsilon = 0.001f;

		private static readonly Voxel.View.Face FullPosXFace = Utils.Geometry.CreateRectangle(
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = Vector2.zero });
		private static readonly Voxel.View.Face FullNegXFace = Utils.Geometry.CreateRectangle(
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = Vector2.zero });
		private static readonly Voxel.View.Face FullPosYFace = Utils.Geometry.CreateRectangle(
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = Vector2.zero });
		private static readonly Voxel.View.Face FullNegYFace = Utils.Geometry.CreateRectangle(
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = Vector2.zero });
		private static readonly Voxel.View.Face FullPosZFace = Utils.Geometry.CreateRectangle(
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = Vector2.zero });
		private static readonly Voxel.View.Face FullNegZFace = Utils.Geometry.CreateRectangle(
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = Vector2.zero },
			new Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = Vector2.zero });

		public struct Vertex
		{
			public Vector3 Position;
			public Vector2 UV;
		}

		public static Voxel.View.Face CreateRectangle(Vertex a, Vertex b, Vertex c, Vertex d)
		{
			var face = new Voxel.View.Face();
			face.AddPolygon(CreateRectanglePolygon(a, b, c, d));
			return face;
		}

		public static Voxel.View.Face CreateTriangle(Vertex a, Vertex b, Vertex c)
		{
			var face = new Voxel.View.Face();
			face.AddPolygon(CreateTrianglePolygon(a, b, c));
			return face;
		}

		public static List<Voxel.View.Face.Vertex> CreateRectanglePolygon(Vertex a, Vertex b, Vertex c, Vertex d)
		{
			var verts = new List<Voxel.View.Face.Vertex>(4)
		{
			new Voxel.View.Face.Vertex { Position = a.Position, TileUV = a.UV },
			new Voxel.View.Face.Vertex { Position = b.Position, TileUV = b.UV },
			new Voxel.View.Face.Vertex { Position = c.Position, TileUV = c.UV },
			new Voxel.View.Face.Vertex { Position = d.Position, TileUV = d.UV }
		};
			return verts;
		}

		public static List<Voxel.View.Face.Vertex> CreateTrianglePolygon(Vertex a, Vertex b, Vertex c)
		{
			var verts = new List<Voxel.View.Face.Vertex>(3)
		{
			new Voxel.View.Face.Vertex { Position = a.Position, TileUV = a.UV },
			new Voxel.View.Face.Vertex { Position = b.Position, TileUV = b.UV },
			new Voxel.View.Face.Vertex { Position = c.Position, TileUV = c.UV }
		};
			return verts;
		}

		public static Vector3 GetNormal(List<Voxel.View.Face.Vertex> verts)
		{
			Vector3 a = verts[0].Position;
			Vector3 b = verts[1].Position;
			Vector3 c = verts[2].Position;
			return Vector3.Cross(b - a, c - a);
		}

		public static bool IsPolygonContained(List<Voxel.View.Face.Vertex> polygon, List<Voxel.View.Face.Vertex> container, Vector3 normal)
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

		public static bool IsPolygonContainedInUnion(List<Voxel.View.Face.Vertex> polygon, List<List<Voxel.View.Face.Vertex>> containers, Vector3 normal)
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
				List<Voxel.View.Face.Vertex> container = containers[i];
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

		public static bool TryFromNormal(Vector3 normal, out Voxel.View.Shape.OuterShellPlane plane)
		{
			if (normal.sqrMagnitude < NormalEpsilon)
			{
				plane = Voxel.View.Shape.OuterShellPlane.PosX;
				return false;
			}

			Vector3 n = normal.normalized;
			float ax = Mathf.Abs(n.x);
			float ay = Mathf.Abs(n.y);
			float az = Mathf.Abs(n.z);

			if (ax >= 1f - NormalEpsilon && ay <= NormalEpsilon && az <= NormalEpsilon)
			{
				plane = n.x >= 0f ? Voxel.View.Shape.OuterShellPlane.PosX : Voxel.View.Shape.OuterShellPlane.NegX;
				return true;
			}
			if (ay >= 1f - NormalEpsilon && ax <= NormalEpsilon && az <= NormalEpsilon)
			{
				plane = n.y >= 0f ? Voxel.View.Shape.OuterShellPlane.PosY : Voxel.View.Shape.OuterShellPlane.NegY;
				return true;
			}
			if (az >= 1f - NormalEpsilon && ax <= NormalEpsilon && ay <= NormalEpsilon)
			{
				plane = n.z >= 0f ? Voxel.View.Shape.OuterShellPlane.PosZ : Voxel.View.Shape.OuterShellPlane.NegZ;
				return true;
			}

			plane = Voxel.View.Shape.OuterShellPlane.PosX;
			return false;
		}

		public static Vector3 PlaneToNormal(Voxel.View.Shape.OuterShellPlane plane)
		{
			switch (plane)
			{
				case Voxel.View.Shape.OuterShellPlane.PosX:
					return Vector3.right;
				case Voxel.View.Shape.OuterShellPlane.NegX:
					return Vector3.left;
				case Voxel.View.Shape.OuterShellPlane.PosY:
					return Vector3.up;
				case Voxel.View.Shape.OuterShellPlane.NegY:
					return Vector3.down;
				case Voxel.View.Shape.OuterShellPlane.PosZ:
					return Vector3.forward;
				case Voxel.View.Shape.OuterShellPlane.NegZ:
					return Vector3.back;
				default:
					return Vector3.zero;
			}
		}

		public static Vector3Int PlaneToOffset(Voxel.View.Shape.OuterShellPlane plane)
		{
			switch (plane)
			{
				case Voxel.View.Shape.OuterShellPlane.PosX:
					return new Vector3Int(1, 0, 0);
				case Voxel.View.Shape.OuterShellPlane.NegX:
					return new Vector3Int(-1, 0, 0);
				case Voxel.View.Shape.OuterShellPlane.PosY:
					return new Vector3Int(0, 1, 0);
				case Voxel.View.Shape.OuterShellPlane.NegY:
					return new Vector3Int(0, -1, 0);
				case Voxel.View.Shape.OuterShellPlane.PosZ:
					return new Vector3Int(0, 0, 1);
				case Voxel.View.Shape.OuterShellPlane.NegZ:
					return new Vector3Int(0, 0, -1);
				default:
					return Vector3Int.zero;
			}
		}

		public static Voxel.View.Shape.OuterShellPlane GetOppositePlane(Voxel.View.Shape.OuterShellPlane plane)
		{
			switch (plane)
			{
				case Voxel.View.Shape.OuterShellPlane.PosX:
					return Voxel.View.Shape.OuterShellPlane.NegX;
				case Voxel.View.Shape.OuterShellPlane.NegX:
					return Voxel.View.Shape.OuterShellPlane.PosX;
				case Voxel.View.Shape.OuterShellPlane.PosY:
					return Voxel.View.Shape.OuterShellPlane.NegY;
				case Voxel.View.Shape.OuterShellPlane.NegY:
					return Voxel.View.Shape.OuterShellPlane.PosY;
				case Voxel.View.Shape.OuterShellPlane.PosZ:
					return Voxel.View.Shape.OuterShellPlane.NegZ;
				case Voxel.View.Shape.OuterShellPlane.NegZ:
					return Voxel.View.Shape.OuterShellPlane.PosZ;
				default:
					return plane;
			}
		}
		public static int OrientationToSteps(Orientation orientation)
		{
			switch (orientation)
			{
				case Orientation.PositiveX:
					return 0;
				case Orientation.PositiveZ:
					return 1;
				case Orientation.NegativeX:
					return 2;
				case Orientation.NegativeZ:
					return 3;
				default:
					return 0;
			}
		}

		public static Voxel.View.Shape.OuterShellPlane MapWorldPlaneToLocal(Voxel.View.Shape.OuterShellPlane plane, Orientation orientation)
		{
			return RotatePlane(plane, -OrientationToSteps(orientation));
		}

		public static Voxel.View.Shape.OuterShellPlane MapWorldPlaneToLocal(Voxel.View.Shape.OuterShellPlane plane, Orientation orientation, FlipOrientation flipOrientation)
		{
			Voxel.View.Shape.OuterShellPlane rotated = RotatePlane(plane, -OrientationToSteps(orientation));
			if (flipOrientation == FlipOrientation.NegativeY)
			{
				return FlipPlaneY(rotated);
			}

			return rotated;
		}

		public static Voxel.View.Shape.OuterShellPlane RotatePlane(Voxel.View.Shape.OuterShellPlane plane, int steps)
		{
			int normalized = ((steps % 4) + 4) % 4;
			if (normalized == 0)
			{
				return plane;
			}

			Vector3 normal = PlaneToNormal(plane);
			Quaternion rotation = Quaternion.AngleAxis(-normalized * 90f, Vector3.up);
			Vector3 rotatedNormal = rotation * normal;
			if (TryFromNormal(rotatedNormal, out Voxel.View.Shape.OuterShellPlane rotatedPlane))
			{
				return rotatedPlane;
			}

			return plane;
		}

		public static Voxel.View.Shape.OuterShellPlane FlipPlaneY(Voxel.View.Shape.OuterShellPlane plane)
		{
			switch (plane)
			{
				case Voxel.View.Shape.OuterShellPlane.PosY:
					return Voxel.View.Shape.OuterShellPlane.NegY;
				case Voxel.View.Shape.OuterShellPlane.NegY:
					return Voxel.View.Shape.OuterShellPlane.PosY;
				default:
					return plane;
			}
		}

		public static Voxel.View.Face TransformFace(Voxel.View.Face face, Orientation orientation, FlipOrientation flipOrientation)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return face;
			}

			int steps = OrientationToSteps(orientation);
			var rotated = new Voxel.View.Face();
			Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);
			List<List<Voxel.View.Face.Vertex>> sourcePolygons = face.Polygons;
			for (int p = 0; p < sourcePolygons.Count; p++)
			{
				List<Voxel.View.Face.Vertex> sourceVertices = sourcePolygons[p];
				if (sourceVertices == null || sourceVertices.Count == 0)
				{
					continue;
				}

				var rotatedPolygon = new List<Voxel.View.Face.Vertex>(sourceVertices.Count);
				for (int i = 0; i < sourceVertices.Count; i++)
				{
					Voxel.View.Face.Vertex vertex = sourceVertices[i];
					Vector3 local = vertex.Position;
					if (steps != 0)
					{
						Vector3 offset = local - pivot;
						Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
						local = rotation * offset + pivot;
					}

					if (flipOrientation == FlipOrientation.NegativeY)
					{
						local.y = 1f - local.y;
					}

					vertex.Position = local;
					rotatedPolygon.Add(vertex);
				}

				if (flipOrientation == FlipOrientation.NegativeY)
				{
					rotatedPolygon.Reverse();
				}

				rotated.Polygons.Add(rotatedPolygon);
			}

			return rotated;
		}

		public static Voxel.View.Face GetFullOuterFace(Voxel.View.Shape.OuterShellPlane plane)
		{
			switch (plane)
			{
				case Voxel.View.Shape.OuterShellPlane.PosX:
					return FullPosXFace;
				case Voxel.View.Shape.OuterShellPlane.NegX:
					return FullNegXFace;
				case Voxel.View.Shape.OuterShellPlane.PosY:
					return FullPosYFace;
				case Voxel.View.Shape.OuterShellPlane.NegY:
					return FullNegYFace;
				case Voxel.View.Shape.OuterShellPlane.PosZ:
					return FullPosZFace;
				case Voxel.View.Shape.OuterShellPlane.NegZ:
					return FullNegZFace;
				default:
					return null;
			}
		}

		public static bool IsFaceCoplanarWithPlane(Voxel.View.Face face, Voxel.View.Shape.OuterShellPlane plane)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return false;
			}

			float target = 0f;
			int axis = 0;
			switch (plane)
			{
				case Voxel.View.Shape.OuterShellPlane.PosX:
					axis = 0;
					target = 1f;
					break;
				case Voxel.View.Shape.OuterShellPlane.NegX:
					axis = 0;
					target = 0f;
					break;
				case Voxel.View.Shape.OuterShellPlane.PosY:
					axis = 1;
					target = 1f;
					break;
				case Voxel.View.Shape.OuterShellPlane.NegY:
					axis = 1;
					target = 0f;
					break;
				case Voxel.View.Shape.OuterShellPlane.PosZ:
					axis = 2;
					target = 1f;
					break;
				case Voxel.View.Shape.OuterShellPlane.NegZ:
					axis = 2;
					target = 0f;
					break;
			}

			List<List<Voxel.View.Face.Vertex>> polygons = face.Polygons;
			for (int p = 0; p < polygons.Count; p++)
			{
				List<Voxel.View.Face.Vertex> polygon = polygons[p];
				if (polygon == null)
				{
					continue;
				}

				for (int i = 0; i < polygon.Count; i++)
				{
					Vector3 pos = polygon[i].Position;
					float value = axis == 0 ? pos.x : axis == 1 ? pos.y : pos.z;
					if (Mathf.Abs(value - target) > Utils.Geometry.PointEpsilon)
					{
						return false;
					}
				}
			}

			return true;
		}

		public static bool IsFullFace(Voxel.View.Face face, Voxel.View.Shape.OuterShellPlane plane)
		{
			if (face == null || !face.HasRenderablePolygons)
			{
				return false;
			}

			Voxel.View.Face fullFace = GetFullOuterFace(plane);
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
}
