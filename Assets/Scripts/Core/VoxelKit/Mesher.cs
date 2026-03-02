using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.VoxelKit
{
	/// <summary>
	/// Voxel meshing utility that builds Unity meshes from a 3D grid of voxel <see cref="Cell"/> instances.
	/// </summary>
	/// <remarks>
	/// This mesher produces two kinds of meshes:
	/// <list type="bullet">
	///   <item>
	///     <description>
	///       <b>Render mesh</b> (<see cref="BuildRenderMesh"/>): emits triangles + UVs for visible faces.
	///       Outer faces are culled against neighbor voxels using face-vs-face occlusion tests.
	///     </description>
	///   </item>
	///   <item>
	///     <description>
	///       <b>Collision mesh</b> (<see cref="BuildCollisionMesh"/>): emits triangles for collision only.
	///       It first merges large cubic volumes into greedy surface rectangles, then emits remaining shapes
	///       with face occlusion rules.
	///     </description>
	///   </item>
	/// </list>
	/// <para>
	/// Notes:
	/// </para>
	/// <list type="bullet">
	///   <item><description>Predicate parameters allow filtering which voxels participate (e.g., only obstacles).</description></item>
	///   <item><description>Orientation/flip are applied per-cell by transforming canonical (non-oriented) shape faces.</description></item>
	///   <item><description>Several operations rely on caches (see <c>MesherUtils</c>) to avoid recomputing transforms/occlusion.</description></item>
	/// </list>
	/// </remarks>
	static class Mesher
	{
		/// <summary>
		/// Predicate that includes every voxel definition.
		/// </summary>
		public static readonly Func<Erelia.Core.VoxelKit.Definition, bool> AnyVoxelPredicate = (definition) => true;

		/// <summary>
		/// Predicate that includes only voxels marked as <see cref="Traversal.Obstacle"/>.
		/// </summary>
		public static readonly Func<Erelia.Core.VoxelKit.Definition, bool> OnlyObstacleVoxelPredicate =
			(definition) => definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Obstacle;

		/// <summary>
		/// Predicate that includes only voxels marked as <see cref="Traversal.Walkable"/>.
		/// </summary>
		public static readonly Func<Erelia.Core.VoxelKit.Definition, bool> OnlyWalkableVoxelPredicate =
			(definition) => definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Walkable;

		/// <summary>
		/// Builds a render mesh from a voxel grid.
		/// </summary>
		/// <param name="cells">3D voxel cell array (chunk).</param>
		/// <param name="registry">Voxel definition registry used to resolve <see cref="Cell.Id"/>.</param>
		/// <param name="predicate">Optional filter: only voxels whose definition satisfies it are meshed.</param>
		/// <returns>A Unity <see cref="Mesh"/> containing vertices, triangles and UVs.</returns>
		/// <remarks>
		/// For each occupied cell:
		/// <list type="bullet">
		///   <item><description>Outer shell faces are tested against neighbor occlusion. Visible faces are emitted.</description></item>
		///   <item><description>If any outer face is visible, inner faces are emitted (they are not neighbor-culled).</description></item>
		/// </list>
		/// </remarks>
		static public UnityEngine.Mesh BuildRenderMesh(
			Erelia.Core.VoxelKit.Cell[,,] cells,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate)
		{
			// Output buffers (Unity Mesh is built from these).
			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			var uvs = new List<Vector2>();

			// Fail safe: return an empty mesh if inputs are invalid.
			if (cells == null || registry == null)
			{
				return new UnityEngine.Mesh();
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			// Iterate all cells in the chunk grid.
			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						// Resolve cell id -> voxel definition, and apply the filter predicate.
						if (!TryGetDefinition(cells[x, y, z], registry, predicate, out Erelia.Core.VoxelKit.Definition definition, out Erelia.Core.VoxelKit.Cell cell))
						{
							continue;
						}

						// A definition without a shape cannot generate geometry.
						Erelia.Core.VoxelKit.Shape shape = definition.Shape;
						if (shape == null)
						{
							continue;
						}

						Erelia.Core.VoxelKit.Shape.FaceSet faceSet = shape.RenderFaces;

						// Voxel-space offset applied to all vertices emitted for this cell.
						Vector3 offset = new Vector3(x, y, z);

						// Only emit inner faces if at least one outer face is visible (cheap culling heuristic).
						bool anyOuterVisible = false;

						// Evaluate each world plane (+X, -X, +Y, -Y, +Z, -Z).
						for (int i = 0; i < 6; i++)
						{
							Erelia.Core.VoxelKit.AxisPlane nonOrientedPlane = (Erelia.Core.VoxelKit.AxisPlane)(i);

							// Convert “raw plane” to the shape's “local plane” given the cell orientation/flip.
							Erelia.Core.VoxelKit.AxisPlane localPlane =
								Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(nonOrientedPlane, cell.Orientation, cell.FlipOrientation);

							// If the shape provides an outer shell face for this plane, test and emit it.
							if (faceSet.OuterShell != null
								&& faceSet.OuterShell.TryGetValue(localPlane, out Erelia.Core.VoxelKit.Face localFace)
								&& localFace != null
								&& localFace.HasRenderablePolygons)
							{
								// Neighbor-based occlusion: if neighbor fully covers this face, skip.
								if (!IsFaceOccludedByNeighbor(
										cells, sizeX, sizeY, sizeZ,
										x, y, z,
										cell,
										localFace,
										nonOrientedPlane,
										registry,
										predicate,
										useCollision: false))
								{
									// Transform canonical face into world orientation (cached).
									Erelia.Core.VoxelKit.Face transformed =
										TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);

									// Emit triangles + UVs.
									AddFace(transformed, offset, vertices, triangles, uvs);

									anyOuterVisible = true;
								}
							}
							else
							{
								// If the shape does not define an outer face here, we still consider visibility:
								// if the neighbor does NOT occlude a "full face" placeholder, the cell is considered exposed.
								if (!IsFullFaceOccludedByNeighbor(
										cells, sizeX, sizeY, sizeZ,
										x, y, z,
										nonOrientedPlane,
										registry,
										predicate,
										useCollision: false))
								{
									anyOuterVisible = true;
								}
							}
						}

						// Inner faces: these are not associated with a specific axis plane, so we emit them
						// only if the cell has at least one visible outer face.
						if (anyOuterVisible && faceSet.Inner != null)
						{
							for (int i = 0; i < faceSet.Inner.Count; i++)
							{
								Erelia.Core.VoxelKit.Face innerFace =
									TransformFaceCached(faceSet.Inner[i], cell.Orientation, cell.FlipOrientation);

								AddFace(innerFace, offset, vertices, triangles, uvs);
							}
						}
					}
				}
			}

			// Build Unity mesh from collected buffers.
			var result = new UnityEngine.Mesh();

			// Switch to 32-bit indices if we exceed 16-bit index limit.
			if (vertices.Count >= 65535)
			{
				result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			result.SetVertices(vertices);
			result.SetTriangles(triangles, 0);
			result.SetUVs(0, uvs);

			// Normals/bounds are derived after topology is set.
			result.RecalculateNormals();
			result.RecalculateBounds();

			return result;
		}

		/// <summary>
		/// Builds a collision mesh from a voxel grid.
		/// </summary>
		/// <param name="cells">3D voxel cell array (chunk).</param>
		/// <param name="registry">Voxel definition registry used to resolve <see cref="Cell.Id"/>.</param>
		/// <param name="predicate">Optional filter: only voxels whose definition satisfies it are meshed.</param>
		/// <returns>A Unity <see cref="Mesh"/> containing vertices and triangles (no UVs required).</returns>
		/// <remarks>
		/// This method produces a <b>single</b> resulting collision mesh using two passes:
		/// <list type="number">
		///   <item>
		///     <description>
		///       <b>Pass 1 (merge cubic volumes):</b>
		///       Detect fully cubic collision voxels, then find the largest contiguous boxes of cubic voxels sharing the same ID,
		///       mark them as <c>consumed</c>, and emit a greedy surface mesh (rectangles/quads) for the outer surface of the
		///       consumed volume, and repeat this process until no large cubic volupme is found.
		/// 	  Neighbor occlusion uses the <c>solid</c> field (not <c>consumed</c>) to avoid generating
		///       internal faces.
		///     </description>
		///   </item>
		///   <item>
		///     <description>
		///       <b>Pass 2 (emit remaining voxels):</b>
		///       For solid voxels not consumed in pass 1, emit collision triangles using the regular face-occlusion logic,
		///       without polygon merging.
		///     </description>
		///   </item>
		/// </list>
		/// </remarks>
		static public UnityEngine.Mesh BuildCollisionMesh(
			Erelia.Core.VoxelKit.Cell[,,] cells,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate)
		{
			if (cells == null || registry == null)
			{
				return new UnityEngine.Mesh();
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			// Build helper maps:
			// - solid[x,y,z] = participates in collision meshing
			// - cubic[x,y,z] = is a “full cube” collision voxel (all 6 outer faces are full)
			// - ids[x,y,z]   = voxel id (for merging only same-id volumes)
			BuildSolidAndCubicMaps(
				cells,
				registry,
				predicate,
				out bool[,,] solid,
				out bool[,,] cubic,
				out int[,,] ids);

			// Mark voxels that got merged into a large box (so they won't be emitted again in pass 2).
			var consumed = new bool[sizeX, sizeY, sizeZ];

			// Find large same-id cubic boxes and mark them consumed.
			ConsumeLargestCubicBoxes(ids, solid, cubic, consumed, sizeX, sizeY, sizeZ);

			// Output buffers for the single collision mesh.
			var vertices = new List<Vector3>();
			var triangles = new List<int>();

			// Pass 1: emit greedy rectangles for the surface of the consumed volume.
			EmitGreedySurfaceForConsumed(consumed, solid, sizeX, sizeY, sizeZ, vertices, triangles);

			// Pass 2: emit remaining collision faces for non-consumed solids.
			EmitRemainingCollisionFaces(
				cells, registry, predicate,
				solid, consumed,
				sizeX, sizeY, sizeZ,
				vertices, triangles);

			var result = new UnityEngine.Mesh();
			if (vertices.Count >= 65535)
			{
				result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			result.SetVertices(vertices);
			result.SetTriangles(triangles, 0);
			result.RecalculateNormals();
			result.RecalculateBounds();
			return result;
		}

		/// <summary>
		/// Returns <c>true</c> if a face set contains no outer-shell faces and no inner faces.
		/// </summary>
		/// <param name="faceSet">Face set to test.</param>
		private static bool IsFaceSetEmpty(Erelia.Core.VoxelKit.Shape.FaceSet faceSet)
		{
			// Outer shell is a dictionary keyed by planes; inner is a list of arbitrary faces.
			bool hasOuter = faceSet.OuterShell != null && faceSet.OuterShell.Count > 0;
			bool hasInner = faceSet.Inner != null && faceSet.Inner.Count > 0;
			return !hasOuter && !hasInner;
		}

		/// <summary>
		/// Builds helper maps used by collision meshing (solid/cubic/id).
		/// </summary>
		/// <param name="cells">Voxel grid.</param>
		/// <param name="registry">Voxel registry.</param>
		/// <param name="predicate">Voxel filter predicate.</param>
		/// <param name="solid">Outputs whether a cell participates in collision geometry.</param>
		/// <param name="cubic">Outputs whether a cell is a fully cubic collision voxel.</param>
		/// <param name="ids">Outputs voxel ids (or -1 when absent).</param>
		private static void BuildSolidAndCubicMaps(
			Erelia.Core.VoxelKit.Cell[,,] cells,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate,
			out bool[,,] solid,
			out bool[,,] cubic,
			out int[,,] ids)
		{
			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			solid = new bool[sizeX, sizeY, sizeZ];
			cubic = new bool[sizeX, sizeY, sizeZ];
			ids = new int[sizeX, sizeY, sizeZ];

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						// Default id value for “no voxel”.
						ids[x, y, z] = -1;

						// Resolve definition (and apply predicate).
						if (!TryGetDefinition(cells[x, y, z], registry, predicate, out Erelia.Core.VoxelKit.Definition def, out Erelia.Core.VoxelKit.Cell cell))
						{
							continue;
						}

						Erelia.Core.VoxelKit.Shape shape = def.Shape;
						if (shape == null)
						{
							continue;
						}

						Erelia.Core.VoxelKit.Shape.FaceSet fs = shape.CollisionFaces;

						// If collision faces are empty, the voxel does not contribute to collision.
						if (IsFaceSetEmpty(fs))
						{
							continue;
						}

						// Check if there is at least one renderable polygon somewhere in the collision set.
						// (A face set may exist but be non-renderable depending on authoring.)
						bool hasAnyRenderable = false;

						if (fs.Inner != null)
						{
							for (int i = 0; i < fs.Inner.Count; i++)
							{
								if (fs.Inner[i] != null && fs.Inner[i].HasRenderablePolygons)
								{
									hasAnyRenderable = true;
									break;
								}
							}
						}

						if (!hasAnyRenderable && fs.OuterShell != null)
						{
							foreach (var kv in fs.OuterShell)
							{
								if (kv.Value != null && kv.Value.HasRenderablePolygons)
								{
									hasAnyRenderable = true;
									break;
								}
							}
						}

						// If nothing is renderable, ignore the voxel for collision emission.
						if (!hasAnyRenderable)
						{
							continue;
						}

						// Mark voxel as solid for collision and store its id.
						solid[x, y, z] = true;
						ids[x, y, z] = cell.Id;

						// Determine whether this voxel behaves like a full cube (all outer planes fully filled).
						cubic[x, y, z] = IsCubicCollisionVoxel(cell, shape);
					}
				}
			}
		}

		/// <summary>
		/// Determines whether a voxel's collision geometry is a full cube.
		/// </summary>
		/// <param name="cell">Cell instance containing orientation/flip/id.</param>
		/// <param name="shape">Shape providing collision faces.</param>
		/// <returns><c>true</c> if all 6 outer collision faces are present and each is a full face; otherwise <c>false</c>.</returns>
		private static bool IsCubicCollisionVoxel(Erelia.Core.VoxelKit.Cell cell, Erelia.Core.VoxelKit.Shape shape)
		{
			if (cell == null || shape == null)
			{
				return false;
			}

			Erelia.Core.VoxelKit.Shape.FaceSet fs = shape.CollisionFaces;
			if (fs.OuterShell == null || fs.OuterShell.Count == 0)
			{
				return false;
			}

			// A voxel is “cubic” if, for each world plane, the mapped local plane provides a face that,
			// once transformed to world, is equivalent to a full unit face for that plane.
			for (int i = 0; i < 6; i++)
			{
				Erelia.Core.VoxelKit.AxisPlane nonOrientedPlane = (Erelia.Core.VoxelKit.AxisPlane)i;
				Erelia.Core.VoxelKit.AxisPlane localPlane =
					Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(nonOrientedPlane, cell.Orientation, cell.FlipOrientation);

				if (!fs.OuterShell.TryGetValue(localPlane, out Erelia.Core.VoxelKit.Face localFace)
					|| localFace == null
					|| !localFace.HasRenderablePolygons)
				{
					return false;
				}

				Erelia.Core.VoxelKit.Face worldFace = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
				if (!Erelia.Core.VoxelKit.Utils.Geometry.IsFullFace(worldFace, nonOrientedPlane))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Represents a rectangular volume (axis-aligned box) in voxel coordinates.
		/// </summary>
		private struct Box
		{
			/// <summary>Box origin (minimum corner) in voxel coordinates.</summary>
			public int X;
			public int Y;
			public int Z;

			/// <summary>Box sizes (extent) in voxels for each axis.</summary>
			public int SX;
			public int SY;
			public int SZ;

			/// <summary>Volume in voxels.</summary>
			public int Volume => SX * SY * SZ;
		}

		/// <summary>
		/// Repeatedly finds and consumes the largest contiguous cubic boxes of same id.
		/// </summary>
		/// <param name="ids">Voxel ids map.</param>
		/// <param name="solid">Solid participation map.</param>
		/// <param name="cubic">Cubic-voxel map.</param>
		/// <param name="consumed">Output map marking voxels consumed by merged boxes.</param>
		/// <param name="sizeX">Grid size X.</param>
		/// <param name="sizeY">Grid size Y.</param>
		/// <param name="sizeZ">Grid size Z.</param>
		private static void ConsumeLargestCubicBoxes(
			int[,,] ids,
			bool[,,] solid,
			bool[,,] cubic,
			bool[,,] consumed,
			int sizeX,
			int sizeY,
			int sizeZ)
		{
			// Greedy loop:
			// - Find the largest valid box anywhere
			// - Mark it consumed
			// - Repeat until remaining boxes are too small
			while (true)
			{
				Box best = default;
				int bestVol = 0;

				for (int x = 0; x < sizeX; x++)
				{
					for (int y = 0; y < sizeY; y++)
					{
						for (int z = 0; z < sizeZ; z++)
						{
							// Only consider solid, cubic, not-yet-consumed cells.
							if (!solid[x, y, z] || !cubic[x, y, z] || consumed[x, y, z])
							{
								continue;
							}

							int id = ids[x, y, z];
							if (id < 0)
							{
								continue;
							}

							// Compute the best box anchored at this cell.
							if (TryFindBestBoxAt(x, y, z, id, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ, out Box candidate))
							{
								int vol = candidate.Volume;
								if (vol > bestVol)
								{
									bestVol = vol;
									best = candidate;
								}
							}
						}
					}
				}

				const int MinBoxVolume = 2;

				// Stop when no worthwhile merge candidate remains.
				if (bestVol < MinBoxVolume)
				{
					break;
				}

				// Mark all voxels in the chosen box as consumed.
				for (int ix = best.X; ix < best.X + best.SX; ix++)
				{
					for (int iy = best.Y; iy < best.Y + best.SY; iy++)
					{
						for (int iz = best.Z; iz < best.Z + best.SZ; iz++)
						{
							consumed[ix, iy, iz] = true;
						}
					}
				}
			}
		}

		/// <summary>
		/// Finds the largest valid same-id cubic box starting at a given origin cell.
		/// </summary>
		/// <param name="x0">Origin X.</param>
		/// <param name="y0">Origin Y.</param>
		/// <param name="z0">Origin Z.</param>
		/// <param name="id">Required voxel id for all box cells.</param>
		/// <param name="ids">Id map.</param>
		/// <param name="solid">Solid map.</param>
		/// <param name="cubic">Cubic map.</param>
		/// <param name="consumed">Consumed map.</param>
		/// <param name="sizeX">Grid size X.</param>
		/// <param name="sizeY">Grid size Y.</param>
		/// <param name="sizeZ">Grid size Z.</param>
		/// <param name="best">Best box found (output).</param>
		/// <returns><c>true</c> if origin cell is valid and a box was evaluated; otherwise <c>false</c>.</returns>
		private static bool TryFindBestBoxAt(
			int x0, int y0, int z0,
			int id,
			int[,,] ids,
			bool[,,] solid,
			bool[,,] cubic,
			bool[,,] consumed,
			int sizeX, int sizeY, int sizeZ,
			out Box best)
		{
			// Default smallest possible box is 1x1x1 at the origin.
			best = new Box { X = x0, Y = y0, Z = z0, SX = 1, SY = 1, SZ = 1 };

			// If the origin cell isn't acceptable, no box can be formed.
			if (!CellOk(x0, y0, z0, id, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
			{
				return false;
			}

			// Try different axis growth orders to find the best box volume.
			int[][] perms = new int[][]
			{
				new int[]{0,1,2},
				new int[]{0,2,1},
				new int[]{1,0,2},
				new int[]{1,2,0},
				new int[]{2,0,1},
				new int[]{2,1,0},
			};

			int bestVol = 1;

			for (int p = 0; p < perms.Length; p++)
			{
				int a = perms[p][0];
				int b = perms[p][1];
				int c = perms[p][2];

				int sx = 1, sy = 1, sz = 1;

				// Grow along axis A then B then C.
				GrowAxis(a, x0, y0, z0, id, ref sx, ref sy, ref sz, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ);
				GrowAxis(b, x0, y0, z0, id, ref sx, ref sy, ref sz, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ);
				GrowAxis(c, x0, y0, z0, id, ref sx, ref sy, ref sz, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ);

				int vol = sx * sy * sz;
				if (vol > bestVol)
				{
					bestVol = vol;
					best = new Box { X = x0, Y = y0, Z = z0, SX = sx, SY = sy, SZ = sz };
				}
			}

			return true;
		}

		/// <summary>
		/// Grows a candidate box along one axis as much as possible.
		/// </summary>
		/// <param name="axis">Axis index: 0=X, 1=Y, 2=Z.</param>
		/// <param name="x0">Origin X.</param>
		/// <param name="y0">Origin Y.</param>
		/// <param name="z0">Origin Z.</param>
		/// <param name="id">Required id.</param>
		/// <param name="sx">Current size X (in/out).</param>
		/// <param name="sy">Current size Y (in/out).</param>
		/// <param name="sz">Current size Z (in/out).</param>
		/// <param name="ids">Id map.</param>
		/// <param name="solid">Solid map.</param>
		/// <param name="cubic">Cubic map.</param>
		/// <param name="consumed">Consumed map.</param>
		/// <param name="sizeX">Grid size X.</param>
		/// <param name="sizeY">Grid size Y.</param>
		/// <param name="sizeZ">Grid size Z.</param>
		private static void GrowAxis(
			int axis,
			int x0, int y0, int z0,
			int id,
			ref int sx, ref int sy, ref int sz,
			int[,,] ids,
			bool[,,] solid,
			bool[,,] cubic,
			bool[,,] consumed,
			int sizeX, int sizeY, int sizeZ)
		{
			// Attempt to extend the box by one layer along the chosen axis while valid.
			while (true)
			{
				int nsx = sx, nsy = sy, nsz = sz;
				if (axis == 0) nsx++;
				else if (axis == 1) nsy++;
				else nsz++;

				// Verify that the new outer “layer” is composed exclusively of valid cells.
				if (!ValidateBoxLayer(x0, y0, z0, id, sx, sy, sz, nsx, nsy, nsz, axis, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
				{
					break;
				}

				// Accept growth.
				sx = nsx;
				sy = nsy;
				sz = nsz;
			}
		}

		/// <summary>
		/// Validates the new layer introduced by growing a box along one axis.
		/// </summary>
		private static bool ValidateBoxLayer(
			int x0, int y0, int z0,
			int id,
			int sx, int sy, int sz,
			int nsx, int nsy, int nsz,
			int grownAxis,
			int[,,] ids,
			bool[,,] solid,
			bool[,,] cubic,
			bool[,,] consumed,
			int sizeX, int sizeY, int sizeZ)
		{
			// When growing along X, validate the new x = x0 + nsx - 1 plane.
			if (grownAxis == 0)
			{
				int x = x0 + nsx - 1;
				for (int y = y0; y < y0 + nsy; y++)
				{
					for (int z = z0; z < z0 + nsz; z++)
					{
						if (!CellOk(x, y, z, id, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
						{
							return false;
						}
					}
				}
				return true;
			}

			// When growing along Y, validate the new y = y0 + nsy - 1 plane.
			if (grownAxis == 1)
			{
				int y = y0 + nsy - 1;
				for (int x = x0; x < x0 + nsx; x++)
				{
					for (int z = z0; z < z0 + nsz; z++)
					{
						if (!CellOk(x, y, z, id, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
						{
							return false;
						}
					}
				}
				return true;
			}

			// When growing along Z, validate the new z = z0 + nsz - 1 plane.
			{
				int z = z0 + nsz - 1;
				for (int x = x0; x < x0 + nsx; x++)
				{
					for (int y = y0; y < y0 + nsy; y++)
					{
						if (!CellOk(x, y, z, id, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		/// <summary>
		/// Returns whether a single cell can be included in a merged cubic box.
		/// </summary>
		/// <remarks>
		/// Conditions:
		/// <list type="bullet">
		///   <item><description>Inside bounds.</description></item>
		///   <item><description>Solid and cubic.</description></item>
		///   <item><description>Not already consumed.</description></item>
		///   <item><description>Id matches the box id.</description></item>
		/// </list>
		/// </remarks>
		private static bool CellOk(
			int x, int y, int z,
			int id,
			int[,,] ids,
			bool[,,] solid,
			bool[,,] cubic,
			bool[,,] consumed,
			int sizeX, int sizeY, int sizeZ)
		{
			// Bounds check.
			if (x < 0 || y < 0 || z < 0 || x >= sizeX || y >= sizeY || z >= sizeZ)
			{
				return false;
			}

			// Must be part of collision, cubic, and not consumed already.
			if (!solid[x, y, z] || consumed[x, y, z] || !cubic[x, y, z])
			{
				return false;
			}

			// Must match the id of the box we're growing.
			return ids[x, y, z] == id;
		}

		/// <summary>
		/// Emits the greedy surface mesh (rectangles) for all consumed voxels.
		/// </summary>
		/// <param name="consumed">Consumed voxels map.</param>
		/// <param name="solid">Solid voxels map (used to decide whether a side is exposed).</param>
		/// <param name="sizeX">Grid size X.</param>
		/// <param name="sizeY">Grid size Y.</param>
		/// <param name="sizeZ">Grid size Z.</param>
		/// <param name="vertices">Output vertices list.</param>
		/// <param name="triangles">Output triangles list.</param>
		/// <remarks>
		/// For each of the six directions, a 2D mask is computed and merged into maximal rectangles,
		/// then emitted as quads. A face is “filled” if the voxel is consumed and the neighboring cell
		/// in that direction is not solid (or out of bounds).
		/// </remarks>
		private static void EmitGreedySurfaceForConsumed(
			bool[,,] consumed,
			bool[,,] solid,
			int sizeX, int sizeY, int sizeZ,
			List<Vector3> vertices,
			List<int> triangles)
		{
			// +X faces (planes at x+1)
			for (int x = 0; x < sizeX; x++)
			{
				EmitGreedyMask2D(
					uSize: sizeY,
					vSize: sizeZ,
					isFilled: (u, v) =>
					{
						// u=varying Y, v=varying Z
						int y = u;
						int z = v;

						// Only consumed voxels contribute.
						if (!consumed[x, y, z]) return false;

						// At boundary, face is exposed.
						if (x == sizeX - 1) return true;

						// Otherwise, exposed only if neighbor in +X is not solid.
						return !solid[x + 1, y, z];
					},
					emitRect: (u0, v0, du, dv) =>
					{
						// Convert rectangle in mask coordinates into a quad in 3D.
						float xp = x + 1f;
						float y0 = u0;
						float y1 = u0 + du;
						float z0 = v0;
						float z1 = v0 + dv;

						Vector3 p0 = new Vector3(xp, y0, z0);
						Vector3 p1 = new Vector3(xp, y1, z0);
						Vector3 p2 = new Vector3(xp, y1, z1);
						Vector3 p3 = new Vector3(xp, y0, z1);

						// Ensure winding matches +X normal.
						AddQuad(p0, p1, p2, p3, Vector3.right, vertices, triangles);
					});
			}

			// -X faces (planes at x)
			for (int x = 0; x < sizeX; x++)
			{
				EmitGreedyMask2D(
					uSize: sizeY,
					vSize: sizeZ,
					isFilled: (u, v) =>
					{
						int y = u;
						int z = v;
						if (!consumed[x, y, z]) return false;
						if (x == 0) return true;
						return !solid[x - 1, y, z];
					},
					emitRect: (u0, v0, du, dv) =>
					{
						float xp = x;
						float y0 = u0;
						float y1 = u0 + du;
						float z0 = v0;
						float z1 = v0 + dv;

						Vector3 p0 = new Vector3(xp, y0, z1);
						Vector3 p1 = new Vector3(xp, y1, z1);
						Vector3 p2 = new Vector3(xp, y1, z0);
						Vector3 p3 = new Vector3(xp, y0, z0);

						AddQuad(p0, p1, p2, p3, Vector3.left, vertices, triangles);
					});
			}

			// +Y faces (planes at y+1)
			for (int y = 0; y < sizeY; y++)
			{
				EmitGreedyMask2D(
					uSize: sizeX,
					vSize: sizeZ,
					isFilled: (u, v) =>
					{
						int x = u;
						int z = v;
						if (!consumed[x, y, z]) return false;
						if (y == sizeY - 1) return true;
						return !solid[x, y + 1, z];
					},
					emitRect: (u0, v0, du, dv) =>
					{
						float yp = y + 1f;
						float x0 = u0;
						float x1 = u0 + du;
						float z0 = v0;
						float z1 = v0 + dv;

						Vector3 p0 = new Vector3(x0, yp, z0);
						Vector3 p1 = new Vector3(x1, yp, z0);
						Vector3 p2 = new Vector3(x1, yp, z1);
						Vector3 p3 = new Vector3(x0, yp, z1);

						AddQuad(p0, p1, p2, p3, Vector3.up, vertices, triangles);
					});
			}

			// -Y faces (planes at y)
			for (int y = 0; y < sizeY; y++)
			{
				EmitGreedyMask2D(
					uSize: sizeX,
					vSize: sizeZ,
					isFilled: (u, v) =>
					{
						int x = u;
						int z = v;
						if (!consumed[x, y, z]) return false;
						if (y == 0) return true;
						return !solid[x, y - 1, z];
					},
					emitRect: (u0, v0, du, dv) =>
					{
						float yp = y;
						float x0 = u0;
						float x1 = u0 + du;
						float z0 = v0;
						float z1 = v0 + dv;

						Vector3 p0 = new Vector3(x0, yp, z1);
						Vector3 p1 = new Vector3(x1, yp, z1);
						Vector3 p2 = new Vector3(x1, yp, z0);
						Vector3 p3 = new Vector3(x0, yp, z0);

						AddQuad(p0, p1, p2, p3, Vector3.down, vertices, triangles);
					});
			}

			// +Z faces (planes at z+1)
			for (int z = 0; z < sizeZ; z++)
			{
				EmitGreedyMask2D(
					uSize: sizeX,
					vSize: sizeY,
					isFilled: (u, v) =>
					{
						int x = u;
						int y = v;
						if (!consumed[x, y, z]) return false;
						if (z == sizeZ - 1) return true;
						return !solid[x, y, z + 1];
					},
					emitRect: (u0, v0, du, dv) =>
					{
						float zp = z + 1f;
						float x0 = u0;
						float x1 = u0 + du;
						float y0 = v0;
						float y1 = v0 + dv;

						Vector3 p0 = new Vector3(x0, y0, zp);
						Vector3 p1 = new Vector3(x1, y0, zp);
						Vector3 p2 = new Vector3(x1, y1, zp);
						Vector3 p3 = new Vector3(x0, y1, zp);

						AddQuad(p0, p1, p2, p3, Vector3.forward, vertices, triangles);
					});
			}

			// -Z faces (planes at z)
			for (int z = 0; z < sizeZ; z++)
			{
				EmitGreedyMask2D(
					uSize: sizeX,
					vSize: sizeY,
					isFilled: (u, v) =>
					{
						int x = u;
						int y = v;
						if (!consumed[x, y, z]) return false;
						if (z == 0) return true;
						return !solid[x, y, z - 1];
					},
					emitRect: (u0, v0, du, dv) =>
					{
						float zp = z;
						float x0 = u0;
						float x1 = u0 + du;
						float y0 = v0;
						float y1 = v0 + dv;

						Vector3 p0 = new Vector3(x1, y0, zp);
						Vector3 p1 = new Vector3(x0, y0, zp);
						Vector3 p2 = new Vector3(x0, y1, zp);
						Vector3 p3 = new Vector3(x1, y1, zp);

						AddQuad(p0, p1, p2, p3, Vector3.back, vertices, triangles);
					});
			}
		}

		/// <summary>
		/// Greedy-rectangle extraction on a 2D boolean mask.
		/// </summary>
		/// <param name="uSize">Mask size along U axis.</param>
		/// <param name="vSize">Mask size along V axis.</param>
		/// <param name="isFilled">Callback telling whether a cell (u,v) should be emitted.</param>
		/// <param name="emitRect">Callback invoked for each maximal rectangle found (u0,v0,height,width).</param>
		/// <remarks>
		/// This is the classic greedy meshing step for a 2D slice:
		/// find an unused filled cell, expand width, then expand height while all cells remain filled/unused,
		/// mark rectangle as used, emit it.
		/// </remarks>
		private static void EmitGreedyMask2D(
			int uSize,
			int vSize,
			Func<int, int, bool> isFilled,
			Action<int, int, int, int> emitRect)
		{
			var used = new bool[uSize, vSize];

			for (int u = 0; u < uSize; u++)
			{
				for (int v = 0; v < vSize; v++)
				{
					// Skip cells already merged into a rectangle or not filled.
					if (used[u, v] || !isFilled(u, v))
					{
						continue;
					}

					// Compute maximal width along V.
					int w = 1;
					while (v + w < vSize && !used[u, v + w] && isFilled(u, v + w))
					{
						w++;
					}

					// Compute maximal height along U, requiring the whole width to be filled.
					int h = 1;
					bool canGrow = true;
					while (u + h < uSize && canGrow)
					{
						for (int k = 0; k < w; k++)
						{
							if (used[u + h, v + k] || !isFilled(u + h, v + k))
							{
								canGrow = false;
								break;
							}
						}

						if (canGrow)
						{
							h++;
						}
					}

					// Mark rectangle area as used.
					for (int du = 0; du < h; du++)
					{
						for (int dv = 0; dv < w; dv++)
						{
							used[u + du, v + dv] = true;
						}
					}

					// Emit the rectangle as a single quad.
					emitRect(u, v, h, w);
				}
			}
		}

		/// <summary>
		/// Adds a quad (two triangles) to the collision buffers, ensuring correct winding for a desired normal.
		/// </summary>
		/// <param name="p0">Quad corner 0.</param>
		/// <param name="p1">Quad corner 1.</param>
		/// <param name="p2">Quad corner 2.</param>
		/// <param name="p3">Quad corner 3.</param>
		/// <param name="expectedNormal">Desired outward normal direction.</param>
		/// <param name="vertices">Vertex buffer.</param>
		/// <param name="triangles">Triangle index buffer.</param>
		private static void AddQuad(
			Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
			Vector3 expectedNormal,
			List<Vector3> vertices,
			List<int> triangles)
		{
			int start = vertices.Count;

			// Compute current normal to detect wrong winding.
			Vector3 n = Vector3.Cross(p1 - p0, p2 - p0);
			if (Vector3.Dot(n, expectedNormal) < 0f)
			{
				// Swap to flip winding (p1 <-> p3).
				Vector3 tmp = p1;
				p1 = p3;
				p3 = tmp;
			}

			// Append vertices.
			vertices.Add(p0);
			vertices.Add(p1);
			vertices.Add(p2);
			vertices.Add(p3);

			// Append indices (two triangles).
			triangles.Add(start + 0);
			triangles.Add(start + 1);
			triangles.Add(start + 2);

			triangles.Add(start + 0);
			triangles.Add(start + 2);
			triangles.Add(start + 3);
		}

		/// <summary>
		/// Emits collision triangles for all solid voxels that were not consumed by the cubic merge step.
		/// </summary>
		private static void EmitRemainingCollisionFaces(
			Erelia.Core.VoxelKit.Cell[,,] cells,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate,
			bool[,,] solid,
			bool[,,] consumed,
			int sizeX, int sizeY, int sizeZ,
			List<Vector3> vertices,
			List<int> triangles)
		{
			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						// Skip air / non-collision / already merged voxels.
						if (!solid[x, y, z] || consumed[x, y, z])
						{
							continue;
						}

						// Resolve definition and the cell instance.
						if (!TryGetDefinition(cells[x, y, z], registry, predicate, out Erelia.Core.VoxelKit.Definition definition, out Erelia.Core.VoxelKit.Cell cell))
						{
							continue;
						}

						Erelia.Core.VoxelKit.Shape shape = definition.Shape;
						if (shape == null)
						{
							continue;
						}

						Erelia.Core.VoxelKit.Shape.FaceSet faceSet = shape.CollisionFaces;
						if (IsFaceSetEmpty(faceSet))
						{
							continue;
						}

						Vector3 offset = new Vector3(x, y, z);

						bool anyOuterVisible = false;

						// Same outer/inner logic as render, but triangles only and using collision faces.
						for (int i = 0; i < 6; i++)
						{
							Erelia.Core.VoxelKit.AxisPlane nonOrientedPlane = (Erelia.Core.VoxelKit.AxisPlane)i;
							Erelia.Core.VoxelKit.AxisPlane localPlane =
								Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(nonOrientedPlane, cell.Orientation, cell.FlipOrientation);

							if (faceSet.OuterShell != null
								&& faceSet.OuterShell.TryGetValue(localPlane, out Erelia.Core.VoxelKit.Face localFace)
								&& localFace != null
								&& localFace.HasRenderablePolygons)
							{
								if (!IsFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, cell, localFace, nonOrientedPlane, registry, predicate, useCollision: true))
								{
									Erelia.Core.VoxelKit.Face transformed =
										TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);

									AddFaceTrianglesOnly(transformed, offset, vertices, triangles);
									anyOuterVisible = true;
								}
							}
							else
							{
								if (!IsFullFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, nonOrientedPlane, registry, predicate, useCollision: true))
								{
									anyOuterVisible = true;
								}
							}
						}

						// Inner collision faces are emitted only if at least one outer face is visible.
						if (anyOuterVisible && faceSet.Inner != null)
						{
							for (int i = 0; i < faceSet.Inner.Count; i++)
							{
								Erelia.Core.VoxelKit.Face innerFace =
									TransformFaceCached(faceSet.Inner[i], cell.Orientation, cell.FlipOrientation);

								AddFaceTrianglesOnly(innerFace, offset, vertices, triangles);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Adds all polygons of a face to the collision buffers (triangles only, no UVs).
		/// </summary>
		/// <param name="face">Face to emit.</param>
		/// <param name="positionOffset">Voxel-space offset (cell position).</param>
		/// <param name="vertices">Vertex buffer.</param>
		/// <param name="triangles">Triangle buffer.</param>
		private static void AddFaceTrianglesOnly(
			Erelia.Core.VoxelKit.Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles)
		{
			// Nothing to emit.
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			// For each polygon, emit vertices then triangulate as a fan (0, i+1, i).
			List<List<Erelia.Core.VoxelKit.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Erelia.Core.VoxelKit.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;

				// Append vertices with cell offset.
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Erelia.Core.VoxelKit.Face.Vertex v = faceVertices[i];
					vertices.Add(positionOffset + v.Position);
				}

				// Triangulate the polygon (triangle fan).
				for (int i = 1; i < faceVertices.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}
		}

		/// <summary>
		/// Resolves a cell's voxel definition from the registry and applies a filter predicate.
		/// </summary>
		/// <param name="cell">Cell to resolve.</param>
		/// <param name="registry">Registry used to map id to definition.</param>
		/// <param name="predicate">Optional filter predicate.</param>
		/// <param name="definition">Resolved definition if found.</param>
		/// <param name="resolvedCell">Returned cell (currently identical to <paramref name="cell"/>).</param>
		/// <returns><c>true</c> if resolved and accepted; otherwise <c>false</c>.</returns>
		private static bool TryGetDefinition(
			Erelia.Core.VoxelKit.Cell cell,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate,
			out Erelia.Core.VoxelKit.Definition definition,
			out Erelia.Core.VoxelKit.Cell resolvedCell)
		{
			definition = null;
			resolvedCell = cell;

			// Reject invalid cell or id.
			if (cell == null || cell.Id < 0 || registry == null)
			{
				return false;
			}

			// Resolve id -> definition.
			if (!registry.TryGet(cell.Id, out definition))
			{
				return false;
			}

			// Apply filter if provided.
			if (predicate != null && !predicate(definition))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Transforms a canonical face according to orientation/flip, using a cache.
		/// </summary>
		/// <param name="face">Canonical (non-oriented) face.</param>
		/// <param name="orientation">Rotation around Y.</param>
		/// <param name="flipOrientation">Optional Y flip.</param>
		/// <returns>The transformed face (cached), or the original face if caching fails.</returns>
		private static Erelia.Core.VoxelKit.Face TransformFaceCached(
			Erelia.Core.VoxelKit.Face face,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			if (face == null)
			{
				return null;
			}

			// Cache lookup (and computation on miss) lives in MesherUtils.
			if (Erelia.Core.VoxelKit.MesherUtils.FaceByOrientationCache.TryGetValue(face, orientation, flipOrientation, out Erelia.Core.VoxelKit.Face output))
			{
				return output;
			}

			// Fallback: return original face if cache indicates failure (should be rare).
			return face;
		}

		/// <summary>
		/// Adds a face (triangles + UVs) to render buffers.
		/// </summary>
		/// <param name="face">Face to emit.</param>
		/// <param name="positionOffset">Voxel-space offset (cell position).</param>
		/// <param name="vertices">Vertex buffer.</param>
		/// <param name="triangles">Triangle buffer.</param>
		/// <param name="uvs">UV buffer.</param>
		private static void AddFace(
			Erelia.Core.VoxelKit.Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			// Emit polygons and triangulate them as triangle fans.
			List<List<Erelia.Core.VoxelKit.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Erelia.Core.VoxelKit.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;

				// Append vertices + UVs.
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Erelia.Core.VoxelKit.Face.Vertex vertex = faceVertices[i];
					vertices.Add(positionOffset + vertex.Position);
					uvs.Add(vertex.TileUV);
				}

				// Triangulate.
				for (int i = 1; i < faceVertices.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}
		}

		/// <summary>
		/// Tests whether a given local face is fully occluded by the neighbor voxel on the specified world plane.
		/// </summary>
		/// <param name="cells">Voxel grid.</param>
		/// <param name="sizeX">Grid size X.</param>
		/// <param name="sizeY">Grid size Y.</param>
		/// <param name="sizeZ">Grid size Z.</param>
		/// <param name="x">Current cell X.</param>
		/// <param name="y">Current cell Y.</param>
		/// <param name="z">Current cell Z.</param>
		/// <param name="cell">Current cell instance (orientation/flip/id).</param>
		/// <param name="localFace">Face in the shape's local space (before transform).</param>
		/// <param name="nonOrientedPlane">World plane of the face being tested.</param>
		/// <param name="registry">Definition registry.</param>
		/// <param name="predicate">Voxel filter predicate.</param>
		/// <param name="useCollision">If true, use neighbor collision faces; otherwise neighbor render faces.</param>
		/// <returns><c>true</c> if occluded by neighbor; otherwise <c>false</c>.</returns>
		private static bool IsFaceOccludedByNeighbor(
			Erelia.Core.VoxelKit.Cell[,,] cells,
			int sizeX,
			int sizeY,
			int sizeZ,
			int x,
			int y,
			int z,
			Erelia.Core.VoxelKit.Cell cell,
			Erelia.Core.VoxelKit.Face localFace,
			Erelia.Core.VoxelKit.AxisPlane nonOrientedPlane,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate,
			bool useCollision)
		{
			// Determine neighbor coordinates in the direction of the world plane.
			Vector3Int offset = Erelia.Core.VoxelKit.Utils.Geometry.PlaneToOffset(nonOrientedPlane);
			int nx = x + offset.x;
			int ny = y + offset.y;
			int nz = z + offset.z;

			// Out of bounds => no neighbor => cannot be occluded.
			if (nx < 0 || ny < 0 || nz < 0 || nx >= sizeX || ny >= sizeY || nz >= sizeZ)
			{
				return false;
			}

			// Resolve neighbor definition (and apply predicate).
			if (!TryGetDefinition(cells[nx, ny, nz], registry, predicate, out Erelia.Core.VoxelKit.Definition neighborDefinition, out Erelia.Core.VoxelKit.Cell neighborCell))
			{
				return false;
			}

			Erelia.Core.VoxelKit.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			// Choose which face set we test against (collision vs render).
			Erelia.Core.VoxelKit.Shape.FaceSet neighborFaceSet = useCollision ? neighborShape.CollisionFaces : neighborShape.RenderFaces;

			// We need the neighbor face that lies opposite to the tested plane.
			Erelia.Core.VoxelKit.AxisPlane oppositePlane = Erelia.Core.VoxelKit.Utils.Geometry.GetOppositePlane(nonOrientedPlane);

			// Convert opposite world plane into neighbor-local plane considering its orientation/flip.
			Erelia.Core.VoxelKit.AxisPlane neighborLocalPlane =
				Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborCell.Orientation, neighborCell.FlipOrientation);

			// Neighbor must provide a renderable outer face on that plane to occlude.
			if (neighborFaceSet.OuterShell == null
				|| !neighborFaceSet.OuterShell.TryGetValue(neighborLocalPlane, out Erelia.Core.VoxelKit.Face neighborLocalFace)
				|| neighborLocalFace == null
				|| !neighborLocalFace.HasRenderablePolygons)
			{
				return false;
			}

			// Transform both faces into world orientation before testing occlusion.
			Erelia.Core.VoxelKit.Face faceWorld = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
			Erelia.Core.VoxelKit.Face neighborWorld = TransformFaceCached(neighborLocalFace, neighborCell.Orientation, neighborCell.FlipOrientation);

			if (faceWorld == null || neighborWorld == null)
			{
				return false;
			}

			// Cache the expensive occlusion test result.
			if (Erelia.Core.VoxelKit.MesherUtils.FaceVsFaceOcclusionCache.TryGetValue(faceWorld, neighborWorld, out bool occluded))
			{
				return occluded;
			}

			return false;
		}

		/// <summary>
		/// Tests whether a “full face” on a plane is occluded by the neighbor voxel.
		/// </summary>
		/// <remarks>
		/// This is used when the current voxel does not define an outer face for a plane.
		/// It answers: “would a full face in that direction be blocked by the neighbor?”
		/// </remarks>
		private static bool IsFullFaceOccludedByNeighbor(
			Erelia.Core.VoxelKit.Cell[,,] cells,
			int sizeX,
			int sizeY,
			int sizeZ,
			int x,
			int y,
			int z,
			Erelia.Core.VoxelKit.AxisPlane nonOrientedPlane,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate,
			bool useCollision)
		{
			Vector3Int offset = Erelia.Core.VoxelKit.Utils.Geometry.PlaneToOffset(nonOrientedPlane);
			int nx = x + offset.x;
			int ny = y + offset.y;
			int nz = z + offset.z;

			if (nx < 0 || ny < 0 || nz < 0 || nx >= sizeX || ny >= sizeY || nz >= sizeZ)
			{
				return false;
			}

			if (!TryGetDefinition(cells[nx, ny, nz], registry, predicate, out Erelia.Core.VoxelKit.Definition neighborDefinition, out Erelia.Core.VoxelKit.Cell neighborCell))
			{
				return false;
			}

			Erelia.Core.VoxelKit.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			Erelia.Core.VoxelKit.Shape.FaceSet neighborFaceSet = useCollision ? neighborShape.CollisionFaces : neighborShape.RenderFaces;

			Erelia.Core.VoxelKit.AxisPlane oppositePlane = Erelia.Core.VoxelKit.Utils.Geometry.GetOppositePlane(nonOrientedPlane);
			Erelia.Core.VoxelKit.AxisPlane neighborLocalPlane =
				Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborCell.Orientation, neighborCell.FlipOrientation);

			if (neighborFaceSet.OuterShell == null
				|| !neighborFaceSet.OuterShell.TryGetValue(neighborLocalPlane, out Erelia.Core.VoxelKit.Face neighborLocalFace)
				|| neighborLocalFace == null
				|| !neighborLocalFace.HasRenderablePolygons)
			{
				return false;
			}

			// Transform neighbor face into world.
			Erelia.Core.VoxelKit.Face neighborWorld =
				TransformFaceCached(neighborLocalFace, neighborCell.Orientation, neighborCell.FlipOrientation);

			if (neighborWorld == null)
			{
				return false;
			}

			// Use a canonical “full face” as the test face.
			Erelia.Core.VoxelKit.Face fullFace = Erelia.Core.VoxelKit.Utils.Geometry.FullOuterFaces[(int)oppositePlane];
			if (fullFace == null)
			{
				return false;
			}

			// First try cached occlusion result (fullFace vs neighborWorld).
			if (Erelia.Core.VoxelKit.MesherUtils.FaceVsFaceOcclusionCache.TryGetValue(fullFace, neighborWorld, out bool occluded))
			{
				return occluded;
			}

			// Fallback: if neighbor is itself a full face on that plane, treat as occluding.
			return Erelia.Core.VoxelKit.Utils.Geometry.IsFullFace(neighborWorld, oppositePlane);
		}
	}
}