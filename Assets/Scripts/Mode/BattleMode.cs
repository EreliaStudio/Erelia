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
	private BattleUnitManager battleUnitManager;

	public BattleSetup CurrentSetup => currentSetup;
	public BattleContext BattleContext => battleContext;

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

	public void Enter(BattleSetup setup)
	{
		if (setup == null || setup.Board == null)
		{
			return;
		}

		currentSetup = setup;
		battleContext = new BattleContext(currentSetup);
		battleContext.ClearRuntime();
		battleUnitManager = new BattleUnitManager(playerTeamRoot, enemyTeamRoot, battleUnitPrefab, battleContext);

		base.Enter();

		boardPresenter.transform.parent.transform.position = (Vector3)currentSetup.Board.WorldAnchor;
		boardPresenter.Assign(currentSetup.Board);

		Vector3Int anchor = currentSetup.Board.WorldAnchor;
		Vector3Int size = new Vector3Int(
			currentSetup.Board.Terrain.SizeX,
			currentSetup.Board.Terrain.SizeY,
			currentSetup.Board.Terrain.SizeZ);

		battlePlayerController.Bind(anchor, size, currentSetup.PlayerWorldPosition);
	}

	protected override void OnExit()
	{
		battleUnitManager?.Dispose();
		battlePlayerController.Unbind();
		battleUnitManager = null;
		battleContext = null;
		currentSetup = null;
	}
}
