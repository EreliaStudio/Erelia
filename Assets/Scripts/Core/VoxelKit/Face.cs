using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.VoxelKit
{
	/// <summary>
	/// Represents a single voxel face geometry as a set of polygons.
	/// Each polygon is described by a list of <see cref="Vertex"/> elements.
	/// </summary>
	public class Face
	{
		/// <summary>
		/// Represents a vertex used by a face polygon.
		/// </summary>
		public struct Vertex
		{
			/// <summary>
			/// The vertex position in 3D space.
			/// </summary>
			public Vector3 Position;

			/// <summary>
			/// The UV coordinates in tile-space (typically used for atlas/tiling).
			/// </summary>
			public Vector2 TileUV;
		}

		/// <summary>
		/// The list of polygons composing this face.
		/// Each polygon is a list of vertices (at least 3 vertices for a renderable polygon).
		/// </summary>
		public List<List<Vertex>> Polygons = new List<List<Vertex>>();

		/// <summary>
		/// Gets a value indicating whether this face contains at least one renderable polygon
		/// (a non-null polygon with at least 3 vertices).
		/// </summary>
		public bool HasRenderablePolygons
		{
			get
			{
				// If the polygon container is missing, the face cannot be rendered.
				if (Polygons == null)
				{
					return false;
				}

				// A polygon is considered renderable if it contains 3+ vertices (triangle or more).
				for (int i = 0; i < Polygons.Count; i++)
				{
					List<Vertex> polygon = Polygons[i];
					if (polygon != null && polygon.Count >= 3)
					{
						return true;
					}
				}

				// No valid polygon found.
				return false;
			}
		}

		/// <summary>
		/// Adds a polygon to this face.
		/// </summary>
		/// <param name="polygon">The polygon vertices to add.</param>
		/// <remarks>
		/// Polygons with no vertices are ignored. This method does not validate winding, planarity,
		/// or vertex count for rendering (beyond checking for null/empty).
		/// </remarks>
		public void AddPolygon(List<Vertex> polygon)
		{
			// Reject null or empty polygons (they have no usable geometry).
			if (polygon == null || polygon.Count == 0)
			{
				return;
			}

			// Store the polygon as-is.
			Polygons.Add(polygon);
		}

		/// <summary>
		/// Applies a UV offset to all vertices of all polygons in this face.
		/// </summary>
		/// <param name="tileOffset">The offset to add to each vertex <see cref="Vertex.TileUV"/>.</param>
		public void ApplyOffset(Vector2 tileOffset)
		{
			// Iterate all polygons.
			for (int p = 0; p < Polygons.Count; p++)
			{
				List<Vertex> polygon = Polygons[p];

				// Iterate all vertices in the polygon and apply the offset.
				// Note: Vertex is a struct, so we must read/modify/write back.
				for (int i = 0; i < polygon.Count; i++)
				{
					Vertex vertex = polygon[i];
					vertex.TileUV += tileOffset;
					polygon[i] = vertex;
				}
			}
		}

		/// <summary>
		/// Determines whether this face is fully occluded by another face.
		/// </summary>
		/// <param name="other">The face used as the occluder.</param>
		/// <returns>
		/// <c>true</c> if every renderable polygon of this face is contained in the union of
		/// <paramref name="other"/>'s polygons along the computed face normal; otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// This method:
		/// <list type="number">
		/// <item><description>Rejects cases where either face has no renderable polygons.</description></item>
		/// <item><description>Computes a stable normal from the first valid polygon of this face.</description></item>
		/// <item><description>Checks that each polygon of this face is contained within the union of the occluder's polygons, projected/compared along that normal.</description></item>
		/// </list>
		/// If a valid normal cannot be computed (degenerate polygons), it returns <c>false</c>.
		/// </remarks>
		public bool IsOccludedBy(Face other)
		{
			// If the occluder is missing, or if either face has no renderable geometry, occlusion cannot be established.
			if (other == null || !HasRenderablePolygons || !other.HasRenderablePolygons)
			{
				return false;
			}

			// Compute a face normal using the first polygon that yields a non-degenerate normal.
			Vector3 normal = Vector3.zero;
			for (int p = 0; p < Polygons.Count; p++)
			{
				List<Vertex> polygon = Polygons[p];
				if (polygon != null && polygon.Count >= 3)
				{
					// Compute polygon normal (implementation is in the Geometry utility).
					normal = Erelia.Core.VoxelKit.Utils.Geometry.GetNormal(polygon);

					// If the normal is strong enough (not near zero), keep it and stop searching.
					if (normal.sqrMagnitude >= Erelia.Core.VoxelKit.Utils.Geometry.NormalEpsilon)
					{
						break;
					}
				}
			}

			// If we still have a near-zero normal, the face is degenerate (cannot reliably test containment).
			if (normal.sqrMagnitude < Erelia.Core.VoxelKit.Utils.Geometry.NormalEpsilon)
			{
				return false;
			}

			// For each polygon of this face, verify it is contained in the union of the occluder's polygons.
			for (int p = 0; p < Polygons.Count; p++)
			{
				List<Vertex> polygon = Polygons[p];
				if (polygon == null || polygon.Count < 3)
				{
					// Skip non-renderable polygons: they do not contribute to visibility tests.
					continue;
				}

				// If any polygon is not fully contained, then the face is not fully occluded.
				if (!Erelia.Core.VoxelKit.Utils.Geometry.IsPolygonContainedInUnion(polygon, other.Polygons, normal))
				{
					return false;
				}
			}

			// All polygons are contained => the face is considered occluded by the other.
			return true;
		}
	}
}