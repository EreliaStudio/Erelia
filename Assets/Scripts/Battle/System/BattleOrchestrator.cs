using System;
using UnityEngine;

[Serializable]
public sealed class BattleOrchestrator : IDisposable
{
	[SerializeField] private CreatureTeamView playerTeamView;
	[SerializeField] private CreatureTeamView enemyTeamView;

	[SerializeField] private SetupPhaseController setupPhaseController;
	[SerializeField] private PlacementPhaseController placementPhaseController;
	[SerializeField] private IdlePhaseController idlePhaseController;
	[SerializeField] private PlayerTurnPhaseController playerTurnPhaseController;
	[SerializeField] private EnemyTurnPhaseController enemyTurnPhaseController;
	[SerializeField] private ResolutionPhaseController resolutionPhaseController;
	[SerializeField] private EndPhaseController endPhaseController;

	private readonly SetupPhase setupPhase = new();
	private readonly PlacementPhase placementPhase = new();
	private readonly IdlePhase idlePhase = new();
	private readonly PlayerTurnPhase playerTurnPhase = new();
	private readonly EnemyTurnPhase enemyTurnPhase = new();
	private readonly ResolutionPhase resolutionPhase = new();
	private readonly EndPhase endPhase = new(); 
	[SerializeField] private BattlePhaseInputRouter inputRouter = new();

	private IBattlePhase activePhase;
	private BattlePhaseController activeController;

	public BattleMode BattleMode { get; private set; }
	public BattleContext BattleContext { get; private set; }
	public BattleCoordinator Coordinator { get; private set; }
	public TurnContext TurnContext => BattleContext?.CurrentTurn;
	public CreatureTeamView PlayerTeamView => playerTeamView;
	public CreatureTeamView EnemyTeamView => enemyTeamView;

	public void Initialize(BattleMode battleMode, BattleContext battleContext)
	{
		Dispose();

		BattleMode = battleMode;
		BattleContext = battleContext;
		Coordinator = new BattleCoordinator();

		BindPhases();
		BindControllers();

		Coordinator.PhaseChanged += OnPhaseChanged;
		Coordinator.TransitionTo(BattlePhaseType.Setup);
	}

	public void Dispose()
	{
		if (Coordinator != null)
		{
			Coordinator.PhaseChanged -= OnPhaseChanged;
		}

		if (activeController != null)
		{
			activeController.Deactivate();
			activeController = null;
		}

		if (activePhase != null)
		{
			activePhase.Exit();
			activePhase = null;
		}

		inputRouter.Dispose();
		Coordinator = null;
		BattleContext = null;
		BattleMode = null;
	}

	public void TransitionTo(BattlePhaseType phaseType)
	{
		Coordinator.TransitionTo(phaseType);
	}

	public void Validate(UnityEngine.Object context)
	{
		ResolveTeamViews();
		inputRouter.Validate(context);

		if (playerTeamView == null)
			Logger.LogError("[BattleOrchestrator] playerTeamView is not assigned.", Logger.Severity.Critical, context);
		if (enemyTeamView == null)
			Logger.LogError("[BattleOrchestrator] enemyTeamView is not assigned.", Logger.Severity.Critical, context);
	}

	private void ResolveTeamViews()
	{
		if (playerTeamView != null && enemyTeamView != null)
		{
			return;
		}

		if (CreatureTeamView.TryResolveSceneTeamViews(out CreatureTeamView resolvedPlayer, out CreatureTeamView resolvedEnemy))
		{
			if (playerTeamView == null) playerTeamView = resolvedPlayer;
			if (enemyTeamView == null) enemyTeamView = resolvedEnemy;
		}
	}

	public void ConfigurePhaseInput()
	{
		inputRouter.Configure(this);
	}

	public bool TrySubmitPendingAction(BattleAction action)
	{
		if (TurnContext == null || !TurnContext.TrySetPendingAction(action))
		{
			return false;
		}

		TransitionTo(BattlePhaseType.Resolution);
		return true;
	}

	public BattleAction ConsumePendingAction()
	{
		return TurnContext?.ConsumePendingAction();
	}

