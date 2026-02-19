using System;
using System.Collections.Generic;
using UnityEngine;
namespace Voxel
{
	static class Mesher
	{
		private static readonly Func<Voxel.Cell[,,], int, int, int, Voxel.Cell, Voxel.Definition, bool> DefaultPredicate = (cells, x, y, z, cell, definition) => true;

		private struct MeshBuffers
		{
			public readonly List<Vector3> Vertices;
			public readonly List<int> Triangles;
			public readonly List<Vector2> Uvs;

			public MeshBuffers(bool withUvs)
			{
				Vertices = new List<Vector3>();
				Triangles = new List<int>();
				Uvs = withUvs ? new List<Vector2>() : null;
			}

			public void ApplyTo(UnityEngine.Mesh mesh)
			{
				if (Vertices.Count == 0)
				{
					return;
				}

				mesh.SetVertices(Vertices);
				mesh.SetTriangles(Triangles, 0);
				if (Uvs != null)
				{
					mesh.SetUVs(0, Uvs);
				}
				mesh.RecalculateNormals();
				mesh.RecalculateBounds();
			}
		}

		static public bool TryBuild(
			Voxel.Cell[,,] cells,
			out UnityEngine.Mesh renderMesh,
			out UnityEngine.Mesh collisionMesh,
			Func<Voxel.Cell[,,], int, int, int, Voxel.Cell, Voxel.Definition, bool> predicate = null)
		{
			renderMesh = new UnityEngine.Mesh();
			collisionMesh = new UnityEngine.Mesh();

			if (cells == null)
			{
				return false;
			}

			Func<Voxel.Cell[,,], int, int, int, Voxel.Cell, Voxel.Definition, bool> filter = predicate ?? DefaultPredicate;
			var renderBuffers = new MeshBuffers(withUvs: true);
			var collisionBuffers = new MeshBuffers(withUvs: false);

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						if (!TryGetCell(cells, x, y, z, out Voxel.Cell cell))
						{
							continue;
						}

						if (!TryGetDefinition(cell, out Voxel.Definition definition))
						{
							continue;
						}

						if (!filter(cells, x, y, z, cell, definition))
						{
							continue;
						}

						AddVoxel(
							cells,
							x, y, z,
							cell,
							definition,
							filter,
							renderBuffers,
							collisionBuffers);
					}
				}
			}

			renderBuffers.ApplyTo(renderMesh);
			collisionBuffers.ApplyTo(collisionMesh);

			return true;
		}

		private static bool TryGetCell(Voxel.Cell[,,] cells, int x, int y, int z, out Voxel.Cell cell)
		{
			cell = null;
			if (cells == null ||
				x < 0 || x >= cells.GetLength(0) ||
				y < 0 || y >= cells.GetLength(1) ||
				z < 0 || z >= cells.GetLength(2))
			{
				return false;
			}

			cell = cells[x, y, z];
			return true;
		}

		private static bool TryGetDefinition(Voxel.Cell cell, out Voxel.Definition definition)
		{
			definition = null;
			if (cell == null || cell.Id < 0)
			{
				return false;
			}

			return Voxel.Registry.TryGet(cell.Id, out definition);
		}

		private static void AddVoxel(
			Voxel.Cell[,,] cells,
			int x,
			int y,
			int z,
			Voxel.Cell cell,
			Voxel.Definition definition,
			Func<Voxel.Cell[,,], int, int, int, Voxel.Cell, Voxel.Definition, bool> filter,
			MeshBuffers renderBuffers,
			MeshBuffers collisionBuffers)
		{
			Voxel.Shape shape = definition.Shape;
			if (shape == null)
			{
				return;
			}

			Voxel.Shape.FaceSet renderFaces = shape.RenderFaces;
			Voxel.Shape.FaceSet collisionFaces = shape.CollisionFaces;
			bool wantsCollision = definition.Data != null && definition.Data.Collision != Voxel.Collision.None;
			Vector3 position = new Vector3(x, y, z);
			bool anyOuterVisibleRender = false;
			bool anyOuterVisibleCollision = false;

			for (int i = 0; i < Voxel.Shape.AxisPlanes.Length; i++)
			{
				Voxel.Shape.AxisPlane plane = Voxel.Shape.AxisPlanes[i];
				TryAddOuterFaces(
					cells,
					cell,
					position,
					x,
					y,
					z,
					plane,
					renderFaces,
					collisionFaces,
					wantsCollision,
					filter,
					ref anyOuterVisibleRender,
					ref anyOuterVisibleCollision,
					renderBuffers,
					collisionBuffers);
			}

			if (anyOuterVisibleRender)
			{
				List<Voxel.Face> innerFaces = renderFaces.Inner;
				if (innerFaces != null)
				{
					for (int i = 0; i < innerFaces.Count; i++)
					{
						Voxel.Face face = TransformFaceCached(innerFaces[i], cell.Orientation, cell.FlipOrientation);
						AddFace(face, position, renderBuffers);
					}
				}
			}

			if (wantsCollision && anyOuterVisibleCollision)
			{
				List<Voxel.Face> innerFaces = collisionFaces.Inner;
				if (innerFaces != null)
				{
					for (int i = 0; i < innerFaces.Count; i++)
					{
						Voxel.Face face = TransformFaceCached(innerFaces[i], cell.Orientation, cell.FlipOrientation);
						AddFace(face, position, collisionBuffers);
					}
				}
			}
		}

		private static void TryAddOuterFaces(
			Voxel.Cell[,,] cells,
			Voxel.Cell cell,
			Vector3 position,
			int x,
			int y,
			int z,
			Voxel.Shape.AxisPlane plane,
			Voxel.Shape.FaceSet renderFaceSet,
			Voxel.Shape.FaceSet collisionFaceSet,
			bool wantsCollision,
			Func<Voxel.Cell[,,], int, int, int, Voxel.Cell, Voxel.Definition, bool> filter,
			ref bool anyOuterVisibleRender,
			ref bool anyOuterVisibleCollision,
			MeshBuffers renderBuffers,
			MeshBuffers collisionBuffers)
		{
			Vector3Int offset = Utils.Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;

			bool hasNeighbor = TryGetNeighbor(
				cells,
				neighborX,
				neighborY,
				neighborZ,
				filter,
				allowNeighborWithoutCollision: true,
				out Voxel.Cell neighborCell,
				out Voxel.Definition neighborDefinition,
				out Voxel.Shape neighborShape);

			Voxel.Shape.AxisPlane localPlane = Utils.Geometry.MapWorldPlaneToLocal(plane, cell.Orientation, cell.FlipOrientation);

			bool renderVisible = TryAddOuterFaceForPlane(
				renderFaceSet,
				localPlane,
				plane,
				cell,
				position,
				hasNeighbor,
				neighborCell,
				neighborShape,
				useRenderFaces: true,
				renderBuffers);
			anyOuterVisibleRender |= renderVisible;

			if (!wantsCollision)
			{
				return;
			}

			bool hasCollisionNeighbor = hasNeighbor
				&& neighborDefinition != null
				&& neighborDefinition.Data != null
				&& neighborDefinition.Data.Collision != Voxel.Collision.None;

			bool collisionVisible = TryAddOuterFaceForPlane(
				collisionFaceSet,
				localPlane,
				plane,
				cell,
				position,
				hasCollisionNeighbor,
				neighborCell,
				neighborShape,
				useRenderFaces: false,
				collisionBuffers);
			anyOuterVisibleCollision |= collisionVisible;
		}

		private static bool TryAddOuterFaceForPlane(
			Voxel.Shape.FaceSet faceSet,
			Voxel.Shape.AxisPlane localPlane,
			Voxel.Shape.AxisPlane worldPlane,
			Voxel.Cell cell,
			Vector3 position,
			bool hasNeighbor,
			Voxel.Cell neighborCell,
			Voxel.Shape neighborShape,
			bool useRenderFaces,
			MeshBuffers buffers)
		{
			if (faceSet.OuterShell == null
				|| !faceSet.OuterShell.TryGetValue(localPlane, out Voxel.Face face)
				|| face == null
				|| !face.HasRenderablePolygons)
			{
				return !IsFullyOccludedByNeighbor(worldPlane, hasNeighbor, neighborCell, neighborShape, useRenderFaces);
			}

			Voxel.Face rotatedFace = TransformFaceCached(face, cell.Orientation, cell.FlipOrientation);
			if (rotatedFace == null)
			{
				return false;
			}

			if (IsFaceOccludedByNeighbor(rotatedFace, worldPlane, hasNeighbor, neighborCell, neighborShape, useRenderFaces))
			{
				return false;
			}

			AddFace(rotatedFace, position, buffers);
			return true;
		}

		private static bool TryGetNeighbor(
			Voxel.Cell[,,] cells,
			int x,
			int y,
			int z,
			Func<Voxel.Cell[,,], int, int, int, Voxel.Cell, Voxel.Definition, bool> filter,
			bool allowNeighborWithoutCollision,
			out Voxel.Cell neighborCell,
			out Voxel.Definition neighborDefinition,
			out Voxel.Shape neighborShape)
		{
			neighborCell = null;
			neighborDefinition = null;
			neighborShape = null;

			if (!TryGetCell(cells, x, y, z, out neighborCell))
			{
				return false;
			}

			if (!TryGetDefinition(neighborCell, out neighborDefinition))
			{
				return false;
			}

			if (!filter(cells, x, y, z, neighborCell, neighborDefinition))
			{
				return false;
			}

			if (!allowNeighborWithoutCollision
				&& (neighborDefinition.Data == null || neighborDefinition.Data.Collision == Voxel.Collision.None))
			{
				return false;
			}

			neighborShape = neighborDefinition.Shape;
			return neighborShape != null;
		}

		private static bool IsFaceOccludedByNeighbor(
			Voxel.Face face,
			Voxel.Shape.AxisPlane plane,
			bool hasNeighbor,
			Voxel.Cell neighborCell,
			Voxel.Shape neighborShape,
			bool useRenderFaces)
		{
			if (!hasNeighbor || neighborShape == null)
			{
				return false;
			}

			if (!TryGetNeighborFace(neighborShape, neighborCell, plane, useRenderFaces, out Voxel.Face neighborFace))
			{
				return false;
			}

			return Mesherutils.FaceVsFaceOcclusionCache.TryGetValue(face, neighborFace, out bool occluded) && occluded;
		}

		private static bool IsFullyOccludedByNeighbor(
			Voxel.Shape.AxisPlane plane,
			bool hasNeighbor,
			Voxel.Cell neighborCell,
			Voxel.Shape neighborShape,
			bool useRenderFaces)
		{
			if (!hasNeighbor || neighborShape == null)
			{
				return false;
			}

			if (!TryGetNeighborFace(neighborShape, neighborCell, plane, useRenderFaces, out Voxel.Face neighborFace))
			{
				return false;
			}

			if (!Utils.Geometry.IsFaceCoplanarWithPlane(neighborFace, plane))
			{
				return false;
			}

			Voxel.Face fullFace = Utils.Geometry.GetFullOuterFace(plane);
			if (fullFace == null)
			{
				return false;
			}

			return Mesherutils.FaceVsFaceOcclusionCache.TryGetValue(fullFace, neighborFace, out bool occluded) && occluded;
		}

		private static bool TryGetNeighborFace(
			Voxel.Shape neighborShape,
			Voxel.Cell neighborCell,
			Voxel.Shape.AxisPlane plane,
			bool useRenderFaces,
			out Voxel.Face neighborFace)
		{
			neighborFace = null;
			if (neighborShape == null)
			{
				return false;
			}

			Voxel.Shape.FaceSet neighborFaces = useRenderFaces
				? neighborShape.RenderFaces
				: neighborShape.CollisionFaces;

			Voxel.Shape.AxisPlane oppositePlane = Utils.Geometry.GetOppositePlane(plane);
			Voxel.Shape.AxisPlane neighborLocalPlane = Utils.Geometry.MapWorldPlaneToLocal(
				oppositePlane,
				neighborCell.Orientation,
				neighborCell.FlipOrientation);

			if (neighborFaces.OuterShell == null
				|| !neighborFaces.OuterShell.TryGetValue(neighborLocalPlane, out Voxel.Face face)
				|| face == null)
			{
				return false;
			}

			neighborFace = TransformFaceCached(face, neighborCell.Orientation, neighborCell.FlipOrientation);
			return neighborFace != null;
		}

		private static void AddFace(
			Voxel.Face face,
			Vector3 positionOffset,
			MeshBuffers buffers)
		{
			AddFace(face, positionOffset, buffers.Vertices, buffers.Triangles, buffers.Uvs);
		}

		private static void AddFace(
			Voxel.Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Voxel.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Voxel.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Voxel.Face.Vertex vertex = faceVertices[i];
					vertices.Add(positionOffset + vertex.Position);
					if (uvs != null)
					{
						uvs.Add(vertex.TileUV);
					}
				}

				for (int i = 1; i < faceVertices.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}
		}

		private static Voxel.Face TransformFaceCached(Voxel.Face face, Voxel.Orientation orientation, Voxel.FlipOrientation flipOrientation)
		{
			if (face == null)
			{
				return null;
			}

			if (Mesherutils.FaceByOrientationCache.TryGetValue(face, orientation, flipOrientation, out Voxel.Face output))
			{
				return output;
			}

			return face;
		}
	}
}
