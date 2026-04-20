using System;
using UnityEngine;

public static class EventCenter
{
	public static event Action<Vector3> PlayerMoved;
	public static event Action<ChunkCoordinates> PlayerChunkChanged;
	public static event Action<ActorMovementRequest> ActorMoveRequested;
	public static event Action<BattleSetup> BattleStartRequested;
	public static event Action BattleEnded;

	public static void EmitPlayerMoved(Vector3 worldPosition)
	{
		PlayerMoved?.Invoke(worldPosition);
	}

	public static void EmitPlayerChunkChanged(ChunkCoordinates chunkCoordinates)
	{
		PlayerChunkChanged?.Invoke(chunkCoordinates);
	}

	public static void EmitActorMoveRequested(ActorMovementRequest request)
	{
		ActorMoveRequested?.Invoke(request);
	}

	public static void EmitBattleStartRequested(BattleSetup setup)
	{
		BattleStartRequested?.Invoke(setup);
	}

	public static void EmitBattleEnded()
	{
		BattleEnded?.Invoke();
	}
}
