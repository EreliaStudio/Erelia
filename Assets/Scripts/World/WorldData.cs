using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldData
{
	public readonly Dictionary<ChunkCoordinates, ChunkData> Chunks = new Dictionary<ChunkCoordinates, ChunkData>();

	public void SetChunk(ChunkCoordinates coordinates, ChunkData chunkData)
	{
		if (chunkData == null)
		{
			Chunks.Remove(coordinates);
			return;
		}

		Chunks[coordinates] = chunkData;
	}

	public bool TryGetChunk(ChunkCoordinates coordinates, out ChunkData chunkData)
	{
		return Chunks.TryGetValue(coordinates, out chunkData);
	}

	public bool TryGetChunk(Vector3Int worldPosition, out ChunkCoordinates coordinates, out Vector3Int localPosition, out ChunkData chunkData)
	{
		coordinates = ChunkCoordinates.FromWorldVoxelPosition(worldPosition);
		localPosition = ChunkCoordinates.ToLocalVoxelPosition(worldPosition);

		if (localPosition.y < 0 || localPosition.y >= ChunkData.FixedSizeY)
		{
			chunkData = null;
			return false;
		}

		return TryGetChunk(coordinates, out chunkData) && chunkData != null;
	}

	public bool TryGetCell(Vector3Int worldPosition, out VoxelCell cell)
	{
		if (!TryGetChunk(worldPosition, out _, out Vector3Int localPosition, out ChunkData chunkData))
		{
			cell = null;
			return false;
		}

		cell = chunkData.Cells[localPosition.x, localPosition.y, localPosition.z];
		return cell != null && !cell.IsEmpty;
	}

	public bool HasChunk(ChunkCoordinates coordinates)
	{
		return Chunks.ContainsKey(coordinates);
	}

	public void Clear()
	{
		Chunks.Clear();
	}
}
