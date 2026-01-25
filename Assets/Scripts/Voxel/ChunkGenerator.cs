using System;
using UnityEngine;

[Serializable]
public class ChunkGenerator
{
    [HideInInspector] [SerializeField] private VoxelRegistry registry;
    [SerializeField] private int seed = 0;

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
                    int worldY = (coord.Y * Chunk.SizeY) + y;
                    int dataId = registry.AirId;

                    if (worldY == 0)
                    {
                        float worldX = (coord.X * Chunk.SizeX) + x;
                        float worldZ = (coord.Z * Chunk.SizeZ) + z;
                        float noise = Mathf.PerlinNoise(
                            (worldX + seed) * 0.1f,
                            (worldZ + seed) * 0.1f);
                        dataId = noise >= 0.5f ? 0 : 1;
                    }

                    chunk.Voxels[x, y, z] = new VoxelCell(dataId, Orientation.PositiveX);
                }
            }
        }

        return chunk;
    }
}
