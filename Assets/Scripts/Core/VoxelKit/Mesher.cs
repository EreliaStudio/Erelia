using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.VoxelKit
{
	static class Mesher
	{
		public static readonly Func<Erelia.Core.VoxelKit.Definition, bool> AnyVoxelPredicate = (definition) => true;
		public static readonly Func<Erelia.Core.VoxelKit.Definition, bool> OnlyObstacleVoxelPredicate = (definition) => definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Obstacle;
		public static readonly Func<Erelia.Core.VoxelKit.Definition, bool> OnlyWalkableVoxelPredicate = (definition) => definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Walkable;

		private const float MergeEpsilon = 0.001f;

		private const int MinBoxVolume = 2;

		static public UnityEngine.Mesh BuildRenderMesh(
			Erelia.Core.VoxelKit.Cell[,,] cells,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate)
		{
			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			var uvs = new List<Vector2>();

			if (cells == null || registry == null)
			{
				return new UnityEngine.Mesh();
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						if (!TryGetDefinition(cells[x, y, z], registry, predicate, out Erelia.Core.VoxelKit.Definition definition, out Erelia.Core.VoxelKit.Cell cell))
						{
							continue;
						}

						Erelia.Core.VoxelKit.Shape shape = definition.Shape;
						if (shape == null)
						{
							continue;
						}

						Erelia.Core.VoxelKit.Shape.FaceSet faceSet = shape.RenderFaces;
						Vector3 offset = new Vector3(x, y, z);

						bool anyOuterVisible = false;
						for (int i = 0; i < Erelia.Core.VoxelKit.Shape.AxisPlanes.Length; i++)
						{
							Erelia.Core.VoxelKit.Shape.AxisPlane worldPlane = Erelia.Core.VoxelKit.Shape.AxisPlanes[i];
							Erelia.Core.VoxelKit.Shape.AxisPlane localPlane = Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(worldPlane, cell.Orientation, cell.FlipOrientation);

							if (faceSet.OuterShell != null
								&& faceSet.OuterShell.TryGetValue(localPlane, out Erelia.Core.VoxelKit.Face localFace)
								&& localFace != null
								&& localFace.HasRenderablePolygons)
							{
								if (!IsFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, cell, localFace, worldPlane, registry, predicate, useCollision: false))
								{
									Erelia.Core.VoxelKit.Face transformed = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
									AddFace(transformed, offset, vertices, triangles, uvs);
									anyOuterVisible = true;
								}
							}
							else
							{
								if (!IsFullFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, worldPlane, registry, predicate, useCollision: false))
								{
									anyOuterVisible = true;
								}
							}
						}

						if (anyOuterVisible && faceSet.Inner != null)
						{
							for (int i = 0; i < faceSet.Inner.Count; i++)
							{
								Erelia.Core.VoxelKit.Face innerFace = TransformFaceCached(faceSet.Inner[i], cell.Orientation, cell.FlipOrientation);
								AddFace(innerFace, offset, vertices, triangles, uvs);
							}
						}
					}
				}
			}

			var result = new UnityEngine.Mesh();
			if (vertices.Count >= 65535)
			{
				result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			result.SetVertices(vertices);
			result.SetTriangles(triangles, 0);
			result.SetUVs(0, uvs);
			result.RecalculateNormals();
			result.RecalculateBounds();
			return result;
		}

		// --------------------------------------------------------------------
		// COLLISION MESH (reworked)
		//
		// One single resulting mesh:
		// Pass 1:
		//   - Find largest cubic boxes (same ID), mark "consumed"
		//   - Emit greedy rectangles for the surface of consumed volume
		//     (occlusion uses SOLID field, not consumed, to avoid internal faces later)
		// Pass 2:
		//   - Emit remaining (non-consumed) voxels with your current
		//     face occlusion logic, without polygon merge.
		// --------------------------------------------------------------------
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

			BuildSolidAndCubicMaps(
				cells,
				registry,
				predicate,
				out bool[,,] solid,
				out bool[,,] cubic,
				out int[,,] ids);

			var consumed = new bool[sizeX, sizeY, sizeZ];

			ConsumeLargestCubicBoxes(ids, solid, cubic, consumed, sizeX, sizeY, sizeZ);

			// Single resulting mesh.
			var vertices = new List<Vector3>();
			var triangles = new List<int>();

			EmitGreedySurfaceForConsumed(consumed, solid, sizeX, sizeY, sizeZ, vertices, triangles);

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

		private static bool IsFaceSetEmpty(Erelia.Core.VoxelKit.Shape.FaceSet faceSet)
		{
			bool hasOuter = faceSet.OuterShell != null && faceSet.OuterShell.Count > 0;
			bool hasInner = faceSet.Inner != null && faceSet.Inner.Count > 0;
			return !hasOuter && !hasInner;
		}

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
						ids[x, y, z] = -1;

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

						if (IsFaceSetEmpty(fs))
						{
							continue;
						}

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

						if (!hasAnyRenderable)
						{
							continue;
						}

						solid[x, y, z] = true;
						ids[x, y, z] = cell.Id;

						cubic[x, y, z] = IsCubicCollisionVoxel(cell, shape);
					}
				}
			}
		}

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

			for (int i = 0; i < Erelia.Core.VoxelKit.Shape.AxisPlanes.Length; i++)
			{
				Erelia.Core.VoxelKit.Shape.AxisPlane worldPlane = Erelia.Core.VoxelKit.Shape.AxisPlanes[i];
				Erelia.Core.VoxelKit.Shape.AxisPlane localPlane = Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(worldPlane, cell.Orientation, cell.FlipOrientation);

				if (!fs.OuterShell.TryGetValue(localPlane, out Erelia.Core.VoxelKit.Face localFace)
					|| localFace == null
					|| !localFace.HasRenderablePolygons)
				{
					return false;
				}

				Erelia.Core.VoxelKit.Face worldFace = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
				if (!Erelia.Core.VoxelKit.Utils.Geometry.IsFullFace(worldFace, worldPlane))
				{
					return false;
				}
			}

			return true;
		}

		private struct Box
		{
			public int X;
			public int Y;
			public int Z;
			public int SX;
			public int SY;
			public int SZ;

			public int Volume => SX * SY * SZ;
		}

		private static void ConsumeLargestCubicBoxes(
			int[,,] ids,
			bool[,,] solid,
			bool[,,] cubic,
			bool[,,] consumed,
			int sizeX,
			int sizeY,
			int sizeZ)
		{
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
							if (!solid[x, y, z] || !cubic[x, y, z] || consumed[x, y, z])
							{
								continue;
							}

							int id = ids[x, y, z];
							if (id < 0)
							{
								continue;
							}

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

				if (bestVol < MinBoxVolume)
				{
					break;
				}

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
			best = new Box { X = x0, Y = y0, Z = z0, SX = 1, SY = 1, SZ = 1 };

			if (!CellOk(x0, y0, z0, id, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
			{
				return false;
			}

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
			while (true)
			{
				int nsx = sx, nsy = sy, nsz = sz;
				if (axis == 0) nsx++;
				else if (axis == 1) nsy++;
				else nsz++;

				if (!ValidateBoxLayer(x0, y0, z0, id, sx, sy, sz, nsx, nsy, nsz, axis, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
				{
					break;
				}

				sx = nsx;
				sy = nsy;
				sz = nsz;
			}
		}

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

		private static bool CellOk(
			int x, int y, int z,
			int id,
			int[,,] ids,
			bool[,,] solid,
			bool[,,] cubic,
			bool[,,] consumed,
			int sizeX, int sizeY, int sizeZ)
		{
			if (x < 0 || y < 0 || z < 0 || x >= sizeX || y >= sizeY || z >= sizeZ)
			{
				return false;
			}

			if (!solid[x, y, z] || consumed[x, y, z] || !cubic[x, y, z])
			{
				return false;
			}

			return ids[x, y, z] == id;
		}

		private static void EmitGreedySurfaceForConsumed(
			bool[,,] consumed,
			bool[,,] solid,
			int sizeX, int sizeY, int sizeZ,
			List<Vector3> vertices,
			List<int> triangles)
		{
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
						if (x == sizeX - 1) return true;
						return !solid[x + 1, y, z];
					},
					emitRect: (u0, v0, du, dv) =>
					{
						float xp = x + 1f;
						float y0 = u0;
						float y1 = u0 + du;
						float z0 = v0;
						float z1 = v0 + dv;

						Vector3 p0 = new Vector3(xp, y0, z0);
						Vector3 p1 = new Vector3(xp, y1, z0);
						Vector3 p2 = new Vector3(xp, y1, z1);
						Vector3 p3 = new Vector3(xp, y0, z1);
						AddQuad(p0, p1, p2, p3, Vector3.right, vertices, triangles);
					});
			}

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
					if (used[u, v] || !isFilled(u, v))
					{
						continue;
					}

					int w = 1;
					while (v + w < vSize && !used[u, v + w] && isFilled(u, v + w))
					{
						w++;
					}

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

					for (int du = 0; du < h; du++)
					{
						for (int dv = 0; dv < w; dv++)
						{
							used[u + du, v + dv] = true;
						}
					}

					emitRect(u, v, h, w);
				}
			}
		}

		private static void AddQuad(
			Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
			Vector3 expectedNormal,
			List<Vector3> vertices,
			List<int> triangles)
		{
			int start = vertices.Count;

			Vector3 n = Vector3.Cross(p1 - p0, p2 - p0);
			if (Vector3.Dot(n, expectedNormal) < 0f)
			{
				Vector3 tmp = p1;
				p1 = p3;
				p3 = tmp;
			}

			vertices.Add(p0);
			vertices.Add(p1);
			vertices.Add(p2);
			vertices.Add(p3);

			triangles.Add(start + 0);
			triangles.Add(start + 1);
			triangles.Add(start + 2);

			triangles.Add(start + 0);
			triangles.Add(start + 2);
			triangles.Add(start + 3);
		}

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
						if (!solid[x, y, z] || consumed[x, y, z])
						{
							continue;
						}

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
						for (int i = 0; i < Erelia.Core.VoxelKit.Shape.AxisPlanes.Length; i++)
						{
							Erelia.Core.VoxelKit.Shape.AxisPlane worldPlane = Erelia.Core.VoxelKit.Shape.AxisPlanes[i];
							Erelia.Core.VoxelKit.Shape.AxisPlane localPlane = Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(worldPlane, cell.Orientation, cell.FlipOrientation);

							if (faceSet.OuterShell != null
								&& faceSet.OuterShell.TryGetValue(localPlane, out Erelia.Core.VoxelKit.Face localFace)
								&& localFace != null
								&& localFace.HasRenderablePolygons)
							{
								if (!IsFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, cell, localFace, worldPlane, registry, predicate, useCollision: true))
								{
									Erelia.Core.VoxelKit.Face transformed = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
									AddFaceTrianglesOnly(transformed, offset, vertices, triangles);
									anyOuterVisible = true;
								}
							}
							else
							{
								if (!IsFullFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, worldPlane, registry, predicate, useCollision: true))
								{
									anyOuterVisible = true;
								}
							}
						}

						if (anyOuterVisible && faceSet.Inner != null)
						{
							for (int i = 0; i < faceSet.Inner.Count; i++)
							{
								Erelia.Core.VoxelKit.Face innerFace = TransformFaceCached(faceSet.Inner[i], cell.Orientation, cell.FlipOrientation);
								AddFaceTrianglesOnly(innerFace, offset, vertices, triangles);
							}
						}
					}
				}
			}
		}

		private static void AddFaceTrianglesOnly(
			Erelia.Core.VoxelKit.Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Erelia.Core.VoxelKit.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Erelia.Core.VoxelKit.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Erelia.Core.VoxelKit.Face.Vertex v = faceVertices[i];
					vertices.Add(positionOffset + v.Position);
				}

				for (int i = 1; i < faceVertices.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}
		}

		private static bool TryGetDefinition(
			Erelia.Core.VoxelKit.Cell cell,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate,
			out Erelia.Core.VoxelKit.Definition definition,
			out Erelia.Core.VoxelKit.Cell resolvedCell)
		{
			definition = null;
			resolvedCell = cell;
			if (cell == null || cell.Id < 0 || registry == null)
			{
				return false;
			}

			if (!registry.TryGet(cell.Id, out definition))
			{
				return false;
			}

			if (predicate != null && !predicate(definition))
			{
				return false;
			}

			return true;
		}

		private static Erelia.Core.VoxelKit.Face TransformFaceCached(
			Erelia.Core.VoxelKit.Face face,
			Erelia.Core.VoxelKit.Orientation orientation,
			Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			if (face == null)
			{
				return null;
			}

			if (Erelia.Core.VoxelKit.MesherUtils.FaceByOrientationCache.TryGetValue(face, orientation, flipOrientation, out Erelia.Core.VoxelKit.Face output))
			{
				return output;
			}

			return face;
		}

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

			List<List<Erelia.Core.VoxelKit.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Erelia.Core.VoxelKit.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Erelia.Core.VoxelKit.Face.Vertex vertex = faceVertices[i];
					vertices.Add(positionOffset + vertex.Position);
					uvs.Add(vertex.TileUV);
				}

				for (int i = 1; i < faceVertices.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}
		}

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
			Erelia.Core.VoxelKit.Shape.AxisPlane worldPlane,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate,
			bool useCollision)
		{
			Vector3Int offset = Erelia.Core.VoxelKit.Utils.Geometry.PlaneToOffset(worldPlane);
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

			Erelia.Core.VoxelKit.Shape.AxisPlane oppositePlane = Erelia.Core.VoxelKit.Utils.Geometry.GetOppositePlane(worldPlane);
			Erelia.Core.VoxelKit.Shape.AxisPlane neighborLocalPlane = Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborCell.Orientation, neighborCell.FlipOrientation);

			if (neighborFaceSet.OuterShell == null
				|| !neighborFaceSet.OuterShell.TryGetValue(neighborLocalPlane, out Erelia.Core.VoxelKit.Face neighborLocalFace)
				|| neighborLocalFace == null
				|| !neighborLocalFace.HasRenderablePolygons)
			{
				return false;
			}

			Erelia.Core.VoxelKit.Face faceWorld = TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation);
			Erelia.Core.VoxelKit.Face neighborWorld = TransformFaceCached(neighborLocalFace, neighborCell.Orientation, neighborCell.FlipOrientation);

			if (faceWorld == null || neighborWorld == null)
			{
				return false;
			}

			if (Erelia.Core.VoxelKit.MesherUtils.FaceVsFaceOcclusionCache.TryGetValue(faceWorld, neighborWorld, out bool occluded))
			{
				return occluded;
			}

			return false;
		}

		private static bool IsFullFaceOccludedByNeighbor(
			Erelia.Core.VoxelKit.Cell[,,] cells,
			int sizeX,
			int sizeY,
			int sizeZ,
			int x,
			int y,
			int z,
			Erelia.Core.VoxelKit.Shape.AxisPlane worldPlane,
			Erelia.Core.VoxelKit.Registry registry,
			Func<Erelia.Core.VoxelKit.Definition, bool> predicate,
			bool useCollision)
		{
			Vector3Int offset = Erelia.Core.VoxelKit.Utils.Geometry.PlaneToOffset(worldPlane);
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

			Erelia.Core.VoxelKit.Shape.AxisPlane oppositePlane = Erelia.Core.VoxelKit.Utils.Geometry.GetOppositePlane(worldPlane);
			Erelia.Core.VoxelKit.Shape.AxisPlane neighborLocalPlane = Erelia.Core.VoxelKit.Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborCell.Orientation, neighborCell.FlipOrientation);

			if (neighborFaceSet.OuterShell == null
				|| !neighborFaceSet.OuterShell.TryGetValue(neighborLocalPlane, out Erelia.Core.VoxelKit.Face neighborLocalFace)
				|| neighborLocalFace == null
				|| !neighborLocalFace.HasRenderablePolygons)
			{
				return false;
			}

			Erelia.Core.VoxelKit.Face neighborWorld = TransformFaceCached(neighborLocalFace, neighborCell.Orientation, neighborCell.FlipOrientation);
			if (neighborWorld == null)
			{
				return false;
			}

			Erelia.Core.VoxelKit.Face fullFace = Erelia.Core.VoxelKit.Utils.Geometry.FullOuterFaces[(int)oppositePlane];
			if (fullFace == null)
			{
				return false;
			}

			if (Erelia.Core.VoxelKit.MesherUtils.FaceVsFaceOcclusionCache.TryGetValue(fullFace, neighborWorld, out bool occluded))
			{
				return occluded;
			}

			return Erelia.Core.VoxelKit.Utils.Geometry.IsFullFace(neighborWorld, oppositePlane);
		}
	}
}