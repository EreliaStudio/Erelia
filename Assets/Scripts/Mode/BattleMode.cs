using UnityEngine;

public sealed class BattleMode : Mode
{
	[SerializeField] private BoardPresenter boardPresenter;
	[SerializeField] private BattlePlayerController battlePlayerController;
	[SerializeField] private GameObject battleUnitPrefab;
	[SerializeField] private Transform playerTeamRoot;
	[SerializeField] private Transform enemyTeamRoot;

	private BattleSetup currentSetup;
	private BattleContext battleContext;
	private BattleCoordinator battleCoordinator;
	private BattleOrchestrator battleOrchestrator;
	private BattleUnitManager battleUnitManager;

	public BattleSetup CurrentSetup => currentSetup;
	public BattleContext BattleContext => battleContext;
	public BattlePhase CurrentPhase => battleCoordinator?.CurrentPhase;

	private void Awake()
	{
		if (boardPresenter == null)
		{
			Logger.LogError("[BattleMode] BoardPresenter is not assigned in the inspector. Please assign a BoardPresenter to the BattleMode component.", Logger.Severity.Critical, this);
		}

		if (battlePlayerController == null)
		{
			Logger.LogError("[BattleMode] BattlePlayerController is not assigned in the inspector. Please assign a BattlePlayerController to the BattleMode component.", Logger.Severity.Critical, this);
		}

		if (battleUnitPrefab == null)
		{
			Logger.LogError("[BattleMode] BattleUnitPrefab is not assigned in the inspector. Please assign a battle unit prefab to the BattleMode component.", Logger.Severity.Critical, this);
		}

		if (playerTeamRoot == null)
		{
			Logger.LogError("[BattleMode] PlayerTeamRoot is not assigned in the inspector. Please assign a Transform to the BattleMode component.", Logger.Severity.Critical, this);
		}

		if (enemyTeamRoot == null)
		{
			Logger.LogError("[BattleMode] EnemyTeamRoot is not assigned in the inspector. Please assign a Transform to the BattleMode component.", Logger.Severity.Critical, this);
		}
	}

	public void Enter(BattleSetup setup)
	{
		if (setup == null || setup.Board == null)
		{
			return;
		}

		currentSetup = setup;
		battleContext = new BattleContext(currentSetup);
		battleUnitManager = new BattleUnitManager(playerTeamRoot, enemyTeamRoot, battleUnitPrefab, battleContext);
		battleOrchestrator = new BattleOrchestrator();
		battleCoordinator = new BattleCoordinator(battleContext, battleOrchestrator, boardPresenter, battlePlayerController, NotifyBattleEnded);

		base.Enter();

		boardPresenter.transform.parent.transform.position = (Vector3)currentSetup.Board.WorldAnchor;
		boardPresenter.Assign(currentSetup.Board);

		Vector3Int anchor = currentSetup.Board.WorldAnchor;
		Vector3Int size = new Vector3Int(
			currentSetup.Board.Terrain.SizeX,
			currentSetup.Board.Terrain.SizeY,
			currentSetup.Board.Terrain.SizeZ);

		battlePlayerController.Bind(anchor, size, currentSetup.PlayerWorldPosition);
		battleCoordinator.Start();
	}

	private void Update()
	{
		if (!IsActive || battleCoordinator == null)
		{
			return;
		}

		battleCoordinator.Tick(Time.deltaTime);
	}

	protected override void OnExit()
	{
		battleCoordinator?.Stop();
		battleUnitManager?.Dispose();
		battlePlayerController.Unbind();
		battleUnitManager = null;
		battleCoordinator = null;
		battleOrchestrator = null;
		battleContext = null;
		currentSetup = null;
	}

	private void NotifyBattleEnded()
	{
		EventCenter.EmitBattleEnded();
	}
}
