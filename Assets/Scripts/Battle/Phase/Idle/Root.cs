using Erelia.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.Phase.Idle
{
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		[SerializeField] private GameObject hudRoot;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement playerCreatureCardGroup;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement enemyCreatureCardGroup;
		[SerializeField] private Button endTurnButton;
		[SerializeField] private TMP_Text statusText;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Idle;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			Erelia.Battle.Data battleData = Context.Instance.BattleData;
			PopulateHud(battleData);
			SetHudVisible(true);
			SetStatus("Waiting for the next turn");
			SetEndTurnButtonVisible(false);
		}

		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
			Erelia.Battle.Data battleData = Context.Instance.BattleData;
			if (battleData == null || battleData.ActiveUnit != null)
			{
				return;
			}

			Erelia.Battle.Unit.Presenter readyUnit = FindReadyUnit(battleData, deltaTime);
			if (readyUnit == null)
			{
				return;
			}

			battleData.SetActiveUnit(readyUnit);
			readyUnit.BeginTurn();

			Orchestrator?.RequestTransition(
				readyUnit.Side == Erelia.Battle.Side.Enemy
					? Erelia.Battle.Phase.Id.EnemyTurn
					: Erelia.Battle.Phase.Id.PlayerTurn);
		}

		private static Erelia.Battle.Unit.Presenter FindReadyUnit(Erelia.Battle.Data battleData, float deltaTime)
		{
			System.Collections.Generic.IReadOnlyList<Erelia.Battle.Unit.Presenter> units = battleData.Units;
			if (units == null)
			{
				return null;
			}

			Erelia.Battle.Unit.Presenter readyUnit = null;
			for (int i = 0; i < units.Count; i++)
			{
				Erelia.Battle.Unit.Presenter unit = units[i];
				if (unit == null || !unit.IsAlive || !unit.IsPlaced || unit.IsTakingTurn)
				{
					continue;
				}

				if (unit.TickStamina(deltaTime) && readyUnit == null)
				{
					readyUnit = unit;
				}
			}

			return readyUnit;
		}

		private void PopulateHud(Erelia.Battle.Data battleData)
		{
			playerCreatureCardGroup?.PopulateUnits(battleData?.PlayerUnits);
			enemyCreatureCardGroup?.PopulateUnits(battleData?.EnemyUnits);
		}

		private void SetHudVisible(bool isVisible)
		{
			if (hudRoot == null || hudRoot.activeSelf == isVisible)
			{
				return;
			}

			hudRoot.SetActive(isVisible);
		}

		private void SetStatus(string value)
		{
			if (statusText == null)
			{
				return;
			}

			bool hasValue = !string.IsNullOrEmpty(value);
			statusText.gameObject.SetActive(hasValue);
			statusText.text = hasValue ? value : string.Empty;
		}

		private void SetEndTurnButtonVisible(bool isVisible)
		{
			if (endTurnButton == null)
			{
				return;
			}

			endTurnButton.gameObject.SetActive(isVisible);
			endTurnButton.interactable = false;
		}
	}
}
