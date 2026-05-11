using System;
using UnityEngine;

public static partial class EventCenter
{
	public static event Action<Vector3Int> PlayerMoved;
	public static event Action<ChunkCoordinates> PlayerChunkChanged;
	public static event Action<ActorMovementRequest> ActorMoveRequested;

	public static void EmitPlayerMoved(Vector3Int worldCell)
	{
		PlayerMoved?.Invoke(worldCell);
	}

	public static void EmitPlayerChunkChanged(ChunkCoordinates chunkCoordinates)
	{
		PlayerChunkChanged?.Invoke(chunkCoordinates);
	}

	public static void EmitActorMoveRequested(ActorMovementRequest request)
	{
		ActorMoveRequested?.Invoke(request);
	}
}
