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
	[SerializeField] private bool debugLogging;

	private void Reset()
	{
		if (modeManager == null)
		{
			modeManager = FindFirstObjectByType<ModeManager>(FindObjectsInactive.Include);
		}

		if (worldPresenter == null)
		{
			worldPresenter = FindFirstObjectByType<WorldPresenter>(FindObjectsInactive.Include);
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
		if (modeManager == null)
		{
			modeManager = FindFirstObjectByType<ModeManager>(FindObjectsInactive.Include);
		}

		if (worldPresenter == null)
		{
			worldPresenter = FindFirstObjectByType<WorldPresenter>(FindObjectsInactive.Include);
		}

		if (modeManager == null || worldPresenter == null || worldPresenter.VoxelRegistry == null)
		{
			LogDebug("Debug battle bootstrap failed because the mode manager, world presenter, or voxel registry is missing.");
			return;
		}

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
			LogDebug("Debug battle bootstrap failed because the board could not be built.");
			return;
		}

		if (showBattleAreaBorder)
		{
			worldPresenter.ShowBattleAreaBorder(boardBuildResult.BorderWorldCells);
		}

		EventCenter.EmitBattleStartRequested(new BattleSetup(enemyTeam, boardBuildResult.Board));
		LogDebug(
			$"Debug battle bootstrapped. Anchor={battleAnchorWorldPosition}, EnemyTeamSize={CountUnits(enemyTeam)}, BorderShown={showBattleAreaBorder}.");
	}

	private static int CountUnits(EncounterUnit[] team)
	{
		if (team == null)
		{
			return 0;
		}

		int count = 0;
		for (int index = 0; index < team.Length; index++)
		{
			if (team[index] != null)
			{
				count++;
			}
		}

		return count;
	}

	private void LogDebug(string message)
	{
		if (!debugLogging)
		{
			return;
		}

		Debug.Log($"[DebugBattleBootstrapper] {message}", this);
	}
}
