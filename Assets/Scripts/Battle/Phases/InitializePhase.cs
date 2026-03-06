namespace Erelia.Battle
{
	/// <summary>
	/// Initialization phase that prepares battle data.
	/// Tries to resolve the board, then transitions to the Placement phase.
	/// </summary>
	[System.Serializable]
	public sealed class InitializePhase : BattlePhase
	{
		/// <summary>
		/// Whether initialization is still pending.
		/// </summary>
		private bool pendingSetup;

		public override BattlePhaseId Id => BattlePhaseId.Initialize;

		/// <summary>
		/// Enters the initialize phase and prepares battle data.
		/// </summary>
		public override void Enter(BattleManager manager)
		{
			// Try to initialize battle data and request the next phase.
			pendingSetup = !TrySetupBattleData(manager);
			if (!pendingSetup && manager != null)
			{
				manager.RequestTransition(BattlePhaseId.Placement);
			}
		}

		/// <summary>
		/// Ticks the initialize phase until setup succeeds.
		/// </summary>
		public override void Tick(BattleManager manager, float deltaTime)
		{
			// Retry setup while it is still pending.
			if (!pendingSetup)
			{
				return;
			}

			pendingSetup = !TrySetupBattleData(manager);
			if (!pendingSetup && manager != null)
			{
				manager.RequestTransition(BattlePhaseId.Placement);
			}
		}

		/// <summary>
		/// Attempts to resolve battle data for the current encounter.
		/// </summary>
		private static bool TrySetupBattleData(BattleManager manager)
		{
			// Resolve battle board from context or presenter.
			Erelia.Core.Context context = Erelia.Core.Context.Instance;
			if (context == null)
			{
				return false;
			}

			Erelia.Battle.Data data = context.BattleData;
			Erelia.Battle.Board.Model board = data != null ? data.Board : null;
			if (board == null || board.Cells == null)
			{
				Erelia.Battle.Board.Presenter presenter =
					manager != null ? manager.GetComponentInChildren<Erelia.Battle.Board.Presenter>(true) : null;
				board = presenter != null ? presenter.Model : null;
			}

			if (board == null || board.Cells == null)
			{
				return false;
			}

			Erelia.Battle.Data battleData = context.GetOrCreateBattleData();
			if (battleData.Board == null)
			{
				battleData.Board = board;
			}

			return true;
		}
	}
}
