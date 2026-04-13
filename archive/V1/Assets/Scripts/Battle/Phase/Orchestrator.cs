using UnityEngine;

namespace Erelia.Battle
{
	public sealed class Orchestrator : MonoBehaviour
	{
		public enum BattleOutcome
		{
			None = 0,
			Victory = 1,
			Defeat = 2
		}

		[SerializeField] private Erelia.Battle.Phase.Id startPhase = Erelia.Battle.Phase.Id.Initialize;

		[SerializeField] private Erelia.Battle.Phase.Registry phaseRegistry = new Erelia.Battle.Phase.Registry();
		[SerializeField] private Erelia.Battle.Player.BattlePlayerController playerController;

		private Erelia.Battle.Phase.Root currentPhase;

		private Erelia.Battle.Phase.Id? pendingPhase;
		private Erelia.Battle.DecidedAction pendingDecidedAction;
		private BattleOutcome pendingBattleOutcome;

		public Erelia.Battle.Phase.Id CurrentPhaseId => currentPhase != null ? currentPhase.Id : Erelia.Battle.Phase.Id.None;

		public Erelia.Battle.Phase.Root CurrentPhase => currentPhase;

		private void Awake()
		{
			if (playerController == null)
			{
				Debug.LogWarning("[Erelia.Battle.Orchestrator] Battle player controller is not assigned.", this);
			}

			pendingDecidedAction = null;
			pendingBattleOutcome = BattleOutcome.None;

			if (startPhase != Erelia.Battle.Phase.Id.None)
			{
				TransitionTo(startPhase);
			}
		}

		private void OnEnable()
		{
			Erelia.Core.Event.Bus.Subscribe<Erelia.Core.Event.PlayerVictoryEvent>(HandlePlayerVictory);
			Erelia.Core.Event.Bus.Subscribe<Erelia.Core.Event.PlayerDefeatEvent>(HandlePlayerDefeat);
		}

		private void OnDisable()
		{
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Core.Event.PlayerVictoryEvent>(HandlePlayerVictory);
			Erelia.Core.Event.Bus.Unsubscribe<Erelia.Core.Event.PlayerDefeatEvent>(HandlePlayerDefeat);
		}

		private void Update()
		{
			if (pendingPhase.HasValue)
			{
				Erelia.Battle.Phase.Id next = pendingPhase.Value;
				pendingPhase = null;

				TransitionTo(next);
			}

			currentPhase?.Tick(this, Time.deltaTime);
		}

		public bool RequestTransition(Erelia.Battle.Phase.Id next)
		{
			if (next == Erelia.Battle.Phase.Id.None)
			{
				Debug.LogWarning("[Erelia.Battle.Orchestrator] Cannot transition to None.", this);
				return false;
			}

			if (next == CurrentPhaseId)
			{
				return false;
			}

			if (phaseRegistry == null || !phaseRegistry.TryGetPhase(next, out _))
			{
				Debug.LogWarning($"[Erelia.Battle.Orchestrator] Phase {next} not registered.", this);
				return false;
			}

			pendingPhase = next;
			return true;
		}

		public bool SubmitDecidedAction(Erelia.Battle.DecidedAction decidedAction)
		{
			if (decidedAction == null)
			{
				Debug.LogWarning("[Erelia.Battle.Orchestrator] Cannot submit a null decided action.", this);
				return false;
			}

			if (pendingDecidedAction != null)
			{
				Debug.LogWarning("[Erelia.Battle.Orchestrator] A decided action is already pending resolution.", this);
				return false;
			}

			pendingDecidedAction = decidedAction;
			if (RequestTransition(Erelia.Battle.Phase.Id.ResolveAction))
			{
				return true;
			}

			pendingDecidedAction = null;
			return false;
		}

		public bool SubmitBattleOutcome(BattleOutcome outcome)
		{
			if (outcome == BattleOutcome.None)
			{
				Debug.LogWarning("[Erelia.Battle.Orchestrator] Cannot submit an empty battle outcome.", this);
				return false;
			}

			if (pendingBattleOutcome != BattleOutcome.None)
			{
				Debug.LogWarning("[Erelia.Battle.Orchestrator] A battle outcome is already pending.", this);
				return false;
			}

			pendingBattleOutcome = outcome;
			if (RequestTransition(Erelia.Battle.Phase.Id.Result))
			{
				return true;
			}

			pendingBattleOutcome = BattleOutcome.None;
			return false;
		}

		public bool TryConsumeDecidedAction(out Erelia.Battle.DecidedAction decidedAction)
		{
			decidedAction = pendingDecidedAction;
			pendingDecidedAction = null;
			return decidedAction != null;
		}

		public bool TryConsumeBattleOutcome(out BattleOutcome outcome)
		{
			outcome = pendingBattleOutcome;
			pendingBattleOutcome = BattleOutcome.None;
			return outcome != BattleOutcome.None;
		}

		public bool RequestDecisionPhase()
		{
			Erelia.Battle.BattleState battle = Erelia.Core.GameContext.Instance?.Battle;
			Erelia.Battle.Unit.Presenter activeUnit = battle?.ActiveUnit;
			if (activeUnit == null || !activeUnit.IsAlive || !activeUnit.IsTakingTurn)
			{
				if (battle != null && activeUnit != null)
				{
					battle.ClearActiveUnit();
				}

				return RequestTransition(Erelia.Battle.Phase.Id.Idle);
			}

			return RequestTransition(
				activeUnit.Side == Erelia.Battle.Side.Enemy
					? Erelia.Battle.Phase.Id.EnemyTurn
					: Erelia.Battle.Phase.Id.PlayerTurn);
		}

		public bool TryGetPhase(Erelia.Battle.Phase.Id id, out Erelia.Battle.Phase.Root phase)
		{
			if (phaseRegistry == null)
			{
				phase = null;
				return false;
			}

			return phaseRegistry.TryGetPhase(id, out phase);
		}

		private void TransitionTo(Erelia.Battle.Phase.Id next)
		{
			if (next == Erelia.Battle.Phase.Id.None)
			{
				return;
			}

			if (phaseRegistry == null || !phaseRegistry.TryGetPhase(next, out Erelia.Battle.Phase.Root target))
			{
				Debug.LogWarning($"[Erelia.Battle.Orchestrator] Phase {next} not registered.", this);
				return;
			}

			if (currentPhase == target)
			{
				return;
			}

			currentPhase?.Exit(this);

			currentPhase = target;

			playerController.SetPhaseController(currentPhase);

			currentPhase.Enter(this);
		}

		private void HandlePlayerVictory(Erelia.Core.Event.PlayerVictoryEvent evt)
		{
			Debug.Log("Victory !");
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.ExplorationSceneDataRequest());
		}

		private void HandlePlayerDefeat(Erelia.Core.Event.PlayerDefeatEvent evt)
		{
			Debug.Log("Defeat ...");

			Erelia.Exploration.ExplorationState exploration = Erelia.Core.GameContext.Instance?.Exploration;
			if (exploration?.Player != null && exploration.TryGetSafePosition(out Vector3 safePosition))
			{
				exploration.Player.SetWorldPosition(safePosition);
			}
			else
			{
				Debug.LogWarning("[Erelia.Battle.Orchestrator] Safe exploration position is not available on defeat.", this);
			}

			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.ExplorationSceneDataRequest());
		}
	}
}


