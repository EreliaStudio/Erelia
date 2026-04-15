using UnityEngine;

[RequireComponent(typeof(PlayerView))]
[RequireComponent(typeof(Actor))]
[DisallowMultipleComponent]
public class PlayerPresenter : MonoBehaviour
{
	[SerializeField] private PlayerData playerData = new PlayerData();
	[SerializeField] private Actor actor;
	[SerializeField] private PlayerView playerView;
	private Vector3 lastWorldPosition;
	private ChunkCoordinates lastChunkCoordinates;

	public PlayerData PlayerData => playerData;
	public PlayerView PlayerView => playerView;

	private void Reset()
	{
		if (actor == null)
		{
			actor = GetComponent<Actor>();
		}

		if (playerView == null)
		{
			playerView = GetComponent<PlayerView>();
		}
	}

	private void Awake()
	{
		if (actor == null)
		{
			actor = GetComponent<Actor>();
		}

		InitializeFromTransform();
	}

	private void OnEnable()
	{
		if (actor != null)
		{
			actor.CellReached += OnActorCellReached;
		}
	}

	private void OnDisable()
	{
		if (actor != null)
		{
			actor.CellReached -= OnActorCellReached;
		}
	}

	private void Start()
	{
		EventCenter.EmitPlayerMoved(lastWorldPosition);
		EventCenter.EmitPlayerChunkChanged(lastChunkCoordinates);
	}

	private void InitializeFromTransform()
	{
		lastWorldPosition = transform.position;
		lastChunkCoordinates = ChunkCoordinates.FromWorldPosition(lastWorldPosition);
	}

	private void OnActorCellReached(Actor sourceActor, Vector3Int worldCellPosition)
	{
		if (sourceActor != actor)
		{
			return;
		}

		Vector3 currentWorldPosition = transform.position;
		ChunkCoordinates currentChunkCoordinates = ChunkCoordinates.FromWorldVoxelPosition(worldCellPosition);

		lastWorldPosition = currentWorldPosition;
		EventCenter.EmitPlayerMoved(currentWorldPosition);

		if (currentChunkCoordinates.Equals(lastChunkCoordinates))
		{
			return;
		}

		lastChunkCoordinates = currentChunkCoordinates;
		EventCenter.EmitPlayerChunkChanged(currentChunkCoordinates);
	}
}
