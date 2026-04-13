using System.Collections.Generic;
using UnityEngine;

public static partial class VoxelMesher
{
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

	private static Mesh BuildColliderMeshInternal(VoxelCell[,,] cells, VoxelRegistry voxelRegistry, VoxelTraversal expectedVoxelTraversal)
	{
		if (cells == null || voxelRegistry == null)
		{
			return new Mesh();
		}

		int sizeX = cells.GetLength(0);
		int sizeY = cells.GetLength(1);
		int sizeZ = cells.GetLength(2);

		BuildSolidAndCubicMaps(cells, voxelRegistry, expectedVoxelTraversal, out bool[,,] solid, out bool[,,] cubic, out int[,,] ids);

		var consumed = new bool[sizeX, sizeY, sizeZ];
		ConsumeLargestCubicBoxes(ids, solid, cubic, consumed, sizeX, sizeY, sizeZ);

		var vertices = new List<Vector3>();
		var triangles = new List<int>();

		EmitGreedySurfaceForConsumed(consumed, solid, sizeX, sizeY, sizeZ, vertices, triangles);
		EmitRemainingCollisionFaces(cells, voxelRegistry, solid, consumed, sizeX, sizeY, sizeZ, vertices, triangles);

		return BuildMesh(vertices, triangles, null);
	}

	private static void BuildSolidAndCubicMaps(VoxelCell[,,] cells, VoxelRegistry voxelRegistry, VoxelTraversal expectedVoxelTraversal, out bool[,,] solid, out bool[,,] cubic, out int[,,] ids)
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

					if (!TryGetVoxelDefinition(cells[x, y, z], voxelRegistry, out VoxelDefinition voxelDefinition, out VoxelCell cell))
					{
						continue;
					}

					if (voxelDefinition.Data.Traversal != expectedVoxelTraversal)
					{
						continue;
					}

					VoxelShape.FaceSet faceSet = voxelDefinition.Shape?.Collision;
					if (faceSet == null || !faceSet.HasAnyRenderableFaces)
					{
						faceSet = voxelDefinition.Shape?.Render;
					}
					if (IsFaceSetEmpty(faceSet))
					{
						continue;
					}

