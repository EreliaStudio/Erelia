using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Core.Utils.Mesher
{
	public class VoxelMaskRenderMesher : Utils.Mesher.Mesher
	{
		[NonSerialized] private readonly List<Vector3> vertices = new List<Vector3>();
		[NonSerialized] private readonly List<int> triangles = new List<int>();
		[NonSerialized] private readonly List<Vector2> uvs = new List<Vector2>();
		public Mesh BuildMesh(Core.Voxel.Model.Cell[,,] cells, Core.Mask.Model.Cell[,,] maskCells)
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
						AddVoxel(cells, maskCells, x, y, z);
					}
				}
			}

			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.SetUVs(0, uvs);
			mesh.RecalculateNormals();

			return mesh;
		}

		private void AddVoxel(Core.Voxel.Model.Cell[,,] cells, Core.Mask.Model.Cell[,,] maskCells, int x, int y, int z)
		{
			if (!TryGetCell(cells, x, y, z, out Core.Voxel.Model.Cell cell))
			{
				return;
			}

			if (cell.Id == Core.Voxel.Service.AirID)
			{
				return;
			}

			if (!TryGetMaskCell(maskCells, x, y, z, out Core.Mask.Model.Cell maskCell) || maskCell == null)
			{
				return;
			}

			if (!maskCell.HasAnyMask())
			{
				return;
			}

			if (!TryGetMaskSprite(maskCell, out Sprite maskSprite))
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
			IReadOnlyDictionary<Core.Voxel.Model.FlipOrientation, List<Core.Voxel.Model.Face>> maskFaces = shape.MaskFaces;
			if (maskFaces == null || maskFaces.Count == 0)
			{
				return;
			}

			if (!maskFaces.TryGetValue(flipOrientation, out List<Core.Voxel.Model.Face> maskFacesForFlip))
			{
				maskFaces.TryGetValue(Core.Voxel.Model.FlipOrientation.PositiveY, out maskFacesForFlip);
			}
			if (maskFacesForFlip == null || maskFacesForFlip.Count == 0)
			{
				return;
			}

			for (int i = 0; i < maskFacesForFlip.Count; i++)
			{
				Core.Voxel.Model.Face rotatedFace = TransformFaceCached(maskFacesForFlip[i], orientation, Core.Voxel.Model.FlipOrientation.PositiveY);
				AddFaceWithSprite(rotatedFace, position, maskSprite, vertices, triangles, uvs);
			}
		}

		private bool TryGetMaskSprite(Core.Mask.Model.Cell maskCell, out Sprite sprite)
		{
			sprite = null;
			if (maskCell == null || maskCell.Masks == null || maskCell.Masks.Count == 0)
			{
				return false;
			}

			Core.Mask.Model.Value maskValue = maskCell.Masks[maskCell.Masks.Count - 1];
			if (ServiceLocator.Instance == null || ServiceLocator.Instance.MaskService == null)
			{
				return false;
			}

			return ServiceLocator.Instance.MaskService.TryGetSprite(maskValue, out sprite);
		}

		private void AddFaceWithSprite(
			Core.Voxel.Model.Face face,
			Vector3 positionOffset,
			Sprite sprite,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			Core.Utils.SpriteUv.GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
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
					uvs.Add(uvAnchor + Vector2.Scale(vertex.TileUV, uvSize));
				}

				for (int i = 1; i < faceVertices.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}
		}

		private bool TryGetMaskCell(Core.Mask.Model.Cell[,,] cellPack, int x, int y, int z, out Core.Mask.Model.Cell cell)
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
	
		public static Mesh Build(Core.Voxel.Model.Cell[,,] cells, Core.Mask.Model.Cell[,,] maskCells)
		{
			return new VoxelMaskRenderMesher().BuildMesh(cells, maskCells);
		}
	}
}
