using UnityEngine;

[DisallowMultipleComponent]
public class GameBootstrapper : MonoBehaviour
{
	[SerializeField] private ModeManager modeManager;
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private ReferenceableDatabase referenceableDatabase;

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

	private void OnEnable()
	{
		EventCenter.EnteringGame += Bootstrap;
	}

	private void OnDisable()
	{
		EventCenter.EnteringGame -= Bootstrap;
	}

	public void Bootstrap(GameSaveData p_gameSaveData)
	{
		if (p_gameSaveData == null)
		{
			Logger.LogError("[GameBootstrapper] Cannot enter game: GameSaveData is null. The entry emitter must provide a valid GameSaveData instance.", Logger.Severity.Critical, this);
			return;
		}

		ServiceLocator.Create(p_gameSaveData);
		ServiceLocator.Instance.ReferenceRegistry.Bind(referenceableDatabase != null ? referenceableDatabase.Entries : null);
		GameContext gameContext = ServiceLocator.Instance.GameContext;
		PlayerService playerService = ServiceLocator.Instance.PlayerService;

		worldPresenter.Bind(gameContext.World);
		ServiceLocator.Instance.WorldService.ConfigureVoxelRegistry(worldPresenter.VoxelRegistry);

		// Empty incoming saves need generated terrain available to resolve the initial spawn.
		worldPresenter.LoadImmediatelyAroundWorldCell(playerService.PlayerWorldCell);
		if (GameInitializationService.TryInitializeNewGameSave(p_gameSaveData, worldPresenter.WorldData, worldPresenter.VoxelRegistry))
		{
			gameContext.LoadFromSave(p_gameSaveData);
			ServiceLocator.Instance.SaveService.Save();
		}

		modeManager.SetGameContext(gameContext);
		modeManager.EnterExplorationMode(gameContext);
	}

	private void OnDestroy()
	{
		ServiceLocator.Destroy();
	}
}
