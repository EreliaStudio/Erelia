using System.Collections.Generic;

namespace Erelia.Battle.Phase.Timeline
{
	/// <summary>
	/// Recurring phase that advances stamina until a unit becomes ready to act.
	/// </summary>
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		[System.NonSerialized] private readonly List<Erelia.Battle.Unit.Presenter> readyUnits =
			new List<Erelia.Battle.Unit.Presenter>();

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Timeline;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			if (battleData != null)
			{
				battleData.ActiveUnit = null;
			}
		}

		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			if (battleData?.Timeline == null)
			{
				return;
			}

			if (!battleData.Timeline.Tick(deltaTime, readyUnits) || readyUnits.Count == 0)
			{
				return;
			}

			Erelia.Battle.Unit.Presenter activeUnit = readyUnits[0];
			battleData.ActiveUnit = activeUnit;

			if (activeUnit?.Model == null)
			{
				return;
			}

			Orchestrator?.RequestTransition(
				activeUnit.Model.Team == Erelia.Battle.Unit.Team.Player
					? Erelia.Battle.Phase.Id.PlayerTurn
					: Erelia.Battle.Phase.Id.EnemyTurn);
		}
	}
}
