using System;

public sealed class BattleCoordinator
{
	private readonly BattleContext context;
	private readonly BattleOrchestrator orchestrator;
	private readonly PlacementPhase placementPhase;
	private readonly TurnIdlePhase turnIdlePhase;
	private readonly PlayerTurnPhase playerTurnPhase;
	private readonly EnemyTurnPhase enemyTurnPhase;
	private readonly ResolveActionPhase resolveActionPhase;
	private readonly ResultPhase resultPhase;
	private readonly Action onBattleEnded;

	public BattleCoordinator(
		BattleContext p_context,
		BattleOrchestrator p_orchestrator,
		BoardPresenter p_boardPresenter, 
		BattlePlayerController p_playerController,
		Action p_onBattleEnded)
	{
		context = p_context ?? throw new ArgumentNullException(nameof(p_context));
		orchestrator = p_orchestrator ?? throw new ArgumentNullException(nameof(p_orchestrator));
		onBattleEnded = p_onBattleEnded;

		placementPhase = new PlacementPhase(context, p_boardPresenter ?? throw new ArgumentNullException(nameof(p_boardPresenter)), p_playerController ?? throw new ArgumentNullException(nameof(p_playerController)));
		turnIdlePhase = new TurnIdlePhase(context);
		playerTurnPhase = new PlayerTurnPhase(context, p_playerController);
		enemyTurnPhase = new EnemyTurnPhase(context);
		resolveActionPhase = new ResolveActionPhase(context);
		resultPhase = new ResultPhase(context);

		placementPhase.PlacementConfirmed += OnPlacementConfirmed;
		turnIdlePhase.UnitReady += OnUnitReady;
		playerTurnPhase.ActionChosen += OnActionChosen;
		enemyTurnPhase.ActionChosen += OnActionChosen;
		resolveActionPhase.Resolved += OnResolved;
		resultPhase.ResultAcknowledged += OnResultAcknowledged;
	}

	public BattlePhase CurrentPhase => orchestrator.CurrentPhase;

	public void Start()
	{
		orchestrator.TransitionTo(placementPhase);
	}

	public void Stop()
	{
		context.ActiveUnit = null;
		context.PendingAction = null;
	}

	public void Tick(float p_deltaTime)
	{
		orchestrator.Tick(p_deltaTime);
	}

	private void OnPlacementConfirmed()
	{
		if (context.TryResolveBattleResult())
		{
			orchestrator.TransitionTo(resultPhase);
			return;
		}

		orchestrator.TransitionTo(turnIdlePhase);
	}

	private void OnUnitReady(BattleUnit p_unit)
	{
		if (p_unit == null)
		{
			return;
		}

		context.ActiveUnit = p_unit;
		orchestrator.TransitionTo(IsPlayerUnit(p_unit) ? playerTurnPhase : enemyTurnPhase);
	}

	private void OnActionChosen(BattleAction p_action)
	{
		if (p_action == null)
		{
			return;
		}

		context.PendingAction = p_action;
		orchestrator.TransitionTo(resolveActionPhase);
	}

	private void OnResolved()
	{
		if (context.TryResolveBattleResult())
		{
			orchestrator.TransitionTo(resultPhase);
			return;
		}

		context.ActiveUnit = null;
		orchestrator.TransitionTo(turnIdlePhase);
	}

	private void OnResultAcknowledged()
	{
		onBattleEnded?.Invoke();
	}

	private static bool IsPlayerUnit(BattleUnit p_unit)
	{
		return p_unit != null && p_unit.Side == BattleSide.Player;
	}
}
