using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.VoxelKit.Utils
{
	/// <summary>
	/// Collection of geometry helper methods used by VoxelKit for face construction,
	/// polygon tests (containment / point-in-polygon), and orientation/plane transforms.
	/// </summary>
	public static class Geometry
	{
		/// <summary>
		/// Epsilon used when validating normals (e.g., rejecting near-zero normals).
		/// </summary>
		public const float NormalEpsilon = 0.001f;

		/// <summary>
		/// Epsilon used for point comparisons (e.g., checking coplanarity against axiZs planes).
		/// </summary>
		public const float PointEpsilon = 0.001f;

		// Precomputed unit-cube outer faces for each axis plane. These are used for fast checks
		// such as "is this face exactly a full outer face of a voxel?".
		public static readonly Erelia.Core.VoxelKit.Face[] FullOuterFaces =
		{
			// PosX
			Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f, 0f), TileUV = Vector2.zero }),

			// NegX
			Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = Vector2.zero }),

			// PosY
			Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = Vector2.zero }),

			// NegY
			Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = Vector2.zero }),

			// PosZ
			Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f, 1f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 1f), TileUV = Vector2.zero }),

			// NegZ
			Erelia.Core.VoxelKit.Utils.Geometry.CreateRectangle(
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 0f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(1f, 1f, 0f), TileUV = Vector2.zero },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = new Vector3(0f, 1f, 0f), TileUV = Vector2.zero }),
		};

		/// <summary>
		/// Creates a <see cref="Erelia.Core.VoxelKit.Face"/> containing a single rectangular polygon.
		/// </summary>
		/// <param name="a">First corner <see cref="Erelia.Core.VoxelKit.Face.Vertex"/>.</param>
		/// <param name="b">Second corner <see cref="Erelia.Core.VoxelKit.Face.Vertex"/>.</param>
		/// <param name="c">Third corner <see cref="Erelia.Core.VoxelKit.Face.Vertex"/>.</param>
		/// <param name="d">Fourth corner <see cref="Erelia.Core.VoxelKit.Face.Vertex"/>.</param>
		/// <returns>A face holding a single polygon (a quad) defined by the 4 given vertices.</returns>
		public static Erelia.Core.VoxelKit.Face CreateRectangle(
			Erelia.Core.VoxelKit.Face.Vertex a,
			Erelia.Core.VoxelKit.Face.Vertex b,
			Erelia.Core.VoxelKit.Face.Vertex c,
			Erelia.Core.VoxelKit.Face.Vertex d)
		{
			// Create an empty face and add one polygon representing the rectangle.
			var face = new Erelia.Core.VoxelKit.Face();

			// Create the list of vertices
			var verts = new List<Erelia.Core.VoxelKit.Face.Vertex>(4)
			{
				new Erelia.Core.VoxelKit.Face.Vertex { Position = a.Position, TileUV = a.TileUV },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = b.Position, TileUV = b.TileUV },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = c.Position, TileUV = c.TileUV },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = d.Position, TileUV = d.TileUV }
			};

			// Add the list of vertices inside the face
			face.AddPolygon(verts);
			return face;
		}

		/// <summary>
		/// Creates a <see cref="Erelia.Core.VoxelKit.Face"/> containing a single triangle polygon.
		/// </summary>
		/// <param name="a">First triangle <see cref="Erelia.Core.VoxelKit.Face.Vertex"/>.</param>
		/// <param name="b">Second triangle <see cref="Erelia.Core.VoxelKit.Face.Vertex"/>.</param>
		/// <param name="c">Third triangle <see cref="Erelia.Core.VoxelKit.Face.Vertex"/>.</param>
		/// <returns>A face holding a single triangle polygon.</returns>
		public static Erelia.Core.VoxelKit.Face CreateTriangle(
			Erelia.Core.VoxelKit.Face.Vertex a,
			Erelia.Core.VoxelKit.Face.Vertex b,
			Erelia.Core.VoxelKit.Face.Vertex c)
		{
			// Create an empty face and add one polygon representing the triangle.
			var face = new Erelia.Core.VoxelKit.Face();

			// Create the list of vertices
			var verts = new List<Erelia.Core.VoxelKit.Face.Vertex>(3)
			{
				new Erelia.Core.VoxelKit.Face.Vertex { Position = a.Position, TileUV = a.TileUV },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = b.Position, TileUV = b.TileUV },
				new Erelia.Core.VoxelKit.Face.Vertex { Position = c.Position, TileUV = c.TileUV }
			};

			// Add the list of vertices inside the face
			face.AddPolygon(verts);
			return face;
		}

		/// <summary>
		/// Computes the (non-normalized) polygon normal using the first 3 vertices.
		/// </summary>
		/// <param name="verts">Polygon vertices (expects at least 3 vertices).</param>
		/// <returns>The non-normalized normal vector computed by a cross product.</returns>
		/// <remarks>
		/// The result magnitude is proportional to the triangle area.
		/// </remarks>
		public static Vector3 GetNormal(List<Erelia.Core.VoxelKit.Face.Vertex> verts)
		{
			// Use the first 3 vertices as a triangle to compute the normal.
			Vector3 a = verts[0].Position;
			Vector3 b = verts[1].Position;
			Vector3 c = verts[2].Position;

			// Cross product gives a vector perpendicular to the triangle plane.
			return Vector3.Cross(b - a, c - a);
		}

		/// <summary>
		/// Tests whether every vertex of <paramref name="polygon"/> lies inside <paramref name="container"/>
		/// when both are projected onto a 2D basis built from <paramref name="normal"/>.
		/// </summary>
		/// <param name="polygon">The polygon to test (must contain at least 3 vertices).</param>
		/// <param name="container">The containing polygon (must contain at least 3 vertices).</param>
		/// <param name="normal">A normal defining the plane to project onto (used to build tangent/bitangent).</param>
		/// <returns><c>true</c> if all vertices of <paramref name="polygon"/> are inside the container; otherwise <c>false</c>.</returns>
		public static bool IsPolygonContained(
			List<Erelia.Core.VoxelKit.Face.Vertex> polygon,
			List<Erelia.Core.VoxelKit.Face.Vertex> container,
			Vector3 normal)
		{
			// Reject invalid inputs.
			if (polygon == null || container == null || polygon.Count < 3 || container.Count < 3)
			{
				return false;
			}

			// Build a stable orthonormal basis (tangent/bitangent) for 3D->2D projection.
			if (!TryBuildBasis(normal, out Vector3 tangent, out Vector3 bitangent))
			{
				return false;
			}

			// Project both polygons into 2D coordinates in the plane basis.
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

			// Check whether each vertex of the tested polygon is inside the container polygon.
			for (int i = 0; i < poly2D.Count; i++)
			{
				if (!IsPointInPolygon(poly2D[i], container2D))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Tests whether a polygon is contained within the union of multiple container polygons,
		/// after projecting onto a 2D basis derived from <paramref name="normal"/>.
		/// </summary>
		/// <param name="polygon">The polygon to test (must contain at least 3 vertices).</param>
		/// <param name="containers">A list of container polygons.</param>
		/// <param name="normal">A normal defining the projection plane (used to build tangent/bitangent).</param>
		/// <returns>
		/// <c>true</c> if a set of sample points on <paramref name="polygon"/> are contained in the union;
		/// otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// This uses sampling (centroid, vertices, and edge midpoints) rather than a full polygon union operation.
		/// </remarks>
		public static bool IsPolygonContainedInUnion(
			List<Erelia.Core.VoxelKit.Face.Vertex> polygon,
			List<List<Erelia.Core.VoxelKit.Face.Vertex>> containers,
			Vector3 normal)
		{
			// Reject invalid inputs.
			if (polygon == null || polygon.Count < 3 || containers == null)
			{
				return false;
			}

			// Build projection basis.
			if (!TryBuildBasis(normal, out Vector3 tangent, out Vector3 bitangent))
			{
				return false;
			}

			// Project the tested polygon into 2D.
			var poly2D = new List<Vector2>(polygon.Count);
			for (int i = 0; i < polygon.Count; i++)
			{
				Vector3 p = polygon[i].Position;
				poly2D.Add(new Vector2(Vector3.Dot(p, tangent), Vector3.Dot(p, bitangent)));
			}

			// Project each valid container polygon into 2D.
			var container2Ds = new List<List<Vector2>>(containers.Count);
			for (int i = 0; i < containers.Count; i++)
			{
				List<Erelia.Core.VoxelKit.Face.Vertex> container = containers[i];
				if (container == null || container.Count < 3)
				{
					// Skip degenerate/missing containers.
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

			// If no valid container exists, containment is impossible.
			if (container2Ds.Count == 0)
			{
				return false;
			}

			// Validate containment using a set of sample points (centroid + vertices + midpoints).
			if (!AreSamplePointsContained(poly2D, container2Ds))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks whether representative sample points from a polygon are inside a union of polygons.
		/// </summary>
		/// <param name="polygon">The polygon to sample (2D).</param>
		/// <param name="containers">Container polygons (2D) forming a union.</param>
		/// <returns><c>true</c> if all tested sample points are inside the union; otherwise <c>false</c>.</returns>
		private static bool AreSamplePointsContained(List<Vector2> polygon, List<List<Vector2>> containers)
		{
			// Reject invalid polygon.
			if (polygon == null || polygon.Count < 3)
			{
				return false;
			}

			// Compute centroid as the average of vertices.
			Vector2 centroid = Vector2.zero;
			for (int i = 0; i < polygon.Count; i++)
			{
				centroid += polygon[i];
			}
			centroid /= polygon.Count;

			// First, ensure the centroid is inside the union.
			if (!IsPointInUnion(centroid, containers))
			{
				return false;
			}

			// Then test each vertex and each edge midpoint.
			for (int i = 0; i < polygon.Count; i++)
			{
				Vector2 a = polygon[i];
				Vector2 b = polygon[(i + 1) % polygon.Count];
				Vector2 mid = (a + b) * 0.5f;

				// A stricter check: vertex and midpoint must both be inside the union.
				if (!IsPointInUnion(a, containers) || !IsPointInUnion(mid, containers))
				{
					return false;
				}

				// Todo : Add verification for the intersection of the edges, not just the mid point
			}

			return true;
		}

		/// <summary>
		/// Tests whether a point lies inside at least one polygon in a set (union membership).
		/// </summary>
		/// <param name="point">Point to test.</param>
		/// <param name="polygons">Polygons defining the union.</param>
		/// <returns><c>true</c> if the point is inside any polygon; otherwise <c>false</c>.</returns>
		private static bool IsPointInUnion(Vector2 point, List<List<Vector2>> polygons)
		{
			// If the point is inside any polygon, it is inside the union.
			for (int i = 0; i < polygons.Count; i++)
			{
				if (IsPointInPolygon(point, polygons[i]))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Attempts to build an orthonormal basis (tangent/bitangent) for a plane defined by a normal.
		/// </summary>
		/// <param name="normal">Plane normal. Must not be near zero.</param>
		/// <param name="tangent">Output tangent vector on the plane.</param>
		/// <param name="bitangent">Output bitangent vector on the plane.</param>
		/// <returns><c>true</c> if the basis could be built; otherwise <c>false</c>.</returns>
		public static bool TryBuildBasis(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
		{
			// Reject near-zero normals (no stable plane).
			if (normal.sqrMagnitude < NormalEpsilon)
			{
				tangent = Vector3.zero;
				bitangent = Vector3.zero;
				return false;
			}

			// Normalize the normal for a stable basis.
			Vector3 n = normal.normalized;

			// Choose an "up" vector that is not nearly parallel to the normal, to avoid degeneracy.
			Vector3 up = Mathf.Abs(n.y) < 0.99f ? Vector3.up : Vector3.right;

			// Tangent is perpendicular to both up and normal.
			tangent = Vector3.Cross(up, n).normalized;

			// Bitangent completes the right-handed basis on the plane.
			bitangent = Vector3.Cross(n, tangent);

			return true;
		}

		/// <summary>
		/// Determines whether a 2D point lies inside a polygon using a ray casting test.
		/// </summary>
		/// <param name="point">Point to test.</param>
		/// <param name="polygon">Polygon vertices in 2D (expects at least 3 points).</param>
		/// <returns><c>true</c> if the point is inside or on the boundary; otherwise <c>false</c>.</returns>
		private static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
		{
			bool inside = false;

			// Iterate edges (pj -> pi). j tracks the previous vertex index.
			for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
			{
				Vector2 pi = polygon[i];
				Vector2 pj = polygon[j];

				// If the point lies exactly on an edge, treat it as inside.
				if (IsPointOnSegment(point, pj, pi))
				{
					return true;
				}

				// Ray casting: toggle "inside" when crossing polygon edges.
				bool intersect = (pi.y > point.y) != (pj.y > point.y)
					&& point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y + 0.000001f) + pi.x;

				if (intersect)
				{
					inside = !inside;
				}
			}

			return inside;
		}

		/// <summary>
		/// Tests whether a point lies on the segment [a,b] within a small tolerance.
		/// </summary>
		/// <param name="point">Point to test.</param>
		/// <param name="a">Segment start.</param>
		/// <param name="b">Segment end.</param>
		/// <returns><c>true</c> if the point is on the segment; otherwise <c>false</c>.</returns>
		private static bool IsPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
		{
			// Cross product magnitude indicates distance from the infinite line (a->b).
			float cross = (point.y - a.y) * (b.x - a.x) - (point.x - a.x) * (b.y - a.y);
			if (Mathf.Abs(cross) > 0.0001f)
			{
				return false;
			}

			// Dot product checks whether the projection lies between endpoints.
			float dot = (point.x - a.x) * (b.x - a.x) + (point.y - a.y) * (b.y - a.y);
			if (dot < -0.0001f)
			{
				return false;
			}

			// Ensure dot does not exceed the squared length (with tolerance).
			float lenSq = (b - a).sqrMagnitude;
			return dot <= lenSq + 0.0001f;
		}

		/// <summary>
		/// Computes the 2D cross product (scalar) of two vectors.
		/// </summary>
		/// <param name="a">First vector.</param>
		/// <param name="b">Second vector.</param>
		/// <returns>The scalar cross product a.x*b.y - a.y*b.x.</returns>
		public static float Cross(Vector2 a, Vector2 b)
		{
			return a.x * b.y - a.y * b.x;
		}

		/// <summary>
		/// Tests whether a point lies inside (or on the boundary of) a triangle.
		/// </summary>
		/// <param name="p">Point to test.</param>
		/// <param name="a">Triangle vertex A.</param>
		/// <param name="b">Triangle vertex B.</param>
		/// <param name="c">Triangle vertex C.</param>
		/// <returns><c>true</c> if the point is inside or on the boundary; otherwise <c>false</c>.</returns>
		/// <remarks>
		/// The sign checks are adjusted depending on the triangle winding.
		/// </remarks>
		public static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
		{
			// Compute reference area sign (winding).
			float area = Cross(b - a, c - a);

			// Compute signed sub-areas / edge-side tests.
			float area1 = Cross(b - a, p - a);
			float area2 = Cross(c - b, p - b);
			float area3 = Cross(a - c, p - c);

			// If triangle is CCW, inside means all have same sign (or zero).
			bool hasNeg = (area1 < 0f) || (area2 < 0f) || (area3 < 0f);
			bool hasPos = (area1 > 0f) || (area2 > 0f) || (area3 > 0f);

			// If triangle is CW, invert the sign logic.
			if (area < 0f)
			{
				hasNeg = (area1 > 0f) || (area2 > 0f) || (area3 > 0f);
				hasPos = (area1 < 0f) || (area2 < 0f) || (area3 < 0f);
			}

			// Inside if not on both sides.
			return !(hasNeg && hasPos);
		}

		/// <summary>
		/// Attempts to map a normal vector to one of the six axis-aligned planes.
		/// </summary>
		/// <param name="normal">Normal vector (does not need to be normalized).</param>
		/// <param name="plane">Output axis plane if the normal is axis-aligned within tolerance.</param>
		/// <returns><c>true</c> if the normal matches an axis plane; otherwise <c>false</c>.</returns>
		public static bool TryFromNormal(Vector3 normal, out Erelia.Core.VoxelKit.AxisPlane plane)
		{
			// Reject degenerate normals.
			if (normal.sqrMagnitude < NormalEpsilon)
			{
				plane = Erelia.Core.VoxelKit.AxisPlane.PosX;
				return false;
			}

			// Normalize to compare components reliably.
			Vector3 n = normal.normalized;

			float ax = Mathf.Abs(n.x);
			float ay = Mathf.Abs(n.y);
			float az = Mathf.Abs(n.z);

			// Match X axis.
			if (ax >= 1f - NormalEpsilon && ay <= NormalEpsilon && az <= NormalEpsilon)
			{
				plane = n.x >= 0f ? Erelia.Core.VoxelKit.AxisPlane.PosX : Erelia.Core.VoxelKit.AxisPlane.NegX;
				return true;
			}

			// Match Y axis.
			if (ay >= 1f - NormalEpsilon && ax <= NormalEpsilon && az <= NormalEpsilon)
			{
				plane = n.y >= 0f ? Erelia.Core.VoxelKit.AxisPlane.PosY : Erelia.Core.VoxelKit.AxisPlane.NegY;
				return true;
			}

			// Match Z axis.
			if (az >= 1f - NormalEpsilon && ax <= NormalEpsilon && ay <= NormalEpsilon)
			{
				plane = n.z >= 0f ? Erelia.Core.VoxelKit.AxisPlane.PosZ : Erelia.Core.VoxelKit.AxisPlane.NegZ;
				return true;
			}

			// Not axis-aligned within tolerance.
			plane = Erelia.Core.VoxelKit.AxisPlane.PosX;
			return false;
		}

		/// <summary>
		/// Converts an axis plane to its outward unit normal.
		/// </summary>
		/// <param name="plane">Plane to convert.</param>
		/// <returns>Unit normal for that plane.</returns>
		public static Vector3 PlaneToNormal(Erelia.Core.VoxelKit.AxisPlane plane)
		{
			switch (plane)
			{
				case Erelia.Core.VoxelKit.AxisPlane.PosX:
					return Vector3.right;
				case Erelia.Core.VoxelKit.AxisPlane.NegX:
					return Vector3.left;
				case Erelia.Core.VoxelKit.AxisPlane.PosY:
					return Vector3.up;
				case Erelia.Core.VoxelKit.AxisPlane.NegY:
					return Vector3.down;
				case Erelia.Core.VoxelKit.AxisPlane.PosZ:
					return Vector3.forward;
				case Erelia.Core.VoxelKit.AxisPlane.NegZ:
					return Vector3.back;
				default:
					return Vector3.zero;
			}
		}

		/// <summary>
		/// Converts an axis plane to a unit grid offset (Vector3Int) in that direction.
		/// This allow us to convert an axis plane into a coordinate offset, corresponding to the plane origin position.
		/// </summary>
		/// <param name="plane">Plane to convert.</param>
		/// <returns>Grid offset corresponding to the plane direction.</returns>
		public static Vector3Int PlaneToOffset(Erelia.Core.VoxelKit.AxisPlane plane)
		{
			switch (plane)
			{
				case Erelia.Core.VoxelKit.AxisPlane.PosX:
					return new Vector3Int(1, 0, 0);
				case Erelia.Core.VoxelKit.AxisPlane.NegX:
					return new Vector3Int(-1, 0, 0);
				case Erelia.Core.VoxelKit.AxisPlane.PosY:
					return new Vector3Int(0, 1, 0);
				case Erelia.Core.VoxelKit.AxisPlane.NegY:
					return new Vector3Int(0, -1, 0);
				case Erelia.Core.VoxelKit.AxisPlane.PosZ:
					return new Vector3Int(0, 0, 1);
				case Erelia.Core.VoxelKit.AxisPlane.NegZ:
					return new Vector3Int(0, 0, -1);
				default:
					return Vector3Int.zero;
			}
		}

		/// <summary>
		/// Returns the opposite axis plane.
		/// </summary>
		/// <param name="plane">Input plane.</param>
		/// <returns>The opposite plane (e.g., PosX -> NegX).</returns>
		public static Erelia.Core.VoxelKit.AxisPlane GetOppositePlane(Erelia.Core.VoxelKit.AxisPlane plane)
		{
			switch (plane)
			{
				case Erelia.Core.VoxelKit.AxisPlane.PosX:
					return Erelia.Core.VoxelKit.AxisPlane.NegX;
				case Erelia.Core.VoxelKit.AxisPlane.NegX:
					return Erelia.Core.VoxelKit.AxisPlane.PosX;
				case Erelia.Core.VoxelKit.AxisPlane.PosY:
					return Erelia.Core.VoxelKit.AxisPlane.NegY;
				case Erelia.Core.VoxelKit.AxisPlane.NegY:
					return Erelia.Core.VoxelKit.AxisPlane.PosY;
				case Erelia.Core.VoxelKit.AxisPlane.PosZ:
					return Erelia.Core.VoxelKit.AxisPlane.NegZ;
				case Erelia.Core.VoxelKit.AxisPlane.NegZ:
					return Erelia.Core.VoxelKit.AxisPlane.PosZ;
				default:
					return plane;
			}
		}

		/// <summary>
		/// Maps a plane expressed in world-space into local-space by applying the inverse orientation rotation.
		/// </summary>
		/// <param name="plane">World plane to map.</param>
		/// <param name="orientation">Local orientation.</param>
		/// <returns>The corresponding local plane.</returns>
		public static Erelia.Core.VoxelKit.AxisPlane MapWorldPlaneToLocal(
			Erelia.Core.VoxelKit.AxisPlane plane,
			Erelia.Core.VoxelKit.Orientation orientation)
		{
			// Inverse mapping: rotate by negative steps.
			return RotatePlane(plane, -(int)orientation);
		}

		/// <summary>
		/// Maps a plane expressed in world-space into local-space by applying the inverse orientation rotation
		/// and an optional Y flip.
		/// </summary>
		/// <param name="plane">World plane to map.</param>
		/// <param name="orientation">Local orientation.</param>
		/// <param name="flipOrientation">Optional flip (typically mirroring along Y).</param>
		/// <returns>The corresponding local plane.</returns>
		public static Erelia.Core.VoxelKit.AxisPlane MapWorldPlaneToLocal(
			Erelia.Core.VoxelKit.AxisPlane plane,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			// Apply inverse rotation first.
			Erelia.Core.VoxelKit.AxisPlane rotated = RotatePlane(plane, -(int)orientation);

			// If the local space is flipped along Y, swap PosY/NegY.
			if (flipOrientation == Erelia.Core.VoxelKit.FlipOrientation.NegativeY)
			{
				return FlipPlaneY(rotated);
			}

			return rotated;
		}

		/// <summary>
		/// Rotates an axis plane around the Y axis by multiples of 90 degrees.
		/// </summary>
		/// <param name="plane">Plane to rotate.</param>
		/// <param name="steps">
		/// Number of 90° steps. Values are normalized modulo 4 (negative allowed).
		/// </param>
		/// <returns>The rotated plane if it remains axis-aligned; otherwise the original plane.</returns>
		public static Erelia.Core.VoxelKit.AxisPlane RotatePlane(Erelia.Core.VoxelKit.AxisPlane plane, int steps)
		{
			// Normalize steps to 0..3.
			int normalized = ((steps % 4) + 4) % 4;
			if (normalized == 0)
			{
				return plane;
			}

			// Rotate the plane normal around Y and try to map it back to a discrete axis plane.
			Vector3 normal = PlaneToNormal(plane);
			Quaternion rotation = Quaternion.AngleAxis(-normalized * 90f, Vector3.up);
			Vector3 rotatedNormal = rotation * normal;

			if (TryFromNormal(rotatedNormal, out Erelia.Core.VoxelKit.AxisPlane rotatedPlane))
			{
				return rotatedPlane;
			}

			// Fallback if numerical imprecision prevents mapping.
			return plane;
		}

		/// <summary>
		/// Flips an axis plane along Y (PosY &lt;-&gt; NegY), leaving other planes unchanged.
		/// </summary>
		/// <param name="plane">Plane to flip.</param>
		/// <returns>Flipped plane.</returns>
		public static Erelia.Core.VoxelKit.AxisPlane FlipPlaneY(Erelia.Core.VoxelKit.AxisPlane plane)
		{
			switch (plane)
			{
				case Erelia.Core.VoxelKit.AxisPlane.PosY:
					return Erelia.Core.VoxelKit.AxisPlane.NegY;
				case Erelia.Core.VoxelKit.AxisPlane.NegY:
					return Erelia.Core.VoxelKit.AxisPlane.PosY;
				default:
					return plane;
			}
		}

		/// <summary>
		/// Transforms a face from local space by applying a cardinal Y rotation and an optional Y flip.
		/// </summary>
		/// <param name="face">Face to transform.</param>
		/// <param name="orientation">Rotation around Y (in 90° steps).</param>
		/// <param name="flipOrientation">Optional flip (mirroring along Y).</param>
		/// <returns>A new face containing transformed polygons. Returns the input face if it is empty.</returns>
		/// <remarks>
		/// Positions are rotated around the center of the unit cube (0.5,0.5,0.5).
		/// When flipping along Y, winding is reversed to keep consistent face orientation.
		/// </remarks>
		public static Erelia.Core.VoxelKit.Face TransformFace(
			Erelia.Core.VoxelKit.Face face,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			// If no geometry exists, return input unchanged.
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return face;
			}

			// Convert orientation to 90° step count.
			int steps = (int)orientation;

			// Prepare output face.
			var rotated = new Erelia.Core.VoxelKit.Face();

			// Rotate around voxel center.
			Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);

			List<List<Erelia.Core.VoxelKit.Face.Vertex>> sourcePolygons = face.Polygons;
			for (int p = 0; p < sourcePolygons.Count; p++)
			{
				List<Erelia.Core.VoxelKit.Face.Vertex> sourceVertices = sourcePolygons[p];
				if (sourceVertices == null || sourceVertices.Count == 0)
				{
					continue;
				}

				// Build transformed polygon.
				var rotatedPolygon = new List<Erelia.Core.VoxelKit.Face.Vertex>(sourceVertices.Count);
				for (int i = 0; i < sourceVertices.Count; i++)
				{
					Erelia.Core.VoxelKit.Face.Vertex vertex = sourceVertices[i];
					Vector3 local = vertex.Position;

					// Apply rotation (if any).
					if (steps != 0)
					{
						Vector3 offset = local - pivot;
						Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
						local = rotation * offset + pivot;
					}

					// Apply Y flip (mirror).
					if (flipOrientation == Erelia.Core.VoxelKit.FlipOrientation.NegativeY)
					{
						local.y = 1f - local.y;
					}

					// Store the transformed position.
					vertex.Position = local;
					rotatedPolygon.Add(vertex);
				}

				// If the geometry is mirrored, reverse winding to keep consistent facing.
				if (flipOrientation == Erelia.Core.VoxelKit.FlipOrientation.NegativeY)
				{
					rotatedPolygon.Reverse();
				}

				rotated.Polygons.Add(rotatedPolygon);
			}

			return rotated;
		}

		/// <summary>
		/// Transforms a point in the unit cube by applying a cardinal Y rotation and an optional Y flip.
		/// </summary>
		/// <param name="point">Point to transform.</param>
		/// <param name="orientation">Rotation around Y (in 90° steps).</param>
		/// <param name="flipOrientation">Optional flip (mirroring along Y).</param>
		/// <returns>Transformed point.</returns>
		public static Vector3 TransformPoint(
			Vector3 point,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			// Convert orientation to 90° step count.
			int steps = (int)orientation;

			// Rotate around voxel center.
			Vector3 pivot = new Vector3(0.5f, 0.5f, 0.5f);

			Vector3 local = point;

			// Apply rotation (if any).
			if (steps != 0)
			{
				Vector3 offset = local - pivot;
				Quaternion rotation = Quaternion.AngleAxis(-steps * 90f, Vector3.up);
				local = rotation * offset + pivot;
			}

			// Apply Y flip (mirror).
			if (flipOrientation == Erelia.Core.VoxelKit.FlipOrientation.NegativeY)
			{
				local.y = 1f - local.y;
			}

			return local;
		}

		/// <summary>
		/// Checks whether all vertices of a face lie on the axis-aligned plane of the unit cube.
		/// </summary>
		/// <param name="face">Face to test.</param>
		/// <param name="plane">Axis plane to test against.</param>
		/// <returns><c>true</c> if all vertices are coplanar with the plane; otherwise <c>false</c>.</returns>
		public static bool IsFaceCoplanarWithPlane(Erelia.Core.VoxelKit.Face face, Erelia.Core.VoxelKit.AxisPlane plane)
		{
			// Reject empty geometry.
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return false;
			}

			// Select axis and coordinate value for the target plane.
			float target = 0f;
			int axis = 0;
			switch (plane)
			{
				case Erelia.Core.VoxelKit.AxisPlane.PosX:
					axis = 0; target = 1f; break;
				case Erelia.Core.VoxelKit.AxisPlane.NegX:
					axis = 0; target = 0f; break;
				case Erelia.Core.VoxelKit.AxisPlane.PosY:
					axis = 1; target = 1f; break;
				case Erelia.Core.VoxelKit.AxisPlane.NegY:
					axis = 1; target = 0f; break;
				case Erelia.Core.VoxelKit.AxisPlane.PosZ:
					axis = 2; target = 1f; break;
				case Erelia.Core.VoxelKit.AxisPlane.NegZ:
					axis = 2; target = 0f; break;
			}

			// Validate every vertex matches the target plane coordinate within PointEpsilon.
			List<List<Erelia.Core.VoxelKit.Face.Vertex>> polygons = face.Polygons;
			for (int p = 0; p < polygons.Count; p++)
			{
				List<Erelia.Core.VoxelKit.Face.Vertex> polygon = polygons[p];
				if (polygon == null)
				{
					continue;
				}

				for (int i = 0; i < polygon.Count; i++)
				{
					Vector3 pos = polygon[i].Position;
					float value = axis == 0 ? pos.x : axis == 1 ? pos.y : pos.z;

					// If any vertex deviates, the face is not coplanar.
					if (Mathf.Abs(value - target) > Erelia.Core.VoxelKit.Utils.Geometry.PointEpsilon)
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Determines whether a face is exactly the full unit-cube face for the given plane.
		/// </summary>
		/// <param name="face">Face to test.</param>
		/// <param name="plane">Which unit-cube outer plane this face should match.</param>
		/// <returns><c>true</c> if the face matches the full face; otherwise <c>false</c>.</returns>
		/// <remarks>
		/// This checks:
		/// <list type="bullet">
		/// <item><description>The face has renderable polygons.</description></item>
		/// <item><description>The face is coplanar with the requested axis plane.</description></item>
		/// <item><description>Mutual occlusion between the face and the precomputed full face (equivalence test).</description></item>
		/// </list>
		/// </remarks>
		public static bool IsFullFace(Erelia.Core.VoxelKit.Face face, Erelia.Core.VoxelKit.AxisPlane plane)
		{
			// Reject empty / non-renderable faces.
			if (face == null || !face.HasRenderablePolygons)
			{
				return false;
			}

			// Get the canonical full face for that plane.
			Erelia.Core.VoxelKit.Face fullFace = FullOuterFaces[(int)plane];
			if (fullFace == null)
			{
				return false;
			}

			// The face must lie on the plane.
			if (!IsFaceCoplanarWithPlane(face, plane))
			{
				return false;
			}

			// Equality-like test using mutual occlusion.
			return fullFace.IsOccludedBy(face) && face.IsOccludedBy(fullFace);
		}
	}

	/// <summary>
	/// Utilities for extracting TileUV rectangle information from a Unity <see cref="Sprite"/>.
	/// </summary>
	public static class SpriteUv
	{
		/// <summary>
		/// Computes the TileUV rectangle occupied by a sprite within its texture/atlas.
		/// </summary>
		/// <param name="sprite">Sprite to analyze.</param>
		/// <param name="uvAnchor">Output TileUV minimum corner (bottom-left in TileUV space).</param>
		/// <param name="uvSize">Output TileUV size (width/height in TileUV space).</param>
		/// <remarks>
		/// Unity's <see cref="Sprite.uv"/> returns the UVs for the sprite mesh. This method finds
		/// the min/max TileUV across that array, which yields an axis-aligned TileUV bounds rectangle.
		/// </remarks>
		public static void GetSpriteUvRect(Sprite sprite, out Vector2 uvAnchor, out Vector2 uvSize)
		{
			// If sprite or its TileUV array is missing, return a default full-rect.
			if (sprite == null || sprite.uv == null || sprite.uv.Length == 0)
			{
				uvAnchor = Vector2.zero;
				uvSize = Vector2.one;
				return;
			}

			// Initialize min/max with the first TileUV.
			Vector2 min = sprite.uv[0];
			Vector2 max = sprite.uv[0];

			// Expand bounds using all UVs.
			for (int i = 1; i < sprite.uv.Length; i++)
			{
				Vector2 uv = sprite.uv[i];
				min = Vector2.Min(min, uv);
				max = Vector2.Max(max, uv);
			}

			// Anchor is minimum, size is delta.
			uvAnchor = min;
			uvSize = max - min;
		}
	}
}