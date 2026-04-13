using System;
using UnityEngine;

public static class EventCenter
{
	public static event Action<Vector3> PlayerMoved;
	public static event Action<ChunkCoordinates> PlayerChunkChanged;

	public static void EmitPlayerMoved(Vector3 worldPosition)
	{
		PlayerMoved?.Invoke(worldPosition);
	}

	public static void EmitPlayerChunkChanged(ChunkCoordinates chunkCoordinates)
	{
		PlayerChunkChanged?.Invoke(chunkCoordinates);
	}
}
