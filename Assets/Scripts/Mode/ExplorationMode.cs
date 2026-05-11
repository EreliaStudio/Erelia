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
	private GameContext currentGameContext;
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

	public void Enter(GameContext gameContext, Vector3Int? worldCellOverride = null)
	{
		if (gameContext == null)
		{
			return;
		}

		currentGameContext = gameContext;

		base.Enter();

		worldPresenter.Bind(gameContext.World);

		Vector3Int playerCell = Vector3Int.FloorToInt(gameContext.Player.Position.Value);
		if (!EnsureActorInstance(gameContext.Player, playerCell, out ActorPresenter actor))
		{
			return;
		}

		Vector3Int actorWorldCell = ResolveActorWorldCell(actor, playerCell, worldCellOverride);
		ServiceLocator.Instance?.PlayerService?.BindWorldCellProvider(TryGetSpawnedActorWorldCell);

		if (!EnsureCameraInstance(out Camera explorationCamera, out OrbitingObject orbiting))
		{
			return;
		}

		playerController.Bind(actor, explorationCamera, orbiting);

		worldPresenter.LoadImmediatelyAroundWorldCell(actorWorldCell);

		EmitInitialPlayerState(actorWorldCell);
	}

	protected override void OnEnter()
	{
		EventCenter.PlayerMoved += OnPlayerMoved;
	}

	protected override void OnExit()
	{
		EventCenter.PlayerMoved -= OnPlayerMoved;
		playerController.Unbind();
		if (spawnedActor != null)
		{
			spawnedActor.gameObject.SetActive(false);
		}
		currentGameContext = null;
	}

	private bool EnsureActorInstance(ActorData actorData, Vector3Int initialWorldCell, out ActorPresenter actor)
	{
		if (spawnedActor != null)
		{
			spawnedActor.gameObject.SetActive(true);
			actor = spawnedActor;
			return true;
		}

		spawnedActor = actorManager.SpawnActor(initialWorldCell, actorData);
		if (spawnedActor == null)
		{
			actor = null;
			return false;
		}

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

	private Vector3Int ResolveActorWorldCell(ActorPresenter actor, Vector3Int fallbackWorldCell, Vector3Int? worldCellOverride)
	{
		if (worldCellOverride.HasValue && actorManager.TrySetWorldCell(actor, worldCellOverride.Value))
		{
			return actorManager.TryGetWorldCell(actor, out Vector3Int overrideResolvedWorldCell)
				? overrideResolvedWorldCell
				: worldCellOverride.Value;
		}

		if (actorManager.TryGetWorldCell(actor, out Vector3Int currentWorldCell))
		{
			return currentWorldCell;
		}

		if (actorManager.TrySetWorldCell(actor, fallbackWorldCell) &&
			actorManager.TryGetWorldCell(actor, out Vector3Int fallbackResolvedWorldCell))
		{
			return fallbackResolvedWorldCell;
		}

		return fallbackWorldCell;
	}

	private Vector3Int? TryGetSpawnedActorWorldCell()
	{
		if (spawnedActor != null &&
			actorManager != null &&
			actorManager.TryGetWorldCell(spawnedActor, out Vector3Int worldCell))
		{
			return worldCell;
		}

		return null;
	}

	private void EmitInitialPlayerState(Vector3Int worldCell)
	{
		lastChunkCoordinates = ChunkCoordinates.FromWorldVoxelPosition(worldCell);
		EventCenter.EmitPlayerChunkChanged(lastChunkCoordinates);
	}

	private void OnPlayerMoved(Vector3Int p_worldCellPosition)
	{
		ChunkCoordinates newChunk = ChunkCoordinates.FromWorldVoxelPosition(p_worldCellPosition);
		if (newChunk.Equals(lastChunkCoordinates))
		{
			return;
		}

		lastChunkCoordinates = newChunk;
		EventCenter.EmitPlayerChunkChanged(newChunk);
	}
}
