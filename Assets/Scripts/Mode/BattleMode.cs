using UnityEngine;
using UnityEngine.InputSystem;

public sealed class BattleMode : Mode
{
	[SerializeField] private BoardPresenter boardPresenter;
	[SerializeField] private BattlePlayerController battlePlayerController;
	[SerializeField] private InputActionReference confirmAction;
	[SerializeField] private InputActionReference cancelAction;
	[SerializeField] private GameObject battleUnitPrefab;
	[SerializeField] private Transform playerTeamRoot;
	[SerializeField] private Transform enemyTeamRoot;
	[SerializeField] private BattleOrchestrator battleOrchestrator = new();

	private BattleContext battleContext;
	private BattleUnitManager battleUnitManager;
	private InputAction resolvedConfirmAction;
	private InputAction resolvedCancelAction;

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

		ResolveActions();
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
		battleOrchestrator.ConfigurePhaseInput(resolvedConfirmAction, resolvedCancelAction);
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

	private void ResolveActions()
	{
		resolvedConfirmAction = ResolveAction(confirmAction, "Confirm");
		resolvedCancelAction = ResolveAction(cancelAction, "Cancel");
	}

	private InputAction ResolveAction(InputActionReference actionReference, string actionName)
	{
		if (actionReference != null)
		{
			return actionReference.action;
		}

		InputActionAsset inputAsset = Resources.Load<InputActionAsset>("Input/BattlePlayer");
		if (inputAsset == null)
		{
			Logger.LogError($"[BattleMode] Could not resolve battle input asset while looking for '{actionName}'.", Logger.Severity.Warning, this);
			return null;
		}

		InputAction action = inputAsset.FindAction($"Player/{actionName}") ?? inputAsset.FindAction(actionName);
		if (action == null)
		{
			Logger.LogError($"[BattleMode] Could not resolve battle action '{actionName}'.", Logger.Severity.Warning, this);
		}

		return action;
	}
}
