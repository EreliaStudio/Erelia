using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VoxelMapData
{
    public Dictionary<ChunkCoord, Chunk> Chunks = new Dictionary<ChunkCoord, Chunk>();
    [SerializeField] private ChunkGenerator generator = new ChunkGenerator();
    [HideInInspector] [SerializeField] private VoxelRegistry registry;

    public VoxelRegistry Registry => registry;

    public void SetRegistry(VoxelRegistry value)
    {
        registry = value;
        generator.SetRegistry(registry);
    }

    public ChunkGenerator Generator => generator;

    public Chunk GetOrCreateChunk(ChunkCoord coord)
    {
        if (!Chunks.TryGetValue(coord, out Chunk chunk))
        {
            chunk = generator != null ? generator.Generate(coord) : new Chunk();
            Chunks.Add(coord, chunk);
        }

        return chunk;
    }

    public Chunk GetOrCreateChunkFromWorld(Vector3 worldPosition)
    {
        ChunkCoord coord = ChunkCoord.FromWorld(worldPosition);
        return GetOrCreateChunk(coord);
    }
}
