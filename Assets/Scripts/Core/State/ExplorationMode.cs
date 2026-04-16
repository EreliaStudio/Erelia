using UnityEngine;

public sealed class ExplorationMode : Mode
{
	public override ModeKind Kind => ModeKind.Exploration;

	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private Transform playerTransform;
	[SerializeField] private PlayerPresenter playerPresenter;
	[SerializeField] private GameObject playerPrefab;
	[SerializeField] private Transform playerParent;

	protected override void Reset()
	{
		base.Reset();
		ResolveReferences();
	}

	protected override void OnEnter(ModeContext context)
	{
		ResolveReferences();

		GameContext gameContext = context?.GameContext;
		if (gameContext == null)
		{
			LogDebug("Entered exploration mode without a game context.");
			return;
		}

		worldPresenter?.Bind(gameContext.World);

		if (!EnsurePlayerInstance())
		{
			LogDebug("Exploration mode could not find or create a player object.");
			return;
		}

		if (playerPresenter == null)
		{
			playerPresenter = playerTransform.GetComponent<PlayerPresenter>();
		}

		playerPresenter?.Bind(gameContext.Player);
		worldPresenter?.LoadImmediatelyAroundWorldCell(gameContext.Player.WorldCell);
		playerTransform.position = gameContext.Player.WorldPosition;

		if (playerPresenter != null)
		{
			playerPresenter.SyncToTransformAndEmit();
			return;
		}

		EventCenter.EmitPlayerMoved(playerTransform.position);
		EventCenter.EmitPlayerChunkChanged(ChunkCoordinates.FromWorldPosition(playerTransform.position));
	}

	private void ResolveReferences()
	{
		if (worldPresenter == null)
		{
			worldPresenter = FindFirstObjectByType<WorldPresenter>(FindObjectsInactive.Include);
		}

		if (playerPresenter == null)
		{
			playerPresenter = FindFirstObjectByType<PlayerPresenter>(FindObjectsInactive.Include);
		}

		if (playerTransform == null && playerPresenter != null)
		{
			playerTransform = playerPresenter.transform;
		}
	}

	private bool EnsurePlayerInstance()
	{
		if (playerTransform != null)
		{
			return true;
		}

		if (playerPrefab == null)
		{
			return false;
		}

		Transform parent = playerParent;
		if (parent == null && Root != null)
		{
			parent = Root.transform;
		}

		GameObject playerInstance = parent != null
			? Instantiate(playerPrefab, parent)
			: Instantiate(playerPrefab);
		playerTransform = playerInstance.transform;
		playerPresenter = playerInstance.GetComponent<PlayerPresenter>();
		LogDebug("Player prefab instantiated for exploration mode.");
		return true;
	}
}
