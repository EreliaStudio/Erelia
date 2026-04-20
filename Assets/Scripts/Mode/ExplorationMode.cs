using UnityEngine;

public sealed class ExplorationMode : Mode
{
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private ActorManager actorManager;
	[SerializeField] private ExplorationPlayerController playerController;
	[SerializeField] private GameObject cameraPrefab;
	[SerializeField] private Vector3 cameraLocalOffset = new Vector3(8f, 11f, 8f);

	private ActorPresenter spawnedActor;
	private GameObject spawnedCamera;
	private PlayerData currentPlayerData;
	private ChunkCoordinates lastChunkCoordinates;

	private void Awake()
	{
		if (worldPresenter == null)
		{
			Logger.LogError("[ExplorationMode] WorldPresenter is not assigned in the inspector. Please assign a WorldPresenter to the ExplorationMode component.", Logger.Severity.Critical, this);
		}

		if (actorManager == null)
		{
			Logger.LogError("[ExplorationMode] ActorManager is not assigned in the inspector. Please assign an ActorManager to the ExplorationMode component.", Logger.Severity.Critical, this);
		}

		if (playerController == null)
		{
			Logger.LogError("[ExplorationMode] ExplorationPlayerController is not assigned in the inspector. Please assign an ExplorationPlayerController to the ExplorationMode component.", Logger.Severity.Critical, this);
		}

		if (cameraPrefab == null)
		{
			Logger.LogError("[ExplorationMode] CameraPrefab is not assigned in the inspector. Please assign a camera prefab to the ExplorationMode component.", Logger.Severity.Critical, this);
		}
	}

	public void Enter(GameContext gameContext)
	{
		if (gameContext == null)
		{
			return;
		}

		currentPlayerData = gameContext.Player;

		base.Enter();

		worldPresenter.Bind(gameContext.World);

		if (!EnsureActorInstance(out ActorPresenter actor))
		{
			return;
		}

		Vector3Int cell = currentPlayerData.WorldCell;
		actor.transform.position = new Vector3(cell.x + 0.5f, cell.y, cell.z + 0.5f);

		if (!EnsureCameraInstance(out Camera explorationCamera, out OrbitingObject orbiting))
		{
			return;
		}

		playerController.Bind(actor, explorationCamera, orbiting);

		worldPresenter.LoadImmediatelyAroundWorldCell(currentPlayerData.WorldCell);

		EmitInitialPlayerState();
	}

	protected override void OnExit()
	{
		playerController.Unbind();
		if (spawnedActor != null)
		{
			spawnedActor.gameObject.SetActive(false);
		}
		currentPlayerData = null;
	}

	private bool EnsureActorInstance(out ActorPresenter actor)
	{
		if (spawnedActor != null)
		{
			spawnedActor.gameObject.SetActive(true);
			actor = spawnedActor;
			return true;
		}

		spawnedActor = actorManager.SpawnActor(currentPlayerData.WorldPosition, currentPlayerData);
		if (spawnedActor == null)
		{
			actor = null;
			return false;
		}

		spawnedActor.CellReached += OnActorCellReached;
		actor = spawnedActor;
		return true;
	}

	private bool EnsureCameraInstance(out Camera camera, out OrbitingObject orbiting)
	{
		if (spawnedCamera != null)
		{
			camera = spawnedCamera.GetComponentInChildren<Camera>();
			orbiting = spawnedCamera.GetComponent<OrbitingObject>();
			return camera != null && orbiting != null;
		}

		Vector3 cameraWorldPosition = spawnedActor.transform.TransformPoint(cameraLocalOffset);
		spawnedCamera = Instantiate(cameraPrefab, cameraWorldPosition, Quaternion.identity, spawnedActor.transform);

		camera = spawnedCamera.GetComponentInChildren<Camera>();
		orbiting = spawnedCamera.GetComponent<OrbitingObject>();
		return camera != null && orbiting != null;
	}

	private void EmitInitialPlayerState()
	{
		if (currentPlayerData == null)
		{
			return;
		}

		lastChunkCoordinates = ChunkCoordinates.FromWorldPosition(currentPlayerData.WorldPosition);
		EventCenter.EmitPlayerMoved(currentPlayerData.WorldPosition);
		EventCenter.EmitPlayerChunkChanged(lastChunkCoordinates);
	}

	private void OnActorCellReached(ActorPresenter presenter, Vector3Int worldCellPosition)
	{
		if (currentPlayerData == null)
		{
			return;
		}

		currentPlayerData.WorldCell = worldCellPosition;
		EventCenter.EmitPlayerMoved(presenter.transform.position);

		ChunkCoordinates newChunk = ChunkCoordinates.FromWorldVoxelPosition(worldCellPosition);
		if (newChunk.Equals(lastChunkCoordinates))
		{
			return;
		}

		lastChunkCoordinates = newChunk;
		EventCenter.EmitPlayerChunkChanged(newChunk);
	}
}