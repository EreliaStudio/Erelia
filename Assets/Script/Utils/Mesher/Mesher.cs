using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace Utils.Mesher
{
	public class Mesher
	{
		private static readonly Voxel.Model.FaceByOrientationCollection transformedFaceCache = new Voxel.Model.FaceByOrientationCollection();

		protected bool TryGetCell(World.Chunk.Model.Cell[,,] cellPack, int x, int y, int z, out World.Chunk.Model.Cell cell)
		{
			cell = default;
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

		protected bool TryGetDefinition(World.Chunk.Model.Cell cell, out Voxel.Model.Definition definition)
		{
			definition = null;
			if (ServiceLocator.Instance.VoxelService.TryGetDefinition(cell.Id, out Voxel.Model.Definition output))
			{
				definition = output;
				return true;
			}
			return false;
		}

		protected void AddFace(
			Voxel.Model.Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Voxel.Model.Face.Vertex>> facePolygons = face.Polygons;

			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Voxel.Model.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Voxel.Model.Face.Vertex vertex = faceVertices[i];
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

		protected Voxel.Model.Face TransformFaceCached(Voxel.Model.Face face, Voxel.Model.Orientation orientation, Voxel.Model.FlipOrientation flipOrientation)
		{
			if (face == null)
			{
				return null;
			}

			if (transformedFaceCache.TryGetValue(face, orientation, flipOrientation, out Voxel.Model.Face output))
			{
				return output;
			}

			return face;
		}
	}
}