					solid[x, y, z] = true;
					ids[x, y, z] = cell.Id;
					cubic[x, y, z] = IsCubicCollisionVoxel(cell, faceSet);
				}
			}
		}
	}

	private static bool IsCubicCollisionVoxel(VoxelCell cell, VoxelShape.FaceSet faceSet)
	{
		if (cell == null || faceSet == null)
		{
			return false;
		}

		for (int i = 0; i < 6; i++)
		{
			VoxelAxisPlane worldPlane = (VoxelAxisPlane)i;
			VoxelAxisPlane localPlane = MapWorldPlaneToLocal(worldPlane, cell.Orientation, cell.FlipOrientation);

			if (!faceSet.TryGetOuterFace(localPlane, out VoxelShape.Face localFace) ||
				localFace == null ||
				!localFace.HasRenderablePolygons)
			{
				return false;
			}

			if (!IsFullFace(TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation), worldPlane))
			{
				return false;
			}
		}

		return true;
	}

	private static void ConsumeLargestCubicBoxes(int[,,] ids, bool[,,] solid, bool[,,] cubic, bool[,,] consumed, int sizeX, int sizeY, int sizeZ)
	{
		while (true)
		{
			Box best = default;
			int bestVolume = 0;

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						if (!solid[x, y, z] || !cubic[x, y, z] || consumed[x, y, z] || ids[x, y, z] < 0)
						{
							continue;
						}

						if (TryFindBestBoxAt(x, y, z, ids[x, y, z], ids, solid, cubic, consumed, sizeX, sizeY, sizeZ, out Box candidate) &&
							candidate.Volume > bestVolume)
						{
							best = candidate;
							bestVolume = candidate.Volume;
						}
					}
				}
			}

			if (bestVolume < 2)
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

	private static bool TryFindBestBoxAt(int x0, int y0, int z0, int id, int[,,] ids, bool[,,] solid, bool[,,] cubic, bool[,,] consumed, int sizeX, int sizeY, int sizeZ, out Box best)
	{
		best = new Box { X = x0, Y = y0, Z = z0, SX = 1, SY = 1, SZ = 1 };
		if (!CellOk(x0, y0, z0, id, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
		{
			return false;
		}

		int[][] permutations =
		{
			new[] { 0, 1, 2 },
			new[] { 0, 2, 1 },
			new[] { 1, 0, 2 },
			new[] { 1, 2, 0 },
			new[] { 2, 0, 1 },
			new[] { 2, 1, 0 }
		};

		int bestVolume = 1;

		for (int i = 0; i < permutations.Length; i++)
		{
			int sx = 1;
			int sy = 1;
			int sz = 1;

			GrowAxis(permutations[i][0], x0, y0, z0, id, ref sx, ref sy, ref sz, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ);
			GrowAxis(permutations[i][1], x0, y0, z0, id, ref sx, ref sy, ref sz, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ);
			GrowAxis(permutations[i][2], x0, y0, z0, id, ref sx, ref sy, ref sz, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ);

			int volume = sx * sy * sz;
			if (volume > bestVolume)
			{
				bestVolume = volume;
				best = new Box { X = x0, Y = y0, Z = z0, SX = sx, SY = sy, SZ = sz };
			}
		}

		return true;
	}

	private static void GrowAxis(int axis, int x0, int y0, int z0, int id, ref int sx, ref int sy, ref int sz, int[,,] ids, bool[,,] solid, bool[,,] cubic, bool[,,] consumed, int sizeX, int sizeY, int sizeZ)
	{
		while (true)
		{
			int nextSx = sx;
			int nextSy = sy;
			int nextSz = sz;

			if (axis == 0) nextSx++;
			else if (axis == 1) nextSy++;
			else nextSz++;

			if (!ValidateBoxLayer(x0, y0, z0, id, nextSx, nextSy, nextSz, axis, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
			{
				break;
			}

			sx = nextSx;
			sy = nextSy;
			sz = nextSz;
		}
	}

	private static bool ValidateBoxLayer(int x0, int y0, int z0, int id, int sx, int sy, int sz, int grownAxis, int[,,] ids, bool[,,] solid, bool[,,] cubic, bool[,,] consumed, int sizeX, int sizeY, int sizeZ)
	{
		if (grownAxis == 0)
		{
			int x = x0 + sx - 1;
			for (int y = y0; y < y0 + sy; y++)
			{
				for (int z = z0; z < z0 + sz; z++)
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
			int y = y0 + sy - 1;
			for (int x = x0; x < x0 + sx; x++)
			{
				for (int z = z0; z < z0 + sz; z++)
				{
					if (!CellOk(x, y, z, id, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
					{
						return false;
					}
				}
			}

			return true;
		}

		int planeZ = z0 + sz - 1;
		for (int x = x0; x < x0 + sx; x++)
		{
			for (int y = y0; y < y0 + sy; y++)
			{
				if (!CellOk(x, y, planeZ, id, ids, solid, cubic, consumed, sizeX, sizeY, sizeZ))
				{
					return false;
				}
			}
		}

		return true;
	}

	private static bool CellOk(int x, int y, int z, int id, int[,,] ids, bool[,,] solid, bool[,,] cubic, bool[,,] consumed, int sizeX, int sizeY, int sizeZ)
	{
		if (x < 0 || y < 0 || z < 0 || x >= sizeX || y >= sizeY || z >= sizeZ)
		{
			return false;
		}

		return solid[x, y, z] && !consumed[x, y, z] && cubic[x, y, z] && ids[x, y, z] == id;
	}

	private static void EmitGreedySurfaceForConsumed(bool[,,] consumed, bool[,,] solid, int sizeX, int sizeY, int sizeZ, List<Vector3> vertices, List<int> triangles)
	{
		for (int x = 0; x < sizeX; x++)
		{
			EmitGreedyMask2D(sizeY, sizeZ, (u, v) => consumed[x, u, v] && (x == sizeX - 1 || !solid[x + 1, u, v]), (u0, v0, du, dv) =>
				AddQuad(new Vector3(x + 1f, u0, v0), new Vector3(x + 1f, u0 + du, v0), new Vector3(x + 1f, u0 + du, v0 + dv), new Vector3(x + 1f, u0, v0 + dv), Vector3.right, vertices, triangles));
		}

		for (int x = 0; x < sizeX; x++)
		{
			EmitGreedyMask2D(sizeY, sizeZ, (u, v) => consumed[x, u, v] && (x == 0 || !solid[x - 1, u, v]), (u0, v0, du, dv) =>
				AddQuad(new Vector3(x, u0, v0 + dv), new Vector3(x, u0 + du, v0 + dv), new Vector3(x, u0 + du, v0), new Vector3(x, u0, v0), Vector3.left, vertices, triangles));
		}

		for (int y = 0; y < sizeY; y++)
		{
			EmitGreedyMask2D(sizeX, sizeZ, (u, v) => consumed[u, y, v] && (y == sizeY - 1 || !solid[u, y + 1, v]), (u0, v0, du, dv) =>
				AddQuad(new Vector3(u0, y + 1f, v0), new Vector3(u0 + du, y + 1f, v0), new Vector3(u0 + du, y + 1f, v0 + dv), new Vector3(u0, y + 1f, v0 + dv), Vector3.up, vertices, triangles));
		}

		for (int y = 0; y < sizeY; y++)
		{
			EmitGreedyMask2D(sizeX, sizeZ, (u, v) => consumed[u, y, v] && (y == 0 || !solid[u, y - 1, v]), (u0, v0, du, dv) =>
				AddQuad(new Vector3(u0, y, v0 + dv), new Vector3(u0 + du, y, v0 + dv), new Vector3(u0 + du, y, v0), new Vector3(u0, y, v0), Vector3.down, vertices, triangles));
		}

		for (int z = 0; z < sizeZ; z++)
		{
			EmitGreedyMask2D(sizeX, sizeY, (u, v) => consumed[u, v, z] && (z == sizeZ - 1 || !solid[u, v, z + 1]), (u0, v0, du, dv) =>
				AddQuad(new Vector3(u0, v0, z + 1f), new Vector3(u0 + du, v0, z + 1f), new Vector3(u0 + du, v0 + dv, z + 1f), new Vector3(u0, v0 + dv, z + 1f), Vector3.forward, vertices, triangles));
		}

		for (int z = 0; z < sizeZ; z++)
		{
			EmitGreedyMask2D(sizeX, sizeY, (u, v) => consumed[u, v, z] && (z == 0 || !solid[u, v, z - 1]), (u0, v0, du, dv) =>
				AddQuad(new Vector3(u0 + du, v0, z), new Vector3(u0, v0, z), new Vector3(u0, v0 + dv, z), new Vector3(u0 + du, v0 + dv, z), Vector3.back, vertices, triangles));
		}
	}

	private static void EmitGreedyMask2D(int uSize, int vSize, System.Func<int, int, bool> isFilled, System.Action<int, int, int, int> emitRectangle)
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

				int width = 1;
				while (v + width < vSize && !used[u, v + width] && isFilled(u, v + width))
				{
					width++;
				}

				int height = 1;
				bool canGrow = true;
				while (u + height < uSize && canGrow)
				{
					for (int k = 0; k < width; k++)
					{
						if (used[u + height, v + k] || !isFilled(u + height, v + k))
						{
							canGrow = false;
							break;
						}
					}

					if (canGrow)
					{
						height++;
					}
				}

				for (int du = 0; du < height; du++)
				{
					for (int dv = 0; dv < width; dv++)
					{
						used[u + du, v + dv] = true;
					}
				}

				emitRectangle(u, v, height, width);
			}
		}
	}

	private static void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 expectedNormal, List<Vector3> vertices, List<int> triangles)
	{
		int start = vertices.Count;
		Vector3 normal = Vector3.Cross(p1 - p0, p2 - p0);

		if (Vector3.Dot(normal, expectedNormal) < 0f)
		{
			Vector3 temp = p1;
			p1 = p3;
			p3 = temp;
		}

		vertices.Add(p0);
		vertices.Add(p1);
		vertices.Add(p2);
		vertices.Add(p3);

		triangles.Add(start);
		triangles.Add(start + 1);
		triangles.Add(start + 2);
		triangles.Add(start);
		triangles.Add(start + 2);
		triangles.Add(start + 3);
	}

	private static void EmitRemainingCollisionFaces(VoxelCell[,,] cells, VoxelRegistry voxelRegistry, bool[,,] solid, bool[,,] consumed, int sizeX, int sizeY, int sizeZ, List<Vector3> vertices, List<int> triangles)
	{
		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					if (!solid[x, y, z] || consumed[x, y, z] || !TryGetVoxelDefinition(cells[x, y, z], voxelRegistry, out VoxelDefinition voxelDefinition, out VoxelCell cell))
					{
						continue;
					}

					VoxelShape.FaceSet faceSet = voxelDefinition.Shape?.Collision;
					if (faceSet == null || !faceSet.HasAnyRenderableFaces)
					{
						faceSet = voxelDefinition.Shape?.Render;
					}
					if (IsFaceSetEmpty(faceSet))
					{
						continue;
					}

					Vector3 offset = new Vector3(x, y, z);
					bool anyOuterVisible = false;

					for (int i = 0; i < 6; i++)
					{
						VoxelAxisPlane worldPlane = (VoxelAxisPlane)i;
						VoxelAxisPlane localPlane = MapWorldPlaneToLocal(worldPlane, cell.Orientation, cell.FlipOrientation);

						if (faceSet.TryGetOuterFace(localPlane, out VoxelShape.Face localFace) && localFace != null && localFace.HasRenderablePolygons)
						{
							if (!IsFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, cell, localFace, worldPlane, voxelRegistry, true))
							{
								AddFaceTrianglesOnly(TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation), offset, vertices, triangles);
								anyOuterVisible = true;
							}
						}
						else if (!IsFullFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, worldPlane, voxelRegistry, true))
						{
							anyOuterVisible = true;
						}
					}

					if (anyOuterVisible && faceSet.InnerFaces != null)
					{
						for (int i = 0; i < faceSet.InnerFaces.Count; i++)
						{
							AddFaceTrianglesOnly(TransformFaceCached(faceSet.InnerFaces[i], cell.Orientation, cell.FlipOrientation), offset, vertices, triangles);
						}
					}
				}
			}
		}
	}
}
