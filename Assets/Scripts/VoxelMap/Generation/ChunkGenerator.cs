using System;
using UnityEngine;

[Serializable]
public class ChunkGenerator
{
	[HideInInspector][SerializeField] private VoxelRegistry registry;
	// [SerializeField] private int seed = 0;
	[Header("Voxel Ids")]
	[SerializeField] private int cubeId = 0;
	[SerializeField] private int stairId = 1;
	[SerializeField] private int slopeId = 2;
	[SerializeField] private int slabId = 3;
	[SerializeField] private int crossPlaneId = 4;

	[Header("Layout (local coords)")]
	[SerializeField] private int upperLayerY = 1;
	[SerializeField] private int stairLine = 0;
	[SerializeField] private int slopeLine = 2;
	[SerializeField] private int slabLine = 4;

	public void SetRegistry(VoxelRegistry value)
	{
		registry = value;
	}

	public Chunk Generate(ChunkCoord coord)
	{
		var chunk = new Chunk();

		for (int x = 0; x < Chunk.SizeX; x++)
		{
			for (int y = 0; y < Chunk.SizeY; y++)
			{
				for (int z = 0; z < Chunk.SizeZ; z++)
				{
					int localY = y;
					int dataId = registry.AirId;

					if (localY == 0)
					{
						dataId = cubeId;
					}
					else if (localY == upperLayerY)
					{
						if (x == stairLine || z == stairLine)
						{
							dataId = stairId;
						}
						else if ((x == slopeLine || z == slopeLine) && x != stairLine && z != stairLine)
						{
							dataId = slopeId;
						}
						else if (x == slabLine || z == slabLine)
						{
							dataId = slabId;
						}
						else if (coord.X == 1 && coord.Y == 0 && coord.Z == 1)
						{
							dataId = crossPlaneId;
						}
					}

					chunk.Voxels[x, y, z] = new VoxelCell(dataId, Orientation.PositiveX, FlipOrientation.PositiveY);
				}
			}
		}

		return chunk;
	}
}
