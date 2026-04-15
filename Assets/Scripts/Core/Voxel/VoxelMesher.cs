using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class VoxelMesher
{
	private const float MaskLayerOffset = 0.001f;

	private static readonly Dictionary<FaceTransformKey, VoxelShape.Face> FaceTransformCache = new Dictionary<FaceTransformKey, VoxelShape.Face>();
	private static readonly Dictionary<FaceOcclusionKey, bool> FaceOcclusionCache = new Dictionary<FaceOcclusionKey, bool>();

	private static readonly VoxelShape.Face[] FullOuterFaces =
	{
		CreateRectangleFace(
			new Vector3(1f, 0f, 0f), new Vector2(0f, 0f),
			new Vector3(1f, 0f, 1f), new Vector2(1f, 0f),
			new Vector3(1f, 1f, 1f), new Vector2(1f, 1f),
			new Vector3(1f, 1f, 0f), new Vector2(0f, 1f)),
		CreateRectangleFace(
			new Vector3(0f, 0f, 0f), new Vector2(0f, 0f),
			new Vector3(0f, 1f, 0f), new Vector2(1f, 0f),
			new Vector3(0f, 1f, 1f), new Vector2(1f, 1f),
			new Vector3(0f, 0f, 1f), new Vector2(0f, 1f)),
		CreateRectangleFace(
			new Vector3(0f, 1f, 0f), new Vector2(0f, 0f),
			new Vector3(1f, 1f, 0f), new Vector2(1f, 0f),
			new Vector3(1f, 1f, 1f), new Vector2(1f, 1f),
			new Vector3(0f, 1f, 1f), new Vector2(0f, 1f)),
		CreateRectangleFace(
			new Vector3(0f, 0f, 0f), new Vector2(0f, 0f),
			new Vector3(0f, 0f, 1f), new Vector2(1f, 0f),
			new Vector3(1f, 0f, 1f), new Vector2(1f, 1f),
			new Vector3(1f, 0f, 0f), new Vector2(0f, 1f)),
		CreateRectangleFace(
			new Vector3(0f, 0f, 1f), new Vector2(0f, 0f),
			new Vector3(0f, 1f, 1f), new Vector2(1f, 0f),
			new Vector3(1f, 1f, 1f), new Vector2(1f, 1f),
			new Vector3(1f, 0f, 1f), new Vector2(0f, 1f)),
		CreateRectangleFace(
			new Vector3(0f, 0f, 0f), new Vector2(0f, 0f),
			new Vector3(1f, 0f, 0f), new Vector2(1f, 0f),
			new Vector3(1f, 1f, 0f), new Vector2(1f, 1f),
			new Vector3(0f, 1f, 0f), new Vector2(0f, 1f))
	};

	public static Mesh BuildRenderMesh(VoxelCell[,,] cells, VoxelRegistry voxelRegistry)
	{
		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var uvs = new List<Vector2>();

		if (cells == null || voxelRegistry == null)
		{
			return new Mesh();
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
					if (!TryGetVoxelDefinition(cells[x, y, z], voxelRegistry, out VoxelDefinition voxelDefinition, out VoxelCell cell))
					{
						continue;
					}

					VoxelShape.FaceSet faceSet = voxelDefinition.Shape?.Render;
					if (IsFaceSetEmpty(faceSet))
					{
						continue;
					}

					Vector3 offset = new Vector3(x, y, z);
					bool anyOuterVisible = false;

					for (int i = 0; i < 6; i++)
					{
						VoxelAxisPlane worldPlane = (VoxelAxisPlane)i;
						VoxelAxisPlane localPlane = MapWorldPlaneToLocal(worldPlane, cell.Orientation, cell.FlipOrientation);

						if (faceSet.TryGetOuterFace(localPlane, out VoxelShape.Face localFace) &&
							localFace != null &&
							localFace.HasRenderablePolygons)
						{
							if (!IsFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, cell, localFace, worldPlane, voxelRegistry, useCollision: false))
							{
								AddFace(TransformFaceCached(localFace, cell.Orientation, cell.FlipOrientation), offset, vertices, triangles, uvs, false, Vector2.zero, Vector2.one);
								anyOuterVisible = true;
							}
						}
						else if (!IsFullFaceOccludedByNeighbor(cells, sizeX, sizeY, sizeZ, x, y, z, worldPlane, voxelRegistry, useCollision: false))
						{
							anyOuterVisible = true;
						}
					}

					if (anyOuterVisible && faceSet.InnerFaces != null)
					{
						for (int i = 0; i < faceSet.InnerFaces.Count; i++)
						{
							AddFace(TransformFaceCached(faceSet.InnerFaces[i], cell.Orientation, cell.FlipOrientation), offset, vertices, triangles, uvs, false, Vector2.zero, Vector2.one);
						}
					}
				}
			}
		}

		return BuildMesh(vertices, triangles, uvs);
	}

	public static Mesh BuildColliderMesh(VoxelCell[,,] cells, VoxelRegistry voxelRegistry, VoxelTraversal expectedVoxelTraversal)
	{
		return BuildColliderMeshInternal(cells, voxelRegistry, expectedVoxelTraversal);
	}

	public static Mesh BuildMaskMesh(VoxelCell[,,] cells, VoxelMaskLayer maskLayer, VoxelRegistry voxelRegistry, VoxelMaskRegistry maskRegistry)
	{
		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var uvs = new List<Vector2>();

		if (cells == null || maskLayer == null || voxelRegistry == null || maskRegistry == null || maskLayer.ActiveCellCount <= 0)
		{
			return new Mesh();
		}

		foreach (KeyValuePair<Vector3Int, VoxelMaskCell> entry in maskLayer.ActiveCells)
		{
			VoxelMaskCell maskCell = entry.Value;
			if (maskCell == null || maskCell.Masks == null || maskCell.Masks.Count == 0)
			{
				continue;
			}

			Vector3Int localPosition = entry.Key;
			if (localPosition.x < 0 || localPosition.x >= cells.GetLength(0) ||
				localPosition.y < 0 || localPosition.y >= cells.GetLength(1) ||
				localPosition.z < 0 || localPosition.z >= cells.GetLength(2))
			{
				continue;
			}

			AppendMaskCell(
				cells,
				localPosition.x,
				localPosition.y,
				localPosition.z,
				maskCell,
				voxelRegistry,
				maskRegistry,
				vertices,
				triangles,
				uvs);
		}

		return BuildMesh(vertices, triangles, uvs);
	}

	private static Mesh BuildMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
	{
		var mesh = new Mesh();
		if (vertices.Count >= 65535)
		{
			mesh.indexFormat = IndexFormat.UInt32;
		}

		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);

		if (uvs != null)
		{
			mesh.SetUVs(0, uvs);
		}

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	private static void AppendMaskCell(
		VoxelCell[,,] cells,
		int x,
		int y,
		int z,
		VoxelMaskCell maskCell,
		VoxelRegistry voxelRegistry,
		VoxelMaskRegistry maskRegistry,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs)
	{
		if (maskCell == null ||
			maskCell.Masks == null ||
			maskCell.Masks.Count == 0 ||
			!TryGetVoxelDefinition(cells[x, y, z], voxelRegistry, out VoxelDefinition voxelDefinition, out VoxelCell cell))
		{
			return;
		}

		List<VoxelShape.Face> maskFaces = voxelDefinition.Shape?.GetMaskFaces(cell.FlipOrientation);
		if (maskFaces == null || maskFaces.Count == 0)
		{
			return;
		}

		Vector3 offset = new Vector3(x, y, z);
		for (int maskIndex = 0; maskIndex < maskCell.Masks.Count; maskIndex++)
		{
			VoxelMask mask = maskCell.Masks[maskIndex];
			if (mask == VoxelMask.None || !maskRegistry.TryGetSprite(mask, out Sprite sprite) || sprite == null)
			{
				continue;
			}

			GetSpriteUvRect(sprite, out Vector2 uvAnchor, out Vector2 uvSize);
			Vector3 layerOffset = new Vector3(0f, maskIndex * MaskLayerOffset, 0f);

			for (int faceIndex = 0; faceIndex < maskFaces.Count; faceIndex++)
			{
				AddFace(
					TransformFaceCached(maskFaces[faceIndex], cell.Orientation, VoxelFlipOrientation.PositiveY),
					offset + layerOffset,
					vertices,
					triangles,
					uvs,
					true,
					uvAnchor,
					uvSize);
			}
		}
	}
}
