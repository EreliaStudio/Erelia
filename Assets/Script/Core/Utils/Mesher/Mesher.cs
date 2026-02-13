using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace Core.Utils.Mesher
{
	public class Mesher
	{
		private static readonly Core.Voxel.Model.FaceByOrientationCollection transformedFaceCache = new Core.Voxel.Model.FaceByOrientationCollection();

		protected bool TryGetCell(Core.Voxel.Model.Cell[,,] cellPack, int x, int y, int z, out Core.Voxel.Model.Cell cell)
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

		protected bool TryGetDefinition(Core.Voxel.Model.Cell cell, out Core.Voxel.Model.Definition definition)
		{
			definition = null;
			if (ServiceLocator.Instance.VoxelService.TryGetDefinition(cell.Id, out Core.Voxel.Model.Definition output))
			{
				definition = output;
				return true;
			}
			return false;
		}

		protected void AddFace(
			Core.Voxel.Model.Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Core.Voxel.Model.Face.Vertex>> facePolygons = face.Polygons;

			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Core.Voxel.Model.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Core.Voxel.Model.Face.Vertex vertex = faceVertices[i];
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

		protected Core.Voxel.Model.Face TransformFaceCached(Core.Voxel.Model.Face face, Core.Voxel.Model.Orientation orientation, Core.Voxel.Model.FlipOrientation flipOrientation)
		{
			if (face == null)
			{
				return null;
			}

			if (transformedFaceCache.TryGetValue(face, orientation, flipOrientation, out Core.Voxel.Model.Face output))
			{
				return output;
			}

			return face;
		}
	}
}
