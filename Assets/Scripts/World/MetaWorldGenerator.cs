using System;
using UnityEngine;

[Serializable]
public class MetaWorldGenerator
{
	[SerializeField] private int seed;
	[SerializeField] private BiomeDefinition defaultBiome;

	public int Seed => seed;
	public BiomeDefinition DefaultBiome => defaultBiome;

	public ChunkMetaData GenerateChunkMeta(ChunkCoordinates coordinates)
	{
		return new ChunkMetaData
		{
			Biome = defaultBiome
		};
	}
}
