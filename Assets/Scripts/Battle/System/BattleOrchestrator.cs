using System;
using UnityEngine;

[Serializable]
public sealed class BattleOrchestrator : IDisposable
{
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

	private IBattlePhase activePhase;
	private BattlePhaseController activeController;

	public BattleMode BattleMode { get; private set; }
	public BattleContext BattleContext { get; private set; }
	public BattleCoordinator Coordinator { get; private set; }

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
			activeController.SetActive(false);
			activeController = null;
		}

		if (activePhase != null)
		{
			activePhase.Exit();
			activePhase = null;
		}

		Coordinator = null;
		BattleContext = null;
		BattleMode = null;
	}

	public void TransitionTo(BattlePhaseType phaseType)
	{
		Coordinator.TransitionTo(phaseType);
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
			activeController.SetActive(false);
		}

		activePhase = GetPhase(phaseType);
		activeController = GetController(phaseType);

		activePhase?.Enter();
		activeController?.SetActive(true);
	}

	private void BindController(BattlePhaseController controller)
	{
		if (controller == null)
		{
			return;
		}

		controller.Bind(this);
		controller.SetActive(false);
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
}
