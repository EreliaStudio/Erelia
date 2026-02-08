using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace World.Chunk.View
{
	public class RenderMesher : World.Chunk.Core.Mesher
	{
		[NonSerialized] private readonly List<Vector3> vertices = new List<Vector3>();
		[NonSerialized] private readonly List<int> triangles = new List<int>();
		[NonSerialized] private readonly List<Vector2> uvs = new List<Vector2>();
		[NonSerialized] private int debugTotalCells = 0;
		[NonSerialized] private int debugAirCells = 0;
		[NonSerialized] private int debugMissingDefinition = 0;
		[NonSerialized] private int debugNullShape = 0;
		[NonSerialized] private int debugAnyOuterVisible = 0;

		public Mesh BuildMesh(World.Chunk.Model.Cell[,,] cells)
		{
			if (cells == null)
			{
				return new Mesh();
			}

			var mesh = new Mesh();
			vertices.Clear();
			triangles.Clear();
			uvs.Clear();
			ResetDebugCounters();

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

			if (vertices.Count == 0)
			{
				Debug.LogWarning(
					"RenderMesher: Empty mesh. " +
					"Cells=" + debugTotalCells +
					", Air=" + debugAirCells +
					", MissingDefinition=" + debugMissingDefinition +
					", NullShape=" + debugNullShape +
					", AnyOuterVisible=" + debugAnyOuterVisible
				);
			}

			return mesh;
		}

		private void ResetDebugCounters()
		{
			debugTotalCells = 0;
			debugAirCells = 0;
			debugMissingDefinition = 0;
			debugNullShape = 0;
			debugAnyOuterVisible = 0;
		}

		private void AddVoxel(World.Chunk.Model.Cell[,,] cells, int x, int y, int z)
		{
			debugTotalCells++;
			if (!TryGetCell(cells, x, y, z, out World.Chunk.Model.Cell cell))
			{
				return;
			}

			if (cell.Id == Voxel.Service.AirID)
			{
				debugAirCells++;
				return;
			}

			if (!TryGetDefinition(cell, out Voxel.Model.Definition definition))
			{
				debugMissingDefinition++;
				return;
			}

			Voxel.View.Shape shape = definition.Shape;
			if (shape == null)
			{
				debugNullShape++;
				return;
			}

			Voxel.Model.Orientation orientation = cell.Orientation;
			Voxel.Model.FlipOrientation flipOrientation = cell.FlipOrientation;
			Vector3 position = new Vector3(x, y, z);
			bool anyOuterVisible = false;

			foreach (Voxel.View.Shape.AxisPlane plane in Voxel.View.Shape.AxisPlanes)
			{
				TryAddOuterFace(cells, shape, orientation, flipOrientation, position, x, y, z, plane, ref anyOuterVisible);
			}

			if (anyOuterVisible)
			{
				debugAnyOuterVisible++;
				IReadOnlyList<Voxel.Model.Face> innerFaces = shape.InnerFaces;
				for (int i = 0; i < innerFaces.Count; i++)
				{
					AddFace(TransformFaceCached(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
				}
			}
		}

		private void TryAddOuterFace(
			World.Chunk.Model.Cell[,,] cells,
			Voxel.View.Shape shape,
			Voxel.Model.Orientation orientation,
			Voxel.Model.FlipOrientation flipOrientation,
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
			bool hasNeighbor = TryGetCell(cells, neighborX, neighborY, neighborZ, out World.Chunk.Model.Cell neighborCell);
			Voxel.Model.Definition neighborDefinition = null;
			if (hasNeighbor && !TryGetDefinition(neighborCell, out neighborDefinition))
			{
				hasNeighbor = false;
			}
			Voxel.View.Shape neighborShape = hasNeighbor ? neighborDefinition.Shape : null;

			Voxel.View.Shape.AxisPlane localPlane = Geometry.MapWorldPlaneToLocal(plane, orientation, flipOrientation);

			if (!shape.OuterShellFaces.TryGetValue(localPlane, out Voxel.Model.Face face) || face == null || !face.HasRenderablePolygons)
			{
				if (!IsFullyOccludedByNeighbor(cells, neighborShape, neighborX, neighborY, neighborZ, plane, hasNeighbor))
				{
					anyOuterVisible = true;
				}
				return;
			}

			bool isOccluded = false;
			Voxel.Model.Face rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
			if (hasNeighbor && neighborShape != null)
			{
				Voxel.View.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
				Voxel.Model.Orientation neighborOrientation = neighborCell != null ? neighborCell.Orientation : Voxel.Model.Orientation.PositiveX;
				Voxel.Model.FlipOrientation neighborFlipOrientation = neighborCell != null ? neighborCell.FlipOrientation : Voxel.Model.FlipOrientation.PositiveY;
				Voxel.View.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
				if (neighborShape.OuterShellFaces.TryGetValue(neighborLocalPlane, out Voxel.Model.Face otherFace))
				{
					Voxel.Model.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
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
			World.Chunk.Model.Cell[,,] cells,
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

			if (!TryGetCell(cells, neighborX, neighborY, neighborZ, out World.Chunk.Model.Cell neighborCell) || neighborCell == null)
			{
				return false;
			}

			Voxel.View.Shape.AxisPlane oppositePlane = Geometry.GetOppositePlane(plane);
			Voxel.Model.Orientation neighborOrientation = neighborCell.Orientation;
			Voxel.Model.FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			Voxel.View.Shape.AxisPlane neighborLocalPlane = Geometry.MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (!neighborShape.OuterShellFaces.TryGetValue(neighborLocalPlane, out Voxel.Model.Face otherFace))
			{
				return false;
			}

			Voxel.Model.Face rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
			if (!Geometry.IsFaceCoplanarWithPlane(rotatedOtherFace, plane))
			{
				return false;
			}

			Voxel.Model.Face fullFace = Geometry.GetFullOuterFace(plane);
			return fullFace != null && fullFace.IsOccludedBy(rotatedOtherFace);
		}
	
		public static Mesh Build(World.Chunk.Model.Cell[,,] cells)
		{
			return new RenderMesher().BuildMesh(cells);
		}
	}
}
