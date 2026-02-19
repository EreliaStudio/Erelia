using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Core.Utils.Mesher
{
	public class VoxelRenderMesher : Utils.Mesher.Mesher
	{
		[NonSerialized] private readonly List<Vector3> vertices = new List<Vector3>();
		[NonSerialized] private readonly List<int> triangles = new List<int>();
		[NonSerialized] private readonly List<Vector2> uvs = new List<Vector2>();
		public Mesh BuildMesh(Core.Voxel.Model.Cell[,,] cells)
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

		private void AddVoxel(Core.Voxel.Model.Cell[,,] cells, int x, int y, int z)
		{
			if (!TryGetCell(cells, x, y, z, out Core.Voxel.Model.Cell cell))
			{
				return;
			}

			if (cell.Id == Core.Voxel.Service.AirID)
			{
				return;
			}

			if (!TryGetDefinition(cell, out Core.Voxel.Model.Definition definition))
			{
				return;
			}

			Core.Voxel.Geometry.Shape shape = definition.Shape;
			if (shape == null)
			{
				return;
			}

			Core.Voxel.Model.Orientation orientation = cell.Orientation;
			Core.Voxel.Model.FlipOrientation flipOrientation = cell.FlipOrientation;
			Vector3 position = new Vector3(x, y, z);
			bool anyOuterVisible = false;

			foreach (Core.Voxel.Geometry.Shape.AxisPlane plane in Core.Voxel.Geometry.Shape.AxisPlanes)
			{
				TryAddOuterFace(cells, shape, orientation, flipOrientation, position, x, y, z, plane, ref anyOuterVisible);
			}

			if (anyOuterVisible)
			{
				IReadOnlyList<Core.Voxel.Model.Face> innerFaces = shape.InnerFaces;
				if (innerFaces == null)
				{
					return;
				}

				for (int i = 0; i < innerFaces.Count; i++)
				{
					AddFace(TransformFaceCached(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
				}
			}
		}

		private void TryAddOuterFace(
			Core.Voxel.Model.Cell[,,] cells,
			Core.Voxel.Geometry.Shape shape,
			Core.Voxel.Model.Orientation orientation,
			Core.Voxel.Model.FlipOrientation flipOrientation,
			Vector3 position,
			int x,
			int y,
			int z,
			Core.Voxel.Geometry.Shape.AxisPlane plane,
			ref bool anyOuterVisible)
		{
			Vector3Int offset = Geometry.PlaneToOffset(plane);
			int neighborX = x + offset.x;
			int neighborY = y + offset.y;
			int neighborZ = z + offset.z;
			bool hasNeighbor = TryGetCell(cells, neighborX, neighborY, neighborZ, out Core.Voxel.Model.Cell neighborCell);
			Core.Voxel.Model.Definition neighborDefinition = null;
			if (hasNeighbor && !TryGetDefinition(neighborCell, out neighborDefinition))
			{
				hasNeighbor = false;
			}
			Core.Voxel.Geometry.Shape neighborShape = hasNeighbor ? neighborDefinition.Shape : null;

			Core.Voxel.Geometry.Shape.AxisPlane localPlane = Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			IReadOnlyDictionary<Core.Voxel.Geometry.Shape.AxisPlane, Core.Voxel.Model.Face> outerShellFaces = shape.OuterShellFaces;
			if (outerShellFaces == null || !outerShellFaces.TryGetValue(localPlane, out Core.Voxel.Model.Face face) || face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullyOccludedByNeighbor(cells, neighborShape, neighborX, neighborY, neighborZ, plane, hasNeighbor))
				{
					anyOuterVisible = true;
				}
				return;
			}

			bool isOccluded = false;
			Core.Voxel.Model.Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (rotatedFace == null)
			{
				return;
			}
			if (hasNeighbor && neighborShape != null)
			{
				Core.Voxel.Geometry.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
				Core.Voxel.Model.Orientation neighborOrientation = neighborCell != null ? neighborCell.Orientation : Core.Voxel.Model.Orientation.PositiveX;
				Core.Voxel.Model.FlipOrientation neighborFlipOrientation = neighborCell != null ? neighborCell.FlipOrientation : Core.Voxel.Model.FlipOrientation.PositiveY;
				Core.Voxel.Geometry.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
				IReadOnlyDictionary<Core.Voxel.Geometry.Shape.AxisPlane, Core.Voxel.Model.Face> neighborOuterFaces = neighborShape.OuterShellFaces;
				if (neighborOuterFaces != null && neighborOuterFaces.TryGetValue(neighborLocalPlane, out Core.Voxel.Model.Face otherFace))
				{
					Core.Voxel.Model.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
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
			Core.Voxel.Model.Cell[,,] cells,
			Core.Voxel.Geometry.Shape neighborShape,
			int neighborX,
			int neighborY,
			int neighborZ,
			Core.Voxel.Geometry.Shape.AxisPlane plane,
			bool hasNeighbor)
		{
			if (!hasNeighbor || neighborShape == null)
			{
				return false;
			}

			if (!TryGetCell(cells, neighborX, neighborY, neighborZ, out Core.Voxel.Model.Cell neighborCell) || neighborCell == null)
			{
				return false;
			}

			Core.Voxel.Geometry.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
			Core.Voxel.Model.Orientation neighborOrientation = neighborCell.Orientation;
			Core.Voxel.Model.FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Core.Voxel.Geometry.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (!neighborShape.OuterShellFaces.TryGetValue(neighborLocalPlane, out Core.Voxel.Model.Face otherFace))
			{
				return false;
			}

			Core.Voxel.Model.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			if (!Geometry.IsFaceCoplanarWithPlane(rotatedOtherFace, plane))
			{
				return false;
			}

			Core.Voxel.Model.Face fullFace = Geometry.GetFullOuterFace(plane);
			return fullFace != null && fullFace.IsOccludedBy(rotatedOtherFace);
		}
	
		public static Mesh Build(Core.Voxel.Model.Cell[,,] cells)
		{
			return new VoxelRenderMesher().BuildMesh(cells);
		}
	}
}
