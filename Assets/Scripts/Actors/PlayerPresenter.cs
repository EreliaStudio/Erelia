using UnityEngine;

[DisallowMultipleComponent]
public class PlayerPresenter : ActorPresenter
{
	private PlayerData playerData;
	private Vector3 lastWorldPosition;
	private ChunkCoordinates lastChunkCoordinates;

	public PlayerData PlayerData => playerData;

	private void OnEnable()
	{
		CellReached += OnCellReached;
	}

	private void OnDisable()
	{
		CellReached -= OnCellReached;
	}

	public void Bind(PlayerData targetPlayerData)
	{
		playerData = targetPlayerData;
		SyncToTransform();
	}

	public void SyncToTransformAndEmit()
	{
		SyncToTransform();
		EmitCurrentState();
	}

	private void SyncToTransform()
	{
		lastWorldPosition = transform.position;
		lastChunkCoordinates = ChunkCoordinates.FromWorldPosition(lastWorldPosition);
		if (playerData != null)
		{
			playerData.WorldCell = Vector3Int.RoundToInt(lastWorldPosition);
		}
	}

	private void EmitCurrentState()
	{
		EventCenter.EmitPlayerMoved(lastWorldPosition);
		EventCenter.EmitPlayerChunkChanged(lastChunkCoordinates);
	}

	private void OnCellReached(ActorPresenter presenter, Vector3Int worldCellPosition)
	{
		Vector3 currentWorldPosition = transform.position;
		ChunkCoordinates currentChunkCoordinates = ChunkCoordinates.FromWorldVoxelPosition(worldCellPosition);

		lastWorldPosition = currentWorldPosition;
		if (playerData != null)
		{
			playerData.WorldCell = worldCellPosition;
		}

		EventCenter.EmitPlayerMoved(currentWorldPosition);

		if (currentChunkCoordinates.Equals(lastChunkCoordinates))
		{
			return;
		}

		lastChunkCoordinates = currentChunkCoordinates;
		EventCenter.EmitPlayerChunkChanged(currentChunkCoordinates);
	}
}
