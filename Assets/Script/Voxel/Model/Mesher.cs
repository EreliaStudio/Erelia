using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace Voxel.Core
{
	public class Mesher
	{
		private static readonly Voxel.View.FaceByOrientationCollection transformedFaceCache = new Voxel.View.FaceByOrientationCollection();

		protected bool TryGetCell(World.Chunk.Cell[,,] cellPack, int x, int y, int z, out World.Chunk.Cell cell)
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

		protected bool TryGetDefinition(World.Chunk.Cell cell, out Voxel.Core.Definition definition)
		{
			definition = null;
			if (ServiceLocator.Instance.VoxelService.TryGetDefinition(cell.Id, out Voxel.Core.Definition output))
			{
				definition = output;
				return true;
			}
			return false;
		}

		protected void AddFace(
			Voxel.View.Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Voxel.View.Face.Vertex>> facePolygons = face.Polygons;

			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Voxel.View.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Voxel.View.Face.Vertex vertex = faceVertices[i];
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

		protected Voxel.View.Face TransformFaceCached(Voxel.View.Face face, Orientation orientation, FlipOrientation flipOrientation)
		{
			if (face == null)
			{
				return null;
			}

			if (transformedFaceCache.TryGetValue(face, orientation, flipOrientation, out Voxel.View.Face output))
			{
				return output;
			}

			return face;
		}
	}
}
