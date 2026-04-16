using UnityEngine;

[DisallowMultipleComponent]
public class GameBootstrapper : MonoBehaviour
{
	[SerializeField] private ModeManager modeManager;
	[SerializeField] private GameSaveData gameSaveData = new GameSaveData();
	[SerializeField] private bool bootstrapOnStart = true;
	[SerializeField] private bool debugLogging;
	[SerializeField] private WorldPresenter worldPresenter; // manually assign this in the Inspector

	private void Reset()
	{
		if (modeManager == null)
		{
			modeManager = FindFirstObjectByType<ModeManager>(FindObjectsInactive.Include);
		}
	}

	private void Awake()
	{
		if (modeManager == null)
		{
			Logger.LogError("[GameBootstrapper] ModeManager is not assigned in the inspector. Please assign a ModeManager to the GameBootstrapper component.", Logger.Severity.Critical, this);
		}

		if (worldPresenter == null)
		{
			Logger.LogError("[GameBootstrapper] WorldPresenter is not assigned in the inspector. Please assign a WorldPresenter to the GameBootstrapper component.", Logger.Severity.Critical, this);
		}
	}

	private void Start()
	{
		if (bootstrapOnStart)
		{
			Bootstrap();
		}
	}

	public void Bootstrap()
	{
		if (modeManager == null)
		{
			Logger.LogError("Bootstrap failed because no ModeManager was assigned to GameBootstrapper.", Logger.Severity.Critical, this);
			return;
		}

		GameContext gameContext = GameContext.CreateFromSave(gameSaveData);

		modeManager.SetGameContext(gameContext);

		if (worldPresenter != null)
		{
			worldPresenter.Bind(gameContext.World);
			worldPresenter.LoadImmediatelyAroundWorldCell(gameContext.Player.WorldCell);
			Logger.LogDebug("WorldPresenter bound and preloaded by bootstrapper.");
		}
		else
		{
			Logger.LogError("Cannot bind WorldPresenter because it is not assigned. Bootstrap will continue without preloading the world.", Logger.Severity.Error, this);
		}

		modeManager.EnterExplorationMode(gameContext);
		Logger.LogDebug($"Game bootstrapped in exploration mode. PlayerCell={gameSaveData.PlayerWorldCell}, WorldSeed={gameSaveData.WorldSeed}.");
	}
}
