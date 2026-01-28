using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoxelRenderMeshBuilder : VoxelMesher
{
	[NonSerialized] private readonly List<Vector3> vertices = new List<Vector3>();
	[NonSerialized] private readonly List<int> triangles = new List<int>();
	[NonSerialized] private readonly List<Vector2> uvs = new List<Vector2>();
	private int currentBaseX;
	private int currentBaseZ;

	public void SetBaseWorldXZ(int worldX, int worldZ)
	{
		currentBaseX = worldX;
		currentBaseZ = worldZ;
	}

	public Mesh BuildMesh(VoxelCell[,,] voxels, int sizeX, int sizeY, int sizeZ)
	{
		var mesh = new Mesh();
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();

		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					AddVoxel(voxels, sizeX, sizeY, sizeZ, x, y, z, vertices, triangles, uvs);
				}
			}
		}

		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateNormals();

		return mesh;
	}

	protected override bool TryGetVoxelDefinition(VoxelCell[,,] voxels, int sizeX, int sizeY, int sizeZ, int x, int y, int z, out Voxel voxel)
	{
		if (!base.TryGetVoxelDefinition(voxels, sizeX, sizeY, sizeZ, x, y, z, out voxel))
		{
			return false;
		}

		return true;
	}

	private bool TryGetVoxelDefinitionWithMask(VoxelCell[,,] voxels, int sizeX, int sizeY, int sizeZ, int x, int y, int z, out Voxel voxel)
	{
		if (!TryGetVoxelDefinition(voxels, sizeX, sizeY, sizeZ, x, y, z, out voxel))
		{
			return false;
		}

		return true;
	}

	private void AddVoxel(VoxelCell[,,] voxels, int sizeX, int sizeY, int sizeZ, int x, int y, int z, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
	{
		if (!TryGetVoxelDefinitionWithMask(voxels, sizeX, sizeY, sizeZ, x, y, z, out Voxel voxel))
		{
			return;
		}

		if (!TryGetVoxelCell(voxels, sizeX, sizeY, sizeZ, x, y, z, out VoxelCell cell))
		{
			return;
		}

		Orientation orientation = cell.Orientation;
		FlipOrientation flipOrientation = cell.FlipOrientation;
		Vector3 position = new Vector3(x, y, z);
		bool anyOuterVisible = false;

		TryAddOuterFace(voxels, sizeX, sizeY, sizeZ, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosX, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(voxels, sizeX, sizeY, sizeZ, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegX, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(voxels, sizeX, sizeY, sizeZ, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosY, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(voxels, sizeX, sizeY, sizeZ, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegY, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(voxels, sizeX, sizeY, sizeZ, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.PosZ, vertices, triangles, uvs, ref anyOuterVisible);
		TryAddOuterFace(voxels, sizeX, sizeY, sizeZ, voxel, orientation, flipOrientation, position, x, y, z, OuterShellPlane.NegZ, vertices, triangles, uvs, ref anyOuterVisible);

		if (anyOuterVisible)
		{
			IReadOnlyList<VoxelFace> innerFaces = voxel.InnerFaces;
			for (int i = 0; i < innerFaces.Count; i++)
			{
				AddFace(TransformFaceCached(innerFaces[i], orientation, flipOrientation), position, vertices, triangles, uvs);
			}
		}
	}

	private void TryAddOuterFace(
		VoxelCell[,,] voxels,
		int sizeX,
		int sizeY,
		int sizeZ,
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
		bool hasNeighbor = TryGetVoxelDefinitionWithMask(voxels, sizeX, sizeY, sizeZ, neighborX, neighborY, neighborZ, out Voxel neighbor);

		OuterShellPlane localPlane = MapWorldPlaneToLocal(plane, orientation, flipOrientation);

		if (!voxel.OuterShellFaces.TryGetValue(localPlane, out VoxelFace face) || face == null || !face.HasRenderablePolygons)
		{
			if (!IsFullyOccludedByNeighbor(voxels, sizeX, sizeY, sizeZ, neighbor, neighborX, neighborY, neighborZ, plane, hasNeighbor))
			{
				anyOuterVisible = true;
			}
			return;
		}

		bool isOccluded = false;
		VoxelFace rotatedFace = TransformFaceCached(face, orientation, flipOrientation);
		if (hasNeighbor)
		{
			OuterShellPlane oppositePlane = OuterShellPlaneUtil.GetOppositePlane(plane);
			if (!TryGetVoxelCell(voxels, sizeX, sizeY, sizeZ, neighborX, neighborY, neighborZ, out VoxelCell neighborCell))
			{
				neighborCell = default;
			}
			Orientation neighborOrientation = neighborCell.Orientation;
			FlipOrientation neighborFlipOrientation = neighborCell.FlipOrientation;
			OuterShellPlane neighborLocalPlane = MapWorldPlaneToLocal(oppositePlane, neighborOrientation, neighborFlipOrientation);
			if (neighbor.OuterShellFaces.TryGetValue(neighborLocalPlane, out VoxelFace otherFace))
			{
				VoxelFace rotatedOtherFace = TransformFaceCached(otherFace, neighborOrientation, neighborFlipOrientation);
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
