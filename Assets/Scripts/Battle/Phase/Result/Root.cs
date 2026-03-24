using UnityEngine;

namespace Erelia.Battle.Phase.Result
{
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		[SerializeField] private GameObject battleHudRoot;
		[SerializeField] private Erelia.Battle.UI.BattleResultHud battleResultHud;
		[System.NonSerialized]
		private Erelia.Battle.Orchestrator.BattleOutcome currentOutcome = Erelia.Battle.Orchestrator.BattleOutcome.None;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Result;

		public override void Enter(Erelia.Battle.Orchestrator orchestrator)
		{
			currentOutcome = Erelia.Battle.Orchestrator.BattleOutcome.None;

			if (battleHudRoot != null)
			{
				battleHudRoot.SetActive(false);
			}

			if (battleResultHud == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Result.Root] Battle result HUD is not assigned.");
				RestoreBattleHud();
				return;
			}

			if (orchestrator == null || !orchestrator.TryConsumeBattleOutcome(out currentOutcome))
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Result.Root] Battle outcome is missing for the result phase.");
				RestoreBattleHud();
				return;
			}

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			battleData?.FeatProgressTracker.FinalizeBattle(
				Erelia.Core.Context.Instance.SystemData?.PlayerTeam,
				battleData.PlayerUnits);
			battleResultHud.CloseRequested -= HandleCloseRequested;
			battleResultHud.CloseRequested += HandleCloseRequested;
			battleResultHud.Show(ResolveTitle(currentOutcome), battleData?.FeatProgressTracker.CreatureResults);
		}

		public override void Exit(Erelia.Battle.Orchestrator orchestrator)
		{
			if (battleResultHud != null)
			{
				battleResultHud.CloseRequested -= HandleCloseRequested;
				battleResultHud.Hide();
			}

			RestoreBattleHud();
			currentOutcome = Erelia.Battle.Orchestrator.BattleOutcome.None;
		}

		private void HandleCloseRequested()
		{
			switch (currentOutcome)
			{
				case Erelia.Battle.Orchestrator.BattleOutcome.Victory:
					Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.PlayerVictoryEvent());
					return;

				case Erelia.Battle.Orchestrator.BattleOutcome.Defeat:
					Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.PlayerDefeatEvent());
					return;
			}
		}

		private void RestoreBattleHud()
		{
			if (battleHudRoot != null)
			{
				battleHudRoot.SetActive(true);
			}
		}

		private static string ResolveTitle(Erelia.Battle.Orchestrator.BattleOutcome outcome)
		{
			switch (outcome)
			{
				case Erelia.Battle.Orchestrator.BattleOutcome.Victory:
					return "Victory";

				case Erelia.Battle.Orchestrator.BattleOutcome.Defeat:
					return "Defeat";

				default:
					return "Battle Result";
			}
		}
	}
}
