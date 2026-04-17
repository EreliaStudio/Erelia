using UnityEngine;

[DisallowMultipleComponent]
public class DebugBattleBootstrapper : MonoBehaviour
{
	[SerializeField] private ModeManager modeManager;
	[SerializeField] private WorldPresenter worldPresenter;
	[SerializeField] private GameSaveData gameSaveData = new GameSaveData();
	[SerializeField] private BoardConfiguration boardConfiguration = new BoardConfiguration();
	[SerializeField] private Vector3Int battleAnchorWorldPosition = Vector3Int.zero;
	[SerializeField] private EncounterUnit[] enemyTeam = new EncounterUnit[GameRule.TeamMemberCount];
	[SerializeField] private bool showBattleAreaBorder = true;
	[SerializeField] private bool bootstrapOnStart;

	private void Awake()
	{
		if (modeManager == null)
		{
			Logger.LogError("[DebugBattleBootstrapper] ModeManager is not assigned in the inspector. Please assign a ModeManager to the DebugBattleBootstrapper component.", Logger.Severity.Critical, this);
		}

		if (worldPresenter == null)
		{
			Logger.LogError("[DebugBattleBootstrapper] WorldPresenter is not assigned in the inspector. Please assign a WorldPresenter to the DebugBattleBootstrapper component.", Logger.Severity.Critical, this);
		}
	}

	private void Start()
	{
		if (bootstrapOnStart)
		{
			Bootstrap();
		}
	}

	[ContextMenu("Bootstrap Debug Battle")]
	public void Bootstrap()
	{
		GameContext gameContext = GameContext.CreateFromSave(gameSaveData);
		modeManager.SetGameContext(gameContext);
		worldPresenter.Bind(gameContext.World);
		worldPresenter.LoadImmediatelyAroundWorldCell(battleAnchorWorldPosition);

		BoardBuildResult boardBuildResult = BoardDataBuilder.Build(
			gameContext.World.WorldData,
			worldPresenter.VoxelRegistry,
			battleAnchorWorldPosition,
			boardConfiguration);

		if (boardBuildResult == null || boardBuildResult.Board == null)
		{
			return;
		}

		if (showBattleAreaBorder)
		{
			worldPresenter.ShowBattleAreaBorder(boardBuildResult.BorderWorldCells);
		}

		EventCenter.EmitBattleStartRequested(new BattleSetup(enemyTeam, boardBuildResult.Board));
	}

}
