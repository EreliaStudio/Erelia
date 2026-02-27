using System.Collections.Generic;
using UnityEngine;

namespace VoxelKit.Utils
{
	public static class Geometry
	{
		public const float NormalEpsilon = 0.001f;
		public const float PointEpsilon = 0.001f;

		private static readonly VoxelKit.Face FullPosXFace = VoxelKit.Utils.Geometry.CreateRectangle(
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = Vector2.zero });
		private static readonly VoxelKit.Face FullNegXFace = VoxelKit.Utils.Geometry.CreateRectangle(
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = Vector2.zero });
		private static readonly VoxelKit.Face FullPosYFace = VoxelKit.Utils.Geometry.CreateRectangle(
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = Vector2.zero });
		private static readonly VoxelKit.Face FullNegYFace = VoxelKit.Utils.Geometry.CreateRectangle(
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = Vector2.zero });
		private static readonly VoxelKit.Face FullPosZFace = VoxelKit.Utils.Geometry.CreateRectangle(
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 1f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 1f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 1f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 1f), UV = Vector2.zero });
		private static readonly VoxelKit.Face FullNegZFace = VoxelKit.Utils.Geometry.CreateRectangle(
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 0f, 0f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 0f, 0f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(1f, 1f, 0f), UV = Vector2.zero },
			new VoxelKit.Utils.Geometry.Vertex { Position = new Vector3(0f, 1f, 0f), UV = Vector2.zero });

		public struct Vertex
		{
			public Vector3 Position;
			public Vector2 UV;
		}

		public static VoxelKit.Face CreateRectangle(Vertex a, Vertex b, Vertex c, Vertex d)
		{
			var face = new VoxelKit.Face();
			face.AddPolygon(CreateRectanglePolygon(a, b, c, d));
			return face;
		}

		public static VoxelKit.Face CreateTriangle(Vertex a, Vertex b, Vertex c)
		{
			var face = new VoxelKit.Face();
			face.AddPolygon(CreateTrianglePolygon(a, b, c));
			return face;
		}

		public static void FlipWindingInPlace(List<int> indices)
		{
			if (indices == null)
			{
				return;
			}

			for (int i = 0; i + 2 < indices.Count; i += 3)
			{
				int temp = indices[i + 1];
				indices[i + 1] = indices[i + 2];
				indices[i + 2] = temp;
			}
		}

		public static List<VoxelKit.Face.Vertex> CreateRectanglePolygon(Vertex a, Vertex b, Vertex c, Vertex d)
		{
			var verts = new List<VoxelKit.Face.Vertex>(4)
		{
			new VoxelKit.Face.Vertex { Position = a.Position, TileUV = a.UV },
			new VoxelKit.Face.Vertex { Position = b.Position, TileUV = b.UV },
			new VoxelKit.Face.Vertex { Position = c.Position, TileUV = c.UV },
			new VoxelKit.Face.Vertex { Position = d.Position, TileUV = d.UV }
		};
			return verts;
		}

		public static List<VoxelKit.Face.Vertex> CreateTrianglePolygon(Vertex a, Vertex b, Vertex c)
		{
			var verts = new List<VoxelKit.Face.Vertex>(3)
		{
			new VoxelKit.Face.Vertex { Position = a.Position, TileUV = a.UV },
			new VoxelKit.Face.Vertex { Position = b.Position, TileUV = b.UV },
			new VoxelKit.Face.Vertex { Position = c.Position, TileUV = c.UV }
		};
			return verts;
		}

		public static Vector3 GetNormal(List<VoxelKit.Face.Vertex> verts)
		{
			Vector3 a = verts[0].Position;
			Vector3 b = verts[1].Position;
			Vector3 c = verts[2].Position;
			return Vector3.Cross(b - a, c - a);
		}

		public static bool IsPolygonContained(List<VoxelKit.Face.Vertex> polygon, List<VoxelKit.Face.Vertex> container, Vector3 normal)
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

		public static bool IsPolygonContainedInUnion(List<VoxelKit.Face.Vertex> polygon, List<List<VoxelKit.Face.Vertex>> containers, Vector3 normal)
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
				List<VoxelKit.Face.Vertex> container = containers[i];
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

		public static float SignedArea(List<Vector2> polygon)
		{
			float area = 0f;
			for (int i = 0; i < polygon.Count; i++)
			{
				Vector2 a = polygon[i];
				Vector2 b = polygon[(i + 1) % polygon.Count];
				area += a.x * b.y - b.x * a.y;
			}
			return area * 0.5f;
		}

		public static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
		{
			return Cross(b - a, c - b) > 0f;
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

		public static bool ContainsPointInTriangle(List<Vector2> polygon, List<int> indices, int aIndex, int bIndex, int cIndex)
		{
			Vector2 a = polygon[aIndex];
			Vector2 b = polygon[bIndex];
			Vector2 c = polygon[cIndex];
			for (int i = 0; i < indices.Count; i++)
			{
				int idx = indices[i];
				if (idx == aIndex || idx == bIndex || idx == cIndex)
				{
					continue;
				}

				if (IsPointInTriangle(polygon[idx], a, b, c))
				{
					return true;
				}
			}

			return false;
		}

		public static void RemoveCollinear(List<Vector2> poly2D, List<Vector3> poly3D)
		{
			int i = 0;
			while (i < poly2D.Count && poly2D.Count >= 3)
			{
				int prev = (i - 1 + poly2D.Count) % poly2D.Count;
				int next = (i + 1) % poly2D.Count;
				Vector2 a = poly2D[prev];
				Vector2 b = poly2D[i];
				Vector2 c = poly2D[next];
				if (Mathf.Abs(Cross(b - a, c - b)) < 0.0001f)
				{
					poly2D.RemoveAt(i);
					poly3D.RemoveAt(i);
					if (i > 0)
					{
						i--;
					}
					continue;
				}
				i++;
			}
		}

		public static bool TryFromNormal(Vector3 normal, out VoxelKit.Shape.AxisPlane plane)
		{
			if (normal.sqrMagnitude < NormalEpsilon)
			{
				plane = VoxelKit.Shape.AxisPlane.PosX;
				return false;
			}

			Vector3 n = normal.normalized;
			float ax = Mathf.Abs(n.x);
			float ay = Mathf.Abs(n.y);
			float az = Mathf.Abs(n.z);

			if (ax >= 1f - NormalEpsilon && ay <= NormalEpsilon && az <= NormalEpsilon)
			{
				plane = n.x >= 0f ? VoxelKit.Shape.AxisPlane.PosX : VoxelKit.Shape.AxisPlane.NegX;
				return true;
			}
			if (ay >= 1f - NormalEpsilon && ax <= NormalEpsilon && az <= NormalEpsilon)
			{
				plane = n.y >= 0f ? VoxelKit.Shape.AxisPlane.PosY : VoxelKit.Shape.AxisPlane.NegY;
				return true;
			}
			if (az >= 1f - NormalEpsilon && ax <= NormalEpsilon && ay <= NormalEpsilon)
			{
				plane = n.z >= 0f ? VoxelKit.Shape.AxisPlane.PosZ : VoxelKit.Shape.AxisPlane.NegZ;
				return true;
			}

			plane = VoxelKit.Shape.AxisPlane.PosX;
			return false;
		}

		public static Vector3 PlaneToNormal(VoxelKit.Shape.AxisPlane plane)
		{
			switch (plane)
			{
				case VoxelKit.Shape.AxisPlane.PosX:
					return Vector3.right;
				case VoxelKit.Shape.AxisPlane.NegX:
					return Vector3.left;
				case VoxelKit.Shape.AxisPlane.PosY:
					return Vector3.up;
				case VoxelKit.Shape.AxisPlane.NegY:
					return Vector3.down;
				case VoxelKit.Shape.AxisPlane.PosZ:
					return Vector3.forward;
				case VoxelKit.Shape.AxisPlane.NegZ:
					return Vector3.back;
				default:
					return Vector3.zero;
			}
		}

		public static Vector3Int PlaneToOffset(VoxelKit.Shape.AxisPlane plane)
		{
			switch (plane)
			{
				case VoxelKit.Shape.AxisPlane.PosX:
					return new Vector3Int(1, 0, 0);
				case VoxelKit.Shape.AxisPlane.NegX:
					return new Vector3Int(-1, 0, 0);
				case VoxelKit.Shape.AxisPlane.PosY:
					return new Vector3Int(0, 1, 0);
				case VoxelKit.Shape.AxisPlane.NegY:
					return new Vector3Int(0, -1, 0);
				case VoxelKit.Shape.AxisPlane.PosZ:
					return new Vector3Int(0, 0, 1);
				case VoxelKit.Shape.AxisPlane.NegZ:
					return new Vector3Int(0, 0, -1);
				default:
					return Vector3Int.zero;
			}
		}

		public static VoxelKit.Shape.AxisPlane GetOppositePlane(VoxelKit.Shape.AxisPlane plane)
		{
			switch (plane)
			{
				case VoxelKit.Shape.AxisPlane.PosX:
					return VoxelKit.Shape.AxisPlane.NegX;
				case VoxelKit.Shape.AxisPlane.NegX:
					return VoxelKit.Shape.AxisPlane.PosX;
				case VoxelKit.Shape.AxisPlane.PosY:
					return VoxelKit.Shape.AxisPlane.NegY;
				case VoxelKit.Shape.AxisPlane.NegY:
					return VoxelKit.Shape.AxisPlane.PosY;
				case VoxelKit.Shape.AxisPlane.PosZ:
					return VoxelKit.Shape.AxisPlane.NegZ;
				case VoxelKit.Shape.AxisPlane.NegZ:
					return VoxelKit.Shape.AxisPlane.PosZ;
				default:
					return plane;
			}
		}
		public static int OrientationToSteps(VoxelKit.Orientation orientation)
		{
			switch (orientation)
			{
				case VoxelKit.Orientation.PositiveX:
					return 0;
				case VoxelKit.Orientation.PositiveZ:
					return 1;
				case VoxelKit.Orientation.NegativeX:
					return 2;
				case VoxelKit.Orientation.NegativeZ:
					return 3;
				default:
					return 0;
			}
		}

		public static VoxelKit.Shape.AxisPlane MapWorldPlaneToLocal(VoxelKit.Shape.AxisPlane plane, VoxelKit.Orientation orientation)
		{
			return RotatePlane(plane, -OrientationToSteps(orientation));
		}

		public static VoxelKit.Shape.AxisPlane MapWorldPlaneToLocal(VoxelKit.Shape.AxisPlane plane, VoxelKit.Orientation orientation, VoxelKit.FlipOrientation flipOrientation)
		{
			VoxelKit.Shape.AxisPlane rotated = RotatePlane(plane, -OrientationToSteps(orientation));
			if (flipOrientation == VoxelKit.FlipOrientation.NegativeY)
			{
				return FlipPlaneY(rotated);
			}

			return rotated;
		}

		public static VoxelKit.Shape.AxisPlane RotatePlane(VoxelKit.Shape.AxisPlane plane, int steps)
		{
			int normalized = ((steps % 4) + 4) % 4;
			if (normalized == 0)
			{
				return plane;
			}

			Vector3 normal = PlaneToNormal(plane);
			Quaternion rotation = Quaternion.AngleAxis(-normalized * 90f, Vector3.up);
			Vector3 rotatedNormal = rotation * normal;
			if (TryFromNormal(rotatedNormal, out VoxelKit.Shape.AxisPlane rotatedPlane))
			{
				return rotatedPlane;
			}

			return plane;
		}

		public static VoxelKit.Shape.AxisPlane FlipPlaneY(VoxelKit.Shape.AxisPlane plane)
		{
			switch (plane)
			{
				case VoxelKit.Shape.AxisPlane.PosY:
					return VoxelKit.Shape.AxisPlane.NegY;
				case VoxelKit.Shape.AxisPlane.NegY:
					return VoxelKit.Shape.AxisPlane.PosY;
				default:
					return plane;
			}
		}

		public static VoxelKit.Face TransformFace(VoxelKit.Face face, VoxelKit.Orientation orientation, VoxelKit.FlipOrientation flipOrientation)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return face;
			}

			int steps = OrientationToSteps(orientation);
			var rotated = new VoxelKit.Face();
			Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);
			List<List<VoxelKit.Face.Vertex>> sourcePolygons = face.Polygons;
			for (int p = 0; p < sourcePolygons.Count; p++)
			{
				List<VoxelKit.Face.Vertex> sourceVertices = sourcePolygons[p];
				if (sourceVertices == null || sourceVertices.Count == 0)
				{
					continue;
				}

				var rotatedPolygon = new List<VoxelKit.Face.Vertex>(sourceVertices.Count);
				for (int i = 0; i < sourceVertices.Count; i++)
				{
					VoxelKit.Face.Vertex vertex = sourceVertices[i];
					Vector3 local = vertex.Position;
					if (steps != 0)
					{
						Vector3 offset = local - pivot;
						Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
						local = rotation * offset + pivot;
					}

					if (flipOrientation == VoxelKit.FlipOrientation.NegativeY)
					{
						local.y = 1f - local.y;
					}

					vertex.Position = local;
					rotatedPolygon.Add(vertex);
				}

				if (flipOrientation == VoxelKit.FlipOrientation.NegativeY)
				{
					rotatedPolygon.Reverse();
				}

				rotated.Polygons.Add(rotatedPolygon);
			}

			return rotated;
		}

		public static Vector3 TransformPoint(Vector3 point, VoxelKit.Orientation orientation, VoxelKit.FlipOrientation flipOrientation)
		{
			int steps = OrientationToSteps(orientation);
			Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);
			Vector3 local = point;
			if (steps != 0)
			{
				Vector3 offset = local - pivot;
				Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
				local = rotation * offset + pivot;
			}

			if (flipOrientation == VoxelKit.FlipOrientation.NegativeY)
			{
				local.y = 1f - local.y;
			}

			return local;
		}

		public static VoxelKit.Face GetFullOuterFace(VoxelKit.Shape.AxisPlane plane)
		{
			switch (plane)
			{
				case VoxelKit.Shape.AxisPlane.PosX:
					return FullPosXFace;
				case VoxelKit.Shape.AxisPlane.NegX:
					return FullNegXFace;
				case VoxelKit.Shape.AxisPlane.PosY:
					return FullPosYFace;
				case VoxelKit.Shape.AxisPlane.NegY:
					return FullNegYFace;
				case VoxelKit.Shape.AxisPlane.PosZ:
					return FullPosZFace;
				case VoxelKit.Shape.AxisPlane.NegZ:
					return FullNegZFace;
				default:
					return null;
			}
		}

		public static bool IsFaceCoplanarWithPlane(VoxelKit.Face face, VoxelKit.Shape.AxisPlane plane)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return false;
			}

			float target = 0f;
			int axis = 0;
			switch (plane)
			{
				case VoxelKit.Shape.AxisPlane.PosX:
					axis = 0;
					target = 1f;
					break;
				case VoxelKit.Shape.AxisPlane.NegX:
					axis = 0;
					target = 0f;
					break;
				case VoxelKit.Shape.AxisPlane.PosY:
					axis = 1;
					target = 1f;
					break;
				case VoxelKit.Shape.AxisPlane.NegY:
					axis = 1;
					target = 0f;
					break;
				case VoxelKit.Shape.AxisPlane.PosZ:
					axis = 2;
					target = 1f;
					break;
				case VoxelKit.Shape.AxisPlane.NegZ:
					axis = 2;
					target = 0f;
					break;
			}

			List<List<VoxelKit.Face.Vertex>> polygons = face.Polygons;
			for (int p = 0; p < polygons.Count; p++)
			{
				List<VoxelKit.Face.Vertex> polygon = polygons[p];
				if (polygon == null)
				{
					continue;
				}

				for (int i = 0; i < polygon.Count; i++)
				{
					Vector3 pos = polygon[i].Position;
					float value = axis == 0 ? pos.x : axis == 1 ? pos.y : pos.z;
					if (Mathf.Abs(value - target) > VoxelKit.Utils.Geometry.PointEpsilon)
					{
						return false;
					}
				}
			}

			return true;
		}

		public static bool IsFullFace(VoxelKit.Face face, VoxelKit.Shape.AxisPlane plane)
		{
			if (face == null || !face.HasRenderablePolygons)
			{
				return false;
			}

			VoxelKit.Face fullFace = GetFullOuterFace(plane);
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



