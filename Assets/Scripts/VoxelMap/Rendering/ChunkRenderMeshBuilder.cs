using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkRenderMeshBuilder : ChunkMesher
{
	public Mesh BuildMesh(Chunk chunk)
	{
		var mesh = new Mesh();
		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var uvs = new List<Vector2>();

		for (int x = 0; x < Chunk.SizeX; x++)
		{
			for (int y = 0; y < Chunk.SizeY; y++)
			{
				for (int z = 0; z < Chunk.SizeZ; z++)
				{
					AddVoxel(chunk, x, y, z, vertices, triangles, uvs);
				}
			}
		}

		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateNormals();
		mesh.name = "RenderMesh";

		return mesh;
	}

	private void AddVoxel(Chunk chunk, int x, int y, int z, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
	{
		if (!TryGetVoxelDefinition(chunk, x, y, z, out Voxel voxel))
		{
			return;
		}

		Orientation orientation = chunk.Voxels[x, y, z].Orientation;
		FlipOrientation flipOrientation = chunk.Voxels[x, y, z].FlipOrientation;
		Vector3 position = new Vector3(x, y, z);
		bool anyOuterVisible = false;

		TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosX, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegX, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosY, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegY, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosZ, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(chunk, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegZ, vertices, triangles, uvs, ref anyOuterVisible);

		if (anyOuterVisible)
		{
			IReadOnlyList<VoxelFace> innerFaces = voxel.InnerFaces;
			for (int i = 0; i < innerFaces.Count; i++)
			{
				AddFace(TransformFace(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
			}
		}
	}

	private void TryAddOuterFace(
		Chunk chunk,
		Voxel voxel,
		Orientation orientation,
		FlipOrientation flipOrientation,
		Vector3 position,
		int x,
		int y,
		int z,
		OuterShellPlane plane,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs,
		ref bool anyOuterVisible)
	{
		Vector3Int offset = OuterShellPlaneUtil.PlaneToOffset(plane);
		int neighborX = x + offset.x;
		int neighborY = y + offset.y;
		int neighborZ = z + offset.z;
		bool hasNeighbor = TryGetVoxelDefinition(chunk, neighborX, neighborY, neighborZ, out Voxel neighbor);

		OuterShellPlane localPlane = MapWorldPlaneToLocal(plane, orientation, flipOrientation);

		if (!voxel.OuterShellFaces.TryGetValue(localPlane, out VoxelFace face) || face == null || !face.HasRenderablePolygons)
		{
			if (!IsFullyOccludedByNeighbor(chunk, neighbor, neighborX, neighborY, neighborZ, plane, hasNeighbor))
			{
				anyOuterVisible = true;
			}
			return;
		}

		bool isOccluded = false;
		VoxelFace rotatedFace = TransformFace(face, orientation, flipOrientation);
		if (hasNeighbor)
		{
			OuterShellPlane oppositePlane = OuterShellPlaneUtil.GetOppositePlane(plane);
			Orientation neighborOrientation = chunk.Voxels[neighborX, neighborY, neighborZ].Orientation;
			FlipOrientation neighborFlipOrientation = chunk.Voxels[neighborX, neighborY, neighborZ].FlipOrientation;
			OuterShellPlane neighborLocalPlane = MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (neighbor.OuterShellFaces.TryGetValue(neighborLocalPlane, out VoxelFace otherFace))
			{
				VoxelFace rotatedOtherFace = TransformFace(otherFace, neighborOrientation, neighborFlipOrientation);
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
}
