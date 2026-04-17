using UnityEngine;

[DisallowMultipleComponent]
public class GameBootstrapper : MonoBehaviour
{
	[SerializeField] private ModeManager modeManager;
	[SerializeField] private GameSaveData gameSaveData = new GameSaveData();
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
		GameContext gameContext = GameContext.CreateFromSave(gameSaveData);

		modeManager.SetGameContext(gameContext);

		worldPresenter.Bind(gameContext.World);
		worldPresenter.LoadImmediatelyAroundWorldCell(gameSaveData.PlayerWorldCell);
		gameContext.EnsurePlayerSpawn(gameSaveData, worldPresenter.WorldData, worldPresenter.VoxelRegistry);

		modeManager.EnterExplorationMode(gameContext);
	}
}
