using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldLoader
{
	[SerializeField, Min(0)] private int visibilityRange = 2;
	[SerializeField, Min(1)] private int chunksPerFrame = 2;
	[SerializeField, Min(0f)] private float updateIntervalSeconds = 0.05f;
	[SerializeField] private int seed;
	[SerializeField] private SimpleDebugChunkGenerator generator = new SimpleDebugChunkGenerator();

	[NonSerialized] private readonly Queue<ChunkCoordinates> pendingChunks = new Queue<ChunkCoordinates>();
	[NonSerialized] private readonly HashSet<ChunkCoordinates> queuedChunks = new HashSet<ChunkCoordinates>();
	[NonSerialized] private float updateTimer;
	[NonSerialized] private ChunkCoordinates currentCenterChunk;

	public int VisibilityRange => visibilityRange;
	public int ChunksPerFrame => chunksPerFrame;
	public float UpdateIntervalSeconds => updateIntervalSeconds;
	public int Seed => seed;
	public SimpleDebugChunkGenerator Generator => generator;
	public bool HasPendingChunks => pendingChunks.Count > 0;

	public WorldLoadResult SetCenterChunk(WorldData worldData, ChunkCoordinates centerChunk)
	{
		currentCenterChunk = centerChunk;
		updateTimer = 0f;
		pendingChunks.Clear();
		queuedChunks.Clear();

		var result = new WorldLoadResult(centerChunk);
		if (worldData == null)
		{
			return result;
		}

		if (generator == null)
		{
			generator = new SimpleDebugChunkGenerator();
		}

		List<ChunkCoordinates> desiredChunks = BuildSpiralChunkOrder(centerChunk, visibilityRange);
		var desiredSet = new HashSet<ChunkCoordinates>(desiredChunks);
		var loadedChunks = new List<ChunkCoordinates>(worldData.Chunks.Keys);

		for (int i = 0; i < loadedChunks.Count; i++)
		{
			ChunkCoordinates coordinates = loadedChunks[i];
			if (desiredSet.Contains(coordinates))
			{
				continue;
			}

			worldData.SetChunk(coordinates, null);
			result.UnloadedChunks.Add(coordinates);
		}

		for (int i = 0; i < desiredChunks.Count; i++)
		{
			ChunkCoordinates coordinates = desiredChunks[i];
			if (worldData.HasChunk(coordinates))
			{
				continue;
			}

			if (queuedChunks.Add(coordinates))
			{
				pendingChunks.Enqueue(coordinates);
			}
		}

		return result;
	}

	public WorldLoadResult ProcessPending(WorldData worldData, float deltaTime)
	{
		var result = new WorldLoadResult(currentCenterChunk);
		if (worldData == null || generator == null || pendingChunks.Count <= 0)
		{
			return result;
		}

		if (updateIntervalSeconds > 0f)
		{
			updateTimer += deltaTime;
			if (updateTimer < updateIntervalSeconds)
			{
				return result;
			}

			updateTimer = 0f;
		}

		int maxChunks = Mathf.Max(1, chunksPerFrame);
		int loadedCount = 0;

		while (loadedCount < maxChunks && pendingChunks.Count > 0)
		{
			ChunkCoordinates coordinates = pendingChunks.Dequeue();
			queuedChunks.Remove(coordinates);

			if (worldData.HasChunk(coordinates))
			{
				continue;
			}

			worldData.SetChunk(coordinates, generator.GenerateChunk(coordinates));
			result.LoadedChunks.Add(coordinates);
			loadedCount++;
		}

		return result;
	}

	private static List<ChunkCoordinates> BuildSpiralChunkOrder(ChunkCoordinates centerChunk, int radius)
	{
		int radiusSquared = radius * radius;
		int targetCount = CountChunksInCircle(radius);
		var orderedChunks = new List<ChunkCoordinates>(targetCount) { centerChunk };

		if (radius <= 0)
		{
			return orderedChunks;
		}

		int x = centerChunk.X;
		int z = centerChunk.Z;
		int stepLength = 1;
		Vector2Int[] directions =
		{
			new Vector2Int(1, 0),
			new Vector2Int(0, 1),
			new Vector2Int(-1, 0),
			new Vector2Int(0, -1)
		};

		while (orderedChunks.Count < targetCount)
		{
			for (int directionIndex = 0; directionIndex < directions.Length && orderedChunks.Count < targetCount; directionIndex++)
			{
				Vector2Int direction = directions[directionIndex];

				for (int step = 0; step < stepLength; step++)
				{
					x += direction.x;
					z += direction.y;

					int dx = x - centerChunk.X;
					int dz = z - centerChunk.Z;
					if ((dx * dx) + (dz * dz) <= radiusSquared)
					{
						orderedChunks.Add(new ChunkCoordinates(x, z));
						if (orderedChunks.Count >= targetCount)
						{
							break;
						}
					}
				}

				if ((directionIndex & 1) == 1)
				{
					stepLength++;
				}
			}
		}

		return orderedChunks;
	}

	private static int CountChunksInCircle(int radius)
	{
		if (radius <= 0)
		{
			return 1;
		}

		int count = 0;
		int radiusSquared = radius * radius;

		for (int dx = -radius; dx <= radius; dx++)
		{
			for (int dz = -radius; dz <= radius; dz++)
			{
				if ((dx * dx) + (dz * dz) <= radiusSquared)
				{
					count++;
				}
			}
		}

		return count;
	}
}
