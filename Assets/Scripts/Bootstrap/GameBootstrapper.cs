using UnityEngine;

[DisallowMultipleComponent]
public class GameBootstrapper : MonoBehaviour
{
	private enum BootstrapMode
	{
		NewGame,
		LoadFromSave
	}

	[SerializeField] private ModeManager modeManager;
	[SerializeField] private GameSaveData gameSaveData = new GameSaveData();
	[SerializeField] private BootstrapMode bootstrapMode = BootstrapMode.NewGame;
	[SerializeField] private bool bootstrapOnStart = true;
	[SerializeField] private WorldPresenter worldPresenter;

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
		gameSaveData ??= new GameSaveData();
		GameContext gameContext = new GameContext();
		gameContext.LoadFromSave(gameSaveData);
		worldPresenter.Bind(gameContext.World);

		if (bootstrapMode == BootstrapMode.NewGame)
		{
			// New-game setup needs generated terrain available to resolve the initial spawn.
			worldPresenter.LoadImmediatelyAroundWorldCell(gameSaveData.PlayerWorldCell);
			if (GameInitializationService.TryInitializeNewGameSave(gameSaveData, worldPresenter.WorldData, worldPresenter.VoxelRegistry))
			{
				gameContext.LoadFromSave(gameSaveData);
			}
		}

		modeManager.SetGameContext(gameContext);
		modeManager.EnterExplorationMode(gameContext);
	}
}
