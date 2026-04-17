using UnityEngine;

public sealed class ExplorationMode : Mode
{
	public override ModeKind Kind => ModeKind.Exploration;

	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private GameObject playerPrefab;

	private PlayerPresenter spawnedPlayer;

	private void Awake()
	{
		if (worldPresenter == null)
		{
			Logger.LogError("[ExplorationMode] WorldPresenter is not assigned in the inspector. Please assign a WorldPresenter to the ExplorationMode component.", Logger.Severity.Critical, this);
		}

		if (playerPrefab == null)
		{
			Logger.LogError("[ExplorationMode] PlayerPrefab is not assigned in the inspector. Please assign a player prefab to the ExplorationMode component.", Logger.Severity.Critical, this);
		}
	}

	protected override void OnEnter(ModeContext context)
	{
		GameContext gameContext = context?.GameContext;
		if (gameContext == null)
		{
			LogDebug("Entered exploration mode without a game context.");
			return;
		}

		if (worldPresenter != null)
		{
			worldPresenter.Bind(gameContext.World);
		}

		if (!EnsurePlayerInstance())
		{
			LogDebug("Exploration mode could not find or create a player object.");
			return;
		}

		spawnedPlayer.transform.position = gameContext.Player.WorldPosition;
		spawnedPlayer.Bind(gameContext.Player);

		if (worldPresenter != null)
		{
			worldPresenter.LoadImmediatelyAroundWorldCell(gameContext.Player.WorldCell);
		}

		spawnedPlayer.SyncToTransformAndEmit();
	}

	private bool EnsurePlayerInstance()
	{
		if (spawnedPlayer != null)
		{
			return true;
		}

		if (playerPrefab == null)
		{
			return false;
		}

		GameObject playerInstance = Instantiate(playerPrefab, transform);
		spawnedPlayer = playerInstance.GetComponent<PlayerPresenter>();
		LogDebug("Player prefab instantiated for exploration mode.");
		return spawnedPlayer != null;
	}
}
