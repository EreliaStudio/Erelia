using UnityEngine;

[RequireComponent(typeof(PlayerView))]
[DisallowMultipleComponent]
public class PlayerPresenter : MonoBehaviour
{
	[SerializeField] private PlayerData playerData = new PlayerData();
	[SerializeField] private PlayerView playerView;

	private Vector3 lastWorldPosition;
	private ChunkCoordinates lastChunkCoordinates;

	public PlayerData PlayerData => playerData;
	public PlayerView PlayerView => playerView;

	private void Reset()
	{
		if (playerView == null)
		{
			playerView = GetComponent<PlayerView>();
		}
	}

	private void Awake()
	{
		InitializeFromTransform();
	}

	private void Start()
	{
		EventCenter.EmitPlayerMoved(lastWorldPosition);
		EventCenter.EmitPlayerChunkChanged(lastChunkCoordinates);
	}

	private void Update()
	{
		Vector3 currentWorldPosition = transform.position;
		ChunkCoordinates currentChunkCoordinates = ChunkCoordinates.FromWorldPosition(currentWorldPosition);

		if (currentWorldPosition != lastWorldPosition)
		{
			lastWorldPosition = currentWorldPosition;
			playerData.CellPosition = Vector3Int.FloorToInt(currentWorldPosition);
			EventCenter.EmitPlayerMoved(currentWorldPosition);
		}

		if (!currentChunkCoordinates.Equals(lastChunkCoordinates))
		{
			lastChunkCoordinates = currentChunkCoordinates;
			playerData.ChunkCoordinates = currentChunkCoordinates;
			EventCenter.EmitPlayerChunkChanged(currentChunkCoordinates);
		}
	}

	private void InitializeFromTransform()
	{
		lastWorldPosition = transform.position;
		lastChunkCoordinates = ChunkCoordinates.FromWorldPosition(lastWorldPosition);
		playerData.CellPosition = Vector3Int.FloorToInt(lastWorldPosition);
		playerData.ChunkCoordinates = lastChunkCoordinates;
	}
}