	public bool TryBeginTurn(BattleUnit activeUnit, out BattlePhaseType phaseType)
	{
		phaseType = BattlePhaseType.Idle;
		if (TurnContext == null || activeUnit == null || activeUnit.IsDefeated)
		{
			return false;
		}

		BattleTurnRules.BeginTurn(BattleContext, activeUnit);
		phaseType = GetTurnPhaseType(activeUnit.Side);
		return true;
	}

	public bool TryBeginNextTurn(out BattlePhaseType phaseType)
	{
		phaseType = BattlePhaseType.Idle;
		if (BattleTurnRules.TryFindNextActiveUnit(BattleContext, out BattleUnit activeUnit) &&
			TryBeginTurn(activeUnit, out phaseType))
		{
			return true;
		}

		return false;
	}

	public bool CanContinueActiveTurn()
	{
		return BattleTurnRules.CanContinueTurn(BattleContext, TurnContext);
	}

	public BattlePhaseType GetCurrentTurnPhaseType()
	{
		return GetTurnPhaseType(TurnContext?.ActiveSide ?? BattleSide.Neutral);
	}

	public bool TryGetPhase(BattlePhaseType phaseType, out IBattlePhase phase)
	{
		phase = GetPhase(phaseType);
		return phase != null;
	}

	public bool TryGetController(BattlePhaseType phaseType, out BattlePhaseController controller)
	{
		controller = GetController(phaseType);
		return controller != null;
	}

	public bool TryGetActiveController(out BattlePhaseController controller)
	{
		controller = activeController;
		return controller != null;
	}

	private void BindPhases()
	{
		setupPhase.Bind(this);
		placementPhase.Bind(this);
		idlePhase.Bind(this);
		playerTurnPhase.Bind(this);
		enemyTurnPhase.Bind(this);
		resolutionPhase.Bind(this);
		endPhase.Bind(this);
	}

	private void BindControllers()
	{
		BindController(setupPhaseController);
		BindController(placementPhaseController);
		BindController(idlePhaseController);
		BindController(playerTurnPhaseController);
		BindController(enemyTurnPhaseController);
		BindController(resolutionPhaseController);
		BindController(endPhaseController);
	}

	private void OnPhaseChanged(BattlePhaseType phaseType)
	{
		if (activePhase != null)
		{
			activePhase.Exit();
		}

		if (activeController != null)
		{
			activeController.Deactivate();
		}

		activePhase = GetPhase(phaseType);
		activeController = GetController(phaseType);

		activePhase?.Enter();
		activeController?.Activate();
	}

	private void BindController(BattlePhaseController controller)
	{
		if (controller == null)
		{
			return;
		}

		controller.Bind(this);
		controller.Deactivate();
	}

	private IBattlePhase GetPhase(BattlePhaseType phaseType)
	{
		return phaseType switch
		{
			BattlePhaseType.Setup => setupPhase,
			BattlePhaseType.Placement => placementPhase,
			BattlePhaseType.Idle => idlePhase,
			BattlePhaseType.PlayerTurn => playerTurnPhase,
			BattlePhaseType.EnemyTurn => enemyTurnPhase,
			BattlePhaseType.Resolution => resolutionPhase,
			BattlePhaseType.End => endPhase,
			_ => null
		};
	}

	private BattlePhaseController GetController(BattlePhaseType phaseType)
	{
		return phaseType switch
		{
			BattlePhaseType.Setup => setupPhaseController,
			BattlePhaseType.Placement => placementPhaseController,
			BattlePhaseType.Idle => idlePhaseController,
			BattlePhaseType.PlayerTurn => playerTurnPhaseController,
			BattlePhaseType.EnemyTurn => enemyTurnPhaseController,
			BattlePhaseType.Resolution => resolutionPhaseController,
			BattlePhaseType.End => endPhaseController,
			_ => null
		};
	}

	private static BattlePhaseType GetTurnPhaseType(BattleSide side)
	{
		return side switch
		{
			BattleSide.Player => BattlePhaseType.PlayerTurn,
			BattleSide.Enemy => BattlePhaseType.EnemyTurn,
			_ => BattlePhaseType.Idle
		};
	}
}
