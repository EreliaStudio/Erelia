using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Chunk
{
	public const int SizeX = 16;
	public const int SizeY = 64;
	public const int SizeZ = 16;

	public VoxelCell[,,] Voxels = new VoxelCell[SizeX, SizeY, SizeZ];

	public Chunk()
	{
		for (int x = 0; x < SizeX; x++)
		{
			for (int y = 0; y < SizeY; y++)
			{
				for (int z = 0; z < SizeZ; z++)
				{
					Voxels[x, y, z] = new VoxelCell(0, Orientation.PositiveX, FlipOrientation.PositiveY);
				}
			}
		}
	}

	public List<Mesh> BuildSolidCollisionMeshes(VoxelSolidCollisionMeshBuilder mesher)
	{
		if (mesher == null)
		{
			return new List<Mesh>();
		}

		return mesher.BuildSolidMeshes(Voxels, SizeX, SizeY, SizeZ);
	}
}
