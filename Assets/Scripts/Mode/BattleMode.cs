using UnityEngine;

public sealed class BattleMode : Mode
{
	[SerializeField] private BoardPresenter boardPresenter;
	[SerializeField] private BattlePlayerController battlePlayerController;

	private BattleSetup currentSetup;
	private BattleContext battleContext;
	private BattleCoordinator battleCoordinator;
	private BattleOrchestrator battleOrchestrator;

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
	}

	public void Enter(BattleSetup setup)
	{
		if (setup == null || setup.Board == null)
		{
			return;
		}

		currentSetup = setup;
		battleContext = new BattleContext(currentSetup);
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
		battlePlayerController.Unbind();
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
