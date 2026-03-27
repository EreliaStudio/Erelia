using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle.Voxel
{
	public static class Mesher
	{
		public static Mesh BuildMaskMesh(
			Erelia.Battle.Voxel.Cell[,,] cells,
			Erelia.Core.Voxel.VoxelRegistry registry,
			Erelia.Battle.MaskSpriteRegistry maskSpriteRegistry)
		{
			var vertices = new List<Vector3>();
			var uvs = new List<Vector2>();
			var triangles = new List<int>();

			if (cells == null || registry == null)
			{
				return new Mesh();
			}

			int typeCount = System.Enum.GetValues(typeof(Erelia.Battle.Voxel.Mask.Type)).Length;
			var uvAnchors = new Vector2[typeCount];
			var uvSizes = new Vector2[typeCount];
			var hasUv = new bool[typeCount];

			if (maskSpriteRegistry != null)
			{
				for (int i = 0; i < typeCount; i++)
				{
					Erelia.Battle.Voxel.Mask.Type type = (Erelia.Battle.Voxel.Mask.Type)i;
					if (maskSpriteRegistry.TryGetSprite(type, out Sprite sprite) && sprite != null)
					{
						Erelia.Core.Voxel.Utils.SpriteUv.GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
						uvAnchors[i] = uvAnchor;
						uvSizes[i] = uvSize;
						hasUv[i] = true;
					}
				}
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						Erelia.Battle.Voxel.Cell maskCell = cells[x, y, z];
						if (!TryGetDefinition(maskCell, registry, out Erelia.Core.Voxel.VoxelDefinition definition, out Erelia.Core.Voxel.Cell cell))
						{
							continue;
						}

						if (!TryGetTopmostMask(maskCell, out Erelia.Battle.Voxel.Mask.Type topmost))
						{
							continue;
						}

						Erelia.Battle.Voxel.Mask.Shape maskShape = definition.MaskShape;
						if (maskShape == null || maskShape.MaskFaces == null)
						{
							continue;
						}

						if (!maskShape.MaskFaces.TryGetValue(cell.FlipOrientation, out List<Erelia.Core.Voxel.Face> faces) || faces == null)
						{
							continue;
						}

						Vector3 offset = new Vector3(x, y, z);
						int typeIndex = (int)topmost;
						if (typeIndex < 0 || typeIndex >= typeCount)
						{
							continue;
						}

						for (int i = 0; i < faces.Count; i++)
						{
							Erelia.Core.Voxel.Face transformed = TransformFaceCached(faces[i], cell.Orientation);
							AddFace(
								transformed,
								offset,
								vertices,
								triangles,
								uvs,
								hasUv[typeIndex],
								uvAnchors[typeIndex],
								uvSizes[typeIndex]);
						}
					}
				}
			}

			var result = new Mesh();
			if (vertices.Count >= 65535)
			{
				result.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			result.SetVertices(vertices);
			result.SetUVs(0, uvs);
			result.SetTriangles(triangles, 0);
			result.RecalculateNormals();
			result.RecalculateBounds();
			return result;
		}

		private static bool TryGetTopmostMask(
			Erelia.Battle.Voxel.Cell maskCell,
			out Erelia.Battle.Voxel.Mask.Type topmost)
		{
			topmost = default;
			if (maskCell == null || maskCell.Masks == null || maskCell.Masks.Count == 0)
			{
				return false;
			}

			topmost = maskCell.Masks[maskCell.Masks.Count - 1];
			return true;
		}

		private static bool TryGetDefinition(
			Erelia.Core.Voxel.Cell cell,
			Erelia.Core.Voxel.VoxelRegistry registry,
			out Erelia.Core.Voxel.VoxelDefinition definition,
			out Erelia.Core.Voxel.Cell resolvedCell)
		{
			definition = null;
			resolvedCell = cell;
			if (cell == null || cell.Id < 0 || registry == null)
			{
				return false;
			}

			if (!registry.TryGet(cell.Id, out definition))
			{
				return false;
			}

			return true;
		}

		private static Erelia.Core.Voxel.Face TransformFaceCached(Erelia.Core.Voxel.Face face, Erelia.Core.Voxel.Orientation orientation)
		{
			if (face == null)
			{
				return null;
			}

			if (Erelia.Core.Voxel.MesherUtils.FaceByOrientationCache.TryGetValue(
					face,
					orientation,
					Erelia.Core.Voxel.FlipOrientation.PositiveY,
					out Erelia.Core.Voxel.Face output))
			{
				return output;
			}

			return face;
		}

		private static void AddFace(
			Erelia.Core.Voxel.Face face,
			Vector3 positionOffset,
			List<Vector3> vertices,
			List<int> triangles,
			List<Vector2> uvs,
			bool hasUvTransform,
			Vector2 uvAnchor,
			Vector2 uvSize)
		{
			if (face == null || face.Polygons == null || face.Polygons.Count == 0)
			{
				return;
			}

			List<List<Erelia.Core.Voxel.Face.Vertex>> facePolygons = face.Polygons;
			for (int p = 0; p < facePolygons.Count; p++)
			{
				List<Erelia.Core.Voxel.Face.Vertex> faceVertices = facePolygons[p];
				if (faceVertices == null || faceVertices.Count < 3)
				{
					continue;
				}

				int start = vertices.Count;
				for (int i = 0; i < faceVertices.Count; i++)
				{
					Erelia.Core.Voxel.Face.Vertex vertex = faceVertices[i];
					vertices.Add(positionOffset + vertex.Position);
					Vector2 uv = vertex.TileUV;
					if (hasUvTransform)
					{
						uv = uvAnchor + Vector2.Scale(uv, uvSize);
					}
					uvs.Add(uv);
				}

				for (int i = 1; i < faceVertices.Count - 1; i++)
				{
					triangles.Add(start);
					triangles.Add(start + i + 1);
					triangles.Add(start + i);
				}
			}
		}
	}
}



