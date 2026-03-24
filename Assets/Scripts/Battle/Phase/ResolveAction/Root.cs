
namespace Erelia.Battle.Phase.ResolveAction
{
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		[UnityEngine.SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

		[System.NonSerialized] private Erelia.Battle.Orchestrator activeOrchestrator;
		[System.NonSerialized] private Erelia.Battle.DecidedAction executingAction;
		[System.NonSerialized] private bool isWaitingForAsyncCompletion;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.ResolveAction;

		public override void Enter(Erelia.Battle.Orchestrator orchestrator)
		{
			activeOrchestrator = orchestrator;
			isWaitingForAsyncCompletion = false;
			executingAction = null;

			if (orchestrator == null || !orchestrator.TryConsumeDecidedAction(out Erelia.Battle.DecidedAction decidedAction))
			{
				CompleteResolution();
				return;
			}

			executingAction = decidedAction;
			ExecuteAction(decidedAction);
		}

		public override void Exit(Erelia.Battle.Orchestrator orchestrator)
		{
			activeOrchestrator = null;
			executingAction = null;
			isWaitingForAsyncCompletion = false;
		}

		private void ExecuteAction(Erelia.Battle.DecidedAction decidedAction)
		{
			if (decidedAction == null)
			{
				CompleteResolution();
				return;
			}

			switch (decidedAction.ActionKind)
			{
				case Erelia.Battle.DecidedAction.Kind.Move:
					ExecuteMovement(decidedAction);
					return;

				case Erelia.Battle.DecidedAction.Kind.Attack:
					ExecuteAttack(decidedAction);
					return;

				case Erelia.Battle.DecidedAction.Kind.EndTurn:
					ExecuteEndTurn(decidedAction);
					return;

				default:
					CompleteResolution();
					return;
			}
		}

		private void ExecuteMovement(Erelia.Battle.DecidedAction decidedAction)
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter actor = decidedAction.Actor;
			if (battleData == null ||
				battleData.Board == null ||
				actor == null ||
				!actor.IsAlive ||
				!actor.IsPlaced ||
				decidedAction.MovementCost <= 0 ||
				decidedAction.MovementCost > actor.RemainingMovementPoints)
			{
				CompleteResolution();
				return;
			}

			isWaitingForAsyncCompletion = true;
			actor.MoveAlongPath(
				decidedAction.MovementPath,
				battleData.Board,
				ResolveBoardPresenter(),
				() => OnMovementResolved(actor, decidedAction.MovementCost));
		}

		private void ExecuteAttack(Erelia.Battle.DecidedAction decidedAction)
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter actor = decidedAction.Actor;
			Erelia.Battle.Attack.Definition attack = decidedAction.Attack;
			if (battleData == null ||
				actor == null ||
				attack == null ||
				!actor.IsAlive ||
				!actor.IsPlaced ||
				!actor.TryConsumeActionPoints(attack.ActionPointCost))
			{
				CompleteResolution();
				return;
			}

			Erelia.Battle.Attack.TargetingUtility.ApplyAttack(
				battleData,
				actor,
				attack,
				decidedAction.TargetCell);

			CompleteResolution();
		}

		private void ExecuteEndTurn(Erelia.Battle.DecidedAction decidedAction)
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter actor = decidedAction.Actor;
			actor?.EndTurn();
			battleData?.FeatProgressTracker.RegisterTurnEnded(actor, battleData.Units);
			battleData?.ClearActiveUnit();
			CompleteResolution();
		}

		private void OnMovementResolved(Erelia.Battle.Unit.Presenter actor, int pathCost)
		{
			isWaitingForAsyncCompletion = false;

			if (actor != null && pathCost > 0 && !actor.TryConsumeMovementPoints(pathCost))
			{
				UnityEngine.Debug.LogWarning(
					$"[Erelia.Battle.Phase.ResolveAction.Root] Failed to consume {pathCost} movement points for '{actor.Creature?.DisplayName ?? actor.name}'.");
			}

			CompleteResolution();
		}

		private void CompleteResolution()
		{
			if (isWaitingForAsyncCompletion)
			{
				return;
			}

			executingAction = null;

			if (activeOrchestrator == null)
			{
				return;
			}

			if (TryGetBattleOutcome(
				Erelia.Core.Context.Instance.BattleData,
				out Erelia.Battle.Orchestrator.BattleOutcome outcome))
			{
				activeOrchestrator.SubmitBattleOutcome(outcome);
				return;
			}

			activeOrchestrator.RequestDecisionPhase();
		}

		private Erelia.Battle.Board.Presenter ResolveBoardPresenter()
		{
			if (boardPresenter == null)
			{
				boardPresenter = UnityEngine.Object.FindFirstObjectByType<Erelia.Battle.Board.Presenter>();
			}

			return boardPresenter;
		}

		private static bool TryGetBattleOutcome(
			Erelia.Battle.Data battleData,
			out Erelia.Battle.Orchestrator.BattleOutcome outcome)
		{
			if (battleData == null)
			{
				outcome = Erelia.Battle.Orchestrator.BattleOutcome.None;
				return false;
			}

			bool hasLivingPlayerUnit = HasLivingUnit(battleData?.PlayerUnits);
			bool hasLivingEnemyUnit = HasLivingUnit(battleData?.EnemyUnits);

			if (!hasLivingPlayerUnit)
			{
				outcome = Erelia.Battle.Orchestrator.BattleOutcome.Defeat;
				return true;
			}

			if (!hasLivingEnemyUnit)
			{
				outcome = Erelia.Battle.Orchestrator.BattleOutcome.Victory;
				return true;
			}

			outcome = Erelia.Battle.Orchestrator.BattleOutcome.None;
			return false;
		}

		private static bool HasLivingUnit(System.Collections.Generic.IReadOnlyList<Erelia.Battle.Unit.Presenter> units)
		{
			if (units == null)
			{
				return false;
			}

			for (int i = 0; i < units.Count; i++)
			{
				Erelia.Battle.Unit.Presenter unit = units[i];
				if (unit != null && unit.IsAlive)
				{
					return true;
				}
			}

			return false;
		}
	}
}
