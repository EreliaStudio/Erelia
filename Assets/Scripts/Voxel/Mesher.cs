using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Voxel
{
	static class Mesher
	{
		public delegate bool Predicate(Cell[,,] cells, int x, int y, int z, Cell cell, Definition definition);

		private static readonly Predicate DefaultPredicate = (cells, x, y, z, cell, definition) => true;
		private static readonly Dictionary<Erelia.Voxel.Shape, bool> CubeCollisionCache = new Dictionary<Erelia.Voxel.Shape, bool>();

		public static Mesh BuildRenderMesh(Voxel.Cell[,,] pack, Predicate predicate)
		{
			Mesh result = new Mesh();
			if (pack == null)
			{
				return result;
			}

			predicate ??= DefaultPredicate;

			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			var uvs = new List<Vector2>();

			int sizeX = pack.GetLength(0);
			int sizeY = pack.GetLength(1);
			int sizeZ = pack.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						AddRenderVoxel(pack, predicate, x, y, z, vertices, triangles, uvs);
					}
				}
			}

			result.SetVertices(vertices);
			result.SetTriangles(triangles, 0);
			result.SetUVs(0, uvs);
			result.RecalculateNormals();
			result.RecalculateBounds();

			//Build the render mesh for the whole voxel pack. No need to split it into multiples separated meshes if they are concaves, as render doesn't care about convex state

			return result;
		}

		private static void AddGreedyCubeCollision(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			List<Vector3> vertices,
			List<int> triangles)
		{
			int sizeX = pack.GetLength(0);
			int sizeY = pack.GetLength(1);
			int sizeZ = pack.GetLength(2);
			int[] dims = { sizeX, sizeY, sizeZ };

			bool IsSolid(int x, int y, int z)
			{
				if (x < 0 || y < 0 || z < 0 || x >= sizeX || y >= sizeY || z >= sizeZ)
				{
					return false;
				}

				if (!TryGetCell(pack, x, y, z, out Cell cell) || cell == null || cell.Id < 0)
				{
					return false;
				}

				if (!TryGetDefinition(cell, out Definition definition))
				{
					return false;
				}

				if (!predicate(pack, x, y, z, cell, definition))
				{
					return false;
				}

				return IsCubeCollisionShape(definition.Shape);
			}

			var x = new int[3];
			var q = new int[3];

			for (int d = 0; d < 3; d++)
			{
				int u = (d + 1) % 3;
				int v = (d + 2) % 3;
				q[0] = 0;
				q[1] = 0;
				q[2] = 0;
				q[d] = 1;

				int maskWidth = dims[u];
				int maskHeight = dims[v];
				int[] mask = new int[maskWidth * maskHeight];

				for (x[d] = -1; x[d] < dims[d]; x[d]++)
				{
					int n = 0;
					for (x[v] = 0; x[v] < dims[v]; x[v]++)
					{
						for (x[u] = 0; x[u] < dims[u]; x[u]++)
						{
							bool a = x[d] >= 0 && IsSolid(x[0], x[1], x[2]);
							bool b = x[d] < dims[d] - 1 && IsSolid(x[0] + q[0], x[1] + q[1], x[2] + q[2]);
							mask[n++] = a == b ? 0 : (a ? 1 : -1);
						}
					}

					n = 0;
					for (int j = 0; j < maskHeight; j++)
					{
						for (int i = 0; i < maskWidth;)
						{
							int c = mask[n];
							if (c == 0)
							{
								i++;
								n++;
								continue;
							}

							int w = 1;
							while (i + w < maskWidth && mask[n + w] == c)
							{
								w++;
							}

							int h = 1;
							bool done = false;
							while (j + h < maskHeight && !done)
							{
								for (int k = 0; k < w; k++)
								{
									if (mask[n + k + h * maskWidth] != c)
									{
										done = true;
										break;
									}
								}
								if (!done)
								{
									h++;
								}
							}

							x[u] = i;
							x[v] = j;
							var basePos = new Vector3(x[0], x[1], x[2]);
							if (c > 0)
							{
								basePos[d] += 1f;
							}

							var du = Vector3.zero;
							var dv = Vector3.zero;
							du[u] = w;
							dv[v] = h;

							AddQuad(vertices, triangles, basePos, du, dv, flip: c < 0);

							for (int l = 0; l < h; l++)
							{
								for (int k = 0; k < w; k++)
								{
									mask[n + k + l * maskWidth] = 0;
								}
							}

							i += w;
							n += w;
						}
					}
				}
			}
		}

		private static void AddNonCubeCollisionFaces(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			List<Vector3> vertices,
			List<int> triangles)
		{
			int sizeX = pack.GetLength(0);
			int sizeY = pack.GetLength(1);
			int sizeZ = pack.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						if (!TryGetCell(pack, x, y, z, out Cell cell))
						{
							continue;
						}

						if (cell.Id < 0)
						{
							continue;
						}

						if (!TryGetDefinition(cell, out Definition definition))
						{
							continue;
						}

						if (!predicate(pack, x, y, z, cell, definition))
						{
							continue;
						}

						Erelia.Voxel.Shape shape = definition.Shape;
						if (shape == null || IsCubeCollisionShape(shape))
						{
							continue;
						}

						Erelia.Voxel.Orientation orientation = cell.Orientation;
						Erelia.Voxel.FlipOrientation flipOrientation = cell.FlipOrientation;
						Vector3 position = new Vector3(x, y, z);
						bool anyOuterVisible = false;

						for (int i = 0; i < Erelia.Voxel.Shape.AxisPlanes.Length; i++)
						{
							Erelia.Voxel.Shape.AxisPlane plane = Erelia.Voxel.Shape.AxisPlanes[i];
							TryAddOuterCollisionFaceMesh(
								pack,
								predicate,
								shape,
								orientation,
								flipOrientation,
								position,
								x,
								y,
								z,
								plane,
								ref anyOuterVisible,
								vertices,
								triangles);
						}

						if (!anyOuterVisible)
						{
							continue;
						}

						List<Erelia.Voxel.Face> innerFaces = shape.CollisionFaces.Inner;
						if (innerFaces == null)
						{
							continue;
						}

						for (int i = 0; i < innerFaces.Count; i++)
						{
							Face rotated = TransformFaceCached(innerFaces[i], orientation, flipOrientation);
							AddFaceCollision(rotated, position, vertices, triangles);
						}
					}
				}
			}
		}

		private static void TryAddOuterCollisionFaceMesh(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			Erelia.Voxel.Shape shape,
			Erelia.Voxel.Orientation orientation,
			Erelia.Voxel.FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Erelia.Voxel.Shape.AxisPlane plane,
			ref bool anyOuterVisible,
			List<Vector3> vertices,
			List<int> triangles)
		{
			Vector3Int offset = Utils.Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			Erelia.Voxel.Shape.AxisPlane localPlane = Utils.Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);
			Dictionary<Erelia.Voxel.Shape.AxisPlane, Erelia.Voxel.Face> outerShellFaces = shape.CollisionFaces.OuterShell;
			if (outerShellFaces == null || !outerShellFaces.TryGetValue(localPlane, out Face face) || face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullFaceOccludedByNeighborCollision(pack, predicate, neighborX, neighborY, neighborZ, plane))
				{
					anyOuterVisible = true;
				}
				return;
			}

			Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (rotatedFace == null)
			{
				return;
			}

			if (IsFaceOccludedByNeighborCollision(rotatedFace, pack, predicate, neighborX, neighborY, neighborZ, plane))
			{
				return;
			}

			AddFaceCollision(rotatedFace, position, vertices, triangles);
			anyOuterVisible = true;
		}

		public static Mesh BuildCollisionMesh(Voxel.Cell[,,] pack, Predicate predicate)
		{
			if (pack == null)
			{
				return new Mesh();
			}

			predicate ??= DefaultPredicate;

			var vertices = new List<Vector3>();
			var triangles = new List<int>();

			AddGreedyCubeCollision(pack, predicate, vertices, triangles);
			AddNonCubeCollisionFaces(pack, predicate, vertices, triangles);

			if (vertices.Count == 0 || triangles.Count == 0)
			{
				return new Mesh();
			}

			var mesh = new Mesh { name = "CollisionMesh" };
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}

		private static void AddRenderVoxel(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			int x,
			int y,
			int z,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (!TryGetCell(pack, x, y, z, out Cell cell))
			{
				return;
			}

			if (cell.Id < 0)
			{
				return;
			}

			if (!TryGetDefinition(cell, out Definition definition))
			{
				return;
			}

			if (!predicate(pack, x, y, z, cell, definition))
			{
				return;
			}

			Erelia.Voxel.Shape shape = definition.Shape;
			if (shape == null)
			{
				return;
			}

			Erelia.Voxel.Orientation orientation = cell.Orientation;
			Erelia.Voxel.FlipOrientation flipOrientation = cell.FlipOrientation;
			Vector3 position = new Vector3(x, y, z);
			bool anyOuterVisible = false;

			for (int i = 0; i < Erelia.Voxel.Shape.AxisPlanes.Length; i++)
			{
				Erelia.Voxel.Shape.AxisPlane plane = Erelia.Voxel.Shape.AxisPlanes[i];
				TryAddOuterRenderFace(
					pack,
					shape,
					orientation,
					flipOrientation,
					position,
					x,
					y,
					z,
					plane,
					ref anyOuterVisible,
					vertices,
					triangles,
					uvs);
			}

			if (!anyOuterVisible)
			{
				return;
			}

			List<Erelia.Voxel.Face> innerFaces = shape.RenderFaces.Inner;
			if (innerFaces == null)
			{
				return;
			}

			for (int i = 0; i < innerFaces.Count; i++)
			{
				AddFace(TransformFaceCached(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
			}
		}

		private static void TryAddOuterRenderFace(
			Voxel.Cell[,,] pack,
			Erelia.Voxel.Shape shape,
			Erelia.Voxel.Orientation orientation,
			Erelia.Voxel.FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Erelia.Voxel.Shape.AxisPlane plane,
			ref bool anyOuterVisible,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			Vector3Int offset = Utils.Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			bool hasNeighbor = TryGetCell(pack, neighborX, neighborY, neighborZ, out Cell neighborCell);
			Definition neighborDefinition = null;
			if (hasNeighbor && !TryGetDefinition(neighborCell, out neighborDefinition))
			{
				hasNeighbor = false;
			}

			Erelia.Voxel.Shape neighborShape = hasNeighbor ? neighborDefinition.Shape : null;
			Erelia.Voxel.Shape.AxisPlane localPlane = Utils.Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			Dictionary<Erelia.Voxel.Shape.AxisPlane, Erelia.Voxel.Face> outerShellFaces = shape.RenderFaces.OuterShell;
			if (outerShellFaces == null || !outerShellFaces.TryGetValue(localPlane, out Face face) || face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullyOccludedByNeighborRender(pack, neighborX, neighborY, neighborZ, plane))
				{
					anyOuterVisible = true;
				}
				return;
			}

			Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (rotatedFace == null)
			{
				return;
			}

			bool isOccluded = false;
			if (hasNeighbor && neighborShape != null)
			{
				Erelia.Voxel.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
				Erelia.Voxel.Orientation neighborOrientation = neighborCell != null ? neighborCell.Orientation : Erelia.Voxel.Orientation.PositiveX;
				Erelia.Voxel.FlipOrientation neighborFlipOrientation = neighborCell != null ? neighborCell.FlipOrientation : Erelia.Voxel.FlipOrientation.PositiveY;
				Erelia.Voxel.Shape.AxisPlane neighborLocalPlane = Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
				Dictionary<Erelia.Voxel.Shape.AxisPlane, Erelia.Voxel.Face> neighborOuterFaces = neighborShape.RenderFaces.OuterShell;
				if (neighborOuterFaces != null && neighborOuterFaces.TryGetValue(neighborLocalPlane, out Face otherFace))
				{
					Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
					isOccluded = IsFaceOccluded(rotatedFace, rotatedOtherFace);
				}
			}

			if (isOccluded)
			{
				return;
			}

			AddFace(rotatedFace, position, vertices, triangles, uvs);
			anyOuterVisible = true;
		}

		private static bool IsFullyOccludedByNeighborRender(
			Voxel.Cell[,,] pack,
			int neighborX,
			int neighborY,
			int neighborZ,
			Erelia.Voxel.Shape.AxisPlane plane)
		{
			if (!TryGetCell(pack, neighborX, neighborY, neighborZ, out Cell neighborCell))
			{
				return false;
			}

			if (neighborCell == null || neighborCell.Id < 0)
			{
				return false;
			}

			if (!TryGetDefinition(neighborCell, out Definition neighborDefinition))
			{
				return false;
			}

			Erelia.Voxel.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			Erelia.Voxel.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
			Erelia.Voxel.Orientation neighborOrientation = neighborCell.Orientation;
			Erelia.Voxel.FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Erelia.Voxel.Shape.AxisPlane neighborLocalPlane = Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (neighborShape.RenderFaces.OuterShell == null
				|| !neighborShape.RenderFaces.OuterShell.TryGetValue(neighborLocalPlane, out Face otherFace))
			{
				return false;
			}

			Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			if (!Utils.Geometry.IsFaceCoplanarWithPlane(rotatedOtherFace, plane))
			{
				return false;
			}

			Face fullFace = Utils.Geometry.GetFullOuterFace(plane);
			return fullFace != null && fullFace.IsOccludedBy(rotatedOtherFace);
		}


		private static bool IsFaceOccludedByNeighborCollision(
			Face face,
			Voxel.Cell[,,] pack,
			Predicate predicate,
			int neighborX,
			int neighborY,
			int neighborZ,
			Erelia.Voxel.Shape.AxisPlane plane)
		{
			if (!TryGetOccludingNeighborFaceCollision(pack, predicate, neighborX, neighborY, neighborZ, plane, out Face neighborFace))
			{
				return false;
			}

			return neighborFace != null && IsFaceOccluded(face, neighborFace);
		}

		private static bool IsFullFaceOccludedByNeighborCollision(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			int neighborX,
			int neighborY,
			int neighborZ,
			Erelia.Voxel.Shape.AxisPlane plane)
		{
			if (!TryGetOccludingNeighborFaceCollision(pack, predicate, neighborX, neighborY, neighborZ, plane, out Face neighborFace))
			{
				return false;
			}

			if (neighborFace == null)
			{
				return false;
			}

			Erelia.Voxel.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
			if (!Utils.Geometry.IsFaceCoplanarWithPlane(neighborFace, oppositePlane))
			{
				return false;
			}

			Face fullFace = Utils.Geometry.GetFullOuterFace(oppositePlane);
			return fullFace != null && fullFace.IsOccludedBy(neighborFace);
		}

		private static bool TryGetOccludingNeighborFaceCollision(
			Voxel.Cell[,,] pack,
			Predicate predicate,
			int neighborX,
			int neighborY,
			int neighborZ,
			Erelia.Voxel.Shape.AxisPlane plane,
			out Face neighborFace)
		{
			neighborFace = null;

			if (!TryGetCell(pack, neighborX, neighborY, neighborZ, out Cell neighborCell))
			{
				return false;
			}

			if (neighborCell == null || neighborCell.Id < 0)
			{
				return false;
			}

			if (!TryGetDefinition(neighborCell, out Definition neighborDefinition))
			{
				return false;
			}

			if (!predicate(pack, neighborX, neighborY, neighborZ, neighborCell, neighborDefinition))
			{
				return false;
			}

			Erelia.Voxel.Shape neighborShape = neighborDefinition.Shape;
			if (neighborShape == null)
			{
				return false;
			}

			Erelia.Voxel.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
			Erelia.Voxel.Orientation neighborOrientation = neighborCell.Orientation;
			Erelia.Voxel.FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Erelia.Voxel.Shape.AxisPlane neighborLocalPlane = Utils.Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			Dictionary<Erelia.Voxel.Shape.AxisPlane, Erelia.Voxel.Face> neighborOuterFaces = neighborShape.CollisionFaces.OuterShell;
			if (neighborOuterFaces == null || !neighborOuterFaces.TryGetValue(neighborLocalPlane, out Face otherFace))
			{
				return false;
			}

			neighborFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			return neighborFace != null;
		}

		private static void AddFace(
			Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Face.Vertex>> facePolygons = face.Polygons;

			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Face.Vertex vertex = faceVertices[i];
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

		private static void AddFaceCollision(
			Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					vertices.Add(positionOffset + faceVertices[i].Position);
				}

				for (int i = 1; i < faceVertices.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}
		}

		private static void AddQuad(
			List<Vector3> vertices,
			List<int> triangles,
			Vector3 origin,
			Vector3 du,
			Vector3 dv,
			bool flip)
		{
			int start = vertices.Count;
			vertices.Add(origin);
			vertices.Add(origin + du);
			vertices.Add(origin + du + dv);
			vertices.Add(origin + dv);

			if (flip)
			{
				triangles.Add(start);
				triangles.Add(start + 2);
				triangles.Add(start + 1);

				triangles.Add(start);
				triangles.Add(start + 3);
				triangles.Add(start + 2);
			}
			else
			{
				triangles.Add(start);
				triangles.Add(start + 1);
				triangles.Add(start + 2);

				triangles.Add(start);
				triangles.Add(start + 2);
				triangles.Add(start + 3);
			}
		}


		private static bool TryGetCell(Voxel.Cell[,,] cellPack, int x, int y, int z, out Voxel.Cell cell)
		{
			cell = null;
			if (cellPack == null ||
				x < 0 || x >= cellPack.GetLength(0) ||
				y < 0 || y >= cellPack.GetLength(1) ||
				z < 0 || z >= cellPack.GetLength(2))
			{
				return false;
			}

			cell = cellPack[x, y, z];
			return true;
		}

		private static bool TryGetDefinition(Voxel.Cell cell, out Voxel.Definition definition)
		{
			definition = null;
			if (cell == null)
			{
				return false;
			}

			return Voxel.Registry.TryGet(cell.Id, out definition);
		}

		private static Face TransformFaceCached(Face face, Erelia.Voxel.Orientation orientation, Erelia.Voxel.FlipOrientation flipOrientation)
		{
			if (face == null)
			{
				return null;
			}

			if (Mesherutils.FaceByOrientationCache.TryGetValue(face, orientation, flipOrientation, out Face output))
			{
				return output;
			}

			return face;
		}

		private static bool IsFaceOccluded(Face face, Face occluder)
		{
			if (face == null || occluder == null)
			{
				return false;
			}

			if (Mesherutils.FaceVsFaceOcclusionCache.TryGetValue(face, occluder, out bool isOccluded))
			{
				return isOccluded;
			}

			return face.IsOccludedBy(occluder);
		}

		private static bool IsCubeCollisionShape(Erelia.Voxel.Shape shape)
		{
			if (shape == null)
			{
				return false;
			}

			if (CubeCollisionCache.TryGetValue(shape, out bool cached))
			{
				return cached;
			}

			bool isCube = false;
			List<Erelia.Voxel.Face> inner = shape.CollisionFaces.Inner;
			Dictionary<Erelia.Voxel.Shape.AxisPlane, Erelia.Voxel.Face> outer = shape.CollisionFaces.OuterShell;
			if ((inner == null || inner.Count == 0) && outer != null && outer.Count == Erelia.Voxel.Shape.AxisPlanes.Length)
			{
				isCube = true;
				for (int i = 0; i < Erelia.Voxel.Shape.AxisPlanes.Length; i++)
				{
					Erelia.Voxel.Shape.AxisPlane plane = Erelia.Voxel.Shape.AxisPlanes[i];
					if (!outer.TryGetValue(plane, out Face face) || !Utils.Geometry.IsFullFace(face, plane))
					{
						isCube = false;
						break;
					}
				}
			}

			CubeCollisionCache[shape] = isCube;
			return isCube;
		}
	}
}


