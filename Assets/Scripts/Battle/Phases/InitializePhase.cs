using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Initialization phase that prepares battle data.
	/// Tries to resolve the board, parses its playable cells, then transitions to the Placement phase.
	/// </summary>
	[System.Serializable]
	public sealed class InitializePhase : BattlePhase
	{
		/// <summary>
		/// Presenter used to resolve the battle board during initialization.
		/// </summary>
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

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
			pendingSetup = !TrySetupBattleData();
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

			pendingSetup = !TrySetupBattleData();
			if (!pendingSetup && manager != null)
			{
				manager.RequestTransition(BattlePhaseId.Placement);
			}
		}

		/// <summary>
		/// Attempts to resolve battle data for the current encounter.
		/// </summary>
		private bool TrySetupBattleData()
		{
			return true;
		}
	}
}
