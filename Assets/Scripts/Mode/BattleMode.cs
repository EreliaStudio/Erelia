using UnityEngine;

public sealed class BattleMode : Mode
{
	[SerializeField] private BoardPresenter boardPresenter;
	[SerializeField] private BattlePlayerController battlePlayerController;
	[SerializeField] private GameObject battleUnitPrefab;
	[SerializeField] private Transform playerTeamRoot;
	[SerializeField] private Transform enemyTeamRoot;
	[SerializeField] private BattleOrchestrator battleOrchestrator = new();

	private BattleContext battleContext;
	private BattleUnitManager battleUnitManager;

	public BattleContext BattleContext => battleContext;
	public BattleOrchestrator BattleOrchestrator => battleOrchestrator;
	public BattlePhaseType? CurrentPhaseType => battleOrchestrator != null && battleOrchestrator.Coordinator.HasActivePhase
		? battleOrchestrator.Coordinator.CurrentPhaseType
		: null;

	private void Awake()
	{
		if (boardPresenter == null)
		{
			Logger.LogError("[BattleMode] BoardPresenter is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (battlePlayerController == null)
		{
			Logger.LogError("[BattleMode] BattlePlayerController is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (battleUnitPrefab == null)
		{
			Logger.LogError("[BattleMode] BattleUnitPrefab is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (playerTeamRoot == null)
		{
			Logger.LogError("[BattleMode] PlayerTeamRoot is not assigned in the inspector.", Logger.Severity.Critical, this);
		}

		if (enemyTeamRoot == null)
		{
			Logger.LogError("[BattleMode] EnemyTeamRoot is not assigned in the inspector.", Logger.Severity.Critical, this);
		}
	}

	public void Enter(BattleContext context)
	{
		if (context == null || context.Board == null)
		{
			return;
		}

		battleContext = context;
		battleContext.ClearRuntime();
		battleUnitManager = new BattleUnitManager(playerTeamRoot, enemyTeamRoot, battleUnitPrefab, battleContext);

		base.Enter();

		boardPresenter.transform.parent.transform.position = (Vector3)battleContext.Board.WorldAnchor;
		boardPresenter.Assign(battleContext.Board);

		Vector3Int anchor = battleContext.Board.WorldAnchor;
		Vector3Int size = new Vector3Int(
			battleContext.Board.Terrain.SizeX,
			battleContext.Board.Terrain.SizeY,
			battleContext.Board.Terrain.SizeZ);

		battlePlayerController.Bind(anchor, size, battleContext.PlayerWorldPosition);
		battleOrchestrator.Initialize(this, battleContext);
	}

	public void TransitionToPhase(BattlePhaseType phaseType)
	{
		battleOrchestrator?.TransitionTo(phaseType);
	}

	protected override void OnExit()
	{
		battleOrchestrator?.Dispose();
		battleUnitManager?.Dispose();
		battlePlayerController.Unbind();
		battleUnitManager = null;
		battleContext = null;
	}
}
