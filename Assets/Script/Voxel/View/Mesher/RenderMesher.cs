using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Voxel.View
{
	public class RenderMesher : Voxel.View.Mesher
	{
		[NonSerialized] private readonly List<Vector3> vertices = new List<Vector3>();
		[NonSerialized] private readonly List<int> triangles = new List<int>();
		[NonSerialized] private readonly List<Vector2> uvs = new List<Vector2>();

		public Mesh BuildMesh(World.Chunk.Cell[,,] cells)
		{
			if (cells == null)
			{
				return new Mesh();
			}

			var mesh = new Mesh();
			vertices.Clear();
			triangles.Clear();
			uvs.Clear();

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						AddVoxel(cells, x, y, z);
					}
				}
			}

			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.SetUVs(0, uvs);
			mesh.RecalculateNormals();

			return mesh;
		}

		private void AddVoxel(World.Chunk.Cell[,,] cells, int x, int y, int z)
		{
			if (!TryGetCell(cells, x, y, z, out World.Chunk.Cell cell))
			{
				return;
			}

			if (!TryGetDefinition(cell, out Voxel.Definition definition))
			{
				return;
			}

			Voxel.View.Shape shape = definition.Shape;
			if (shape == null)
			{
				return;
			}

			Orientation orientation = cell.Orientation;
			FlipOrientation flipOrientation = cell.FlipOrientation;
			Vector3 position = new Vector3(x, y, z);
			bool anyOuterVisible = false;

			foreach (Voxel.View.Shape.AxisPlane plane in Voxel.View.Shape.AxisPlanes)
			{
				TryAddOuterFace(cells, shape, orientation, flipOrientation, position, x, y, z, plane, ref anyOuterVisible);
			}

			if (anyOuterVisible)
			{
				IReadOnlyList<Voxel.View.Face> innerFaces = shape.InnerFaces;
				for (int i = 0; i < innerFaces.Count; i++)
				{
					AddFace(TransformFaceCached(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
				}
			}
		}

		private void TryAddOuterFace(
			World.Chunk.Cell[,,] cells,
			Voxel.View.Shape shape,
			Orientation orientation,
			FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Voxel.View.Shape.AxisPlane plane,
			ref bool anyOuterVisible)
		{
			Vector3Int offset = Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;
			bool hasNeighbor = TryGetCell(cells, neighborX, neighborY, neighborZ, out World.Chunk.Cell neighborCell);
			Voxel.Definition neighborDefinition = null;
			if (hasNeighbor && !TryGetDefinition(neighborCell, out neighborDefinition))
			{
				hasNeighbor = false;
			}
			Voxel.View.Shape neighborShape = hasNeighbor ? neighborDefinition.Shape : null;
			if (neighborShape != null)
			{
				neighborShape.EnsureBuilt();
			}

			Voxel.View.Shape.AxisPlane localPlane = Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			if (!shape.OuterShellFaces.TryGetValue(localPlane, out Voxel.View.Face face) || face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullyOccludedByNeighbor(cells, neighborShape, neighborX, neighborY, neighborZ, plane, hasNeighbor))
				{
					anyOuterVisible = true;
				}
				return;
			}

			bool isOccluded = false;
			Voxel.View.Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (hasNeighbor && neighborShape != null)
			{
				Voxel.View.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
				Orientation neighborOrientation = neighborCell != null ? neighborCell.Orientation : Orientation.PositiveX;
				FlipOrientation neighborFlipOrientation = neighborCell != null ? neighborCell.FlipOrientation : FlipOrientation.PositiveY;
				Voxel.View.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
				if (neighborShape.OuterShellFaces.TryGetValue(neighborLocalPlane, out Voxel.View.Face otherFace))
				{
					Voxel.View.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
					if (rotatedFace.IsOccludedBy(rotatedOtherFace))
					{
						isOccluded = true;
					}
				}
			}

			if (isOccluded)
			{
				return;
			}

			AddFace(rotatedFace, position, vertices, triangles, uvs);
			anyOuterVisible = true;
		}

		private bool IsFullyOccludedByNeighbor(
			World.Chunk.Cell[,,] cells,
			Voxel.View.Shape neighborShape,
			int neighborX,
			int neighborY,
			int neighborZ,
			Voxel.View.Shape.AxisPlane plane,
			bool hasNeighbor)
		{
			if (!hasNeighbor || neighborShape == null)
			{
				return false;
			}

			if (!TryGetCell(cells, neighborX, neighborY, neighborZ, out World.Chunk.Cell neighborCell) || neighborCell == null)
			{
				return false;
			}

			Voxel.View.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
			Orientation neighborOrientation = neighborCell.Orientation;
			FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Voxel.View.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (!neighborShape.OuterShellFaces.TryGetValue(neighborLocalPlane, out Voxel.View.Face otherFace))
			{
				return false;
			}

			Voxel.View.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			if (!Geometry.IsFaceCoplanarWithPlane(rotatedOtherFace, plane))
			{
				return false;
			}

			Voxel.View.Face fullFace = Geometry.GetFullOuterFace(plane);
			return fullFace != null && fullFace.IsOccludedBy(rotatedOtherFace);
		}
	}
}
