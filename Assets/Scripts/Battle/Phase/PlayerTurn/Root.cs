
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.Phase.PlayerTurn
{
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		[SerializeField] private GameObject hudRoot;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement playerCreatureCardGroup;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement enemyCreatureCardGroup;
		[SerializeField] private Button endTurnButton;
		[SerializeField] private TMP_Text statusText;

		[System.NonSerialized] private Erelia.Battle.Orchestrator activeOrchestrator;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.PlayerTurn;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			activeOrchestrator = Orchestrator;

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (activeUnit == null)
			{
				Orchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
				return;
			}

			if (activeUnit.Side == Erelia.Battle.Side.Enemy)
			{
				Orchestrator?.RequestTransition(Erelia.Battle.Phase.Id.EnemyTurn);
				return;
			}

			PopulateHud(battleData);
			SetHudVisible(true);
			SetStatus(BuildTurnStatus(activeUnit, "Player turn"));
			BindEndTurnButton();
		}

		public override void Exit(Erelia.Battle.Orchestrator Orchestrator)
		{
			UnbindEndTurnButton();
			activeOrchestrator = null;
		}

		private void EndTurn()
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (activeUnit != null)
			{
				activeUnit.EndTurn();
				battleData.ClearActiveUnit();
			}

			activeOrchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
		}

		private static string BuildTurnStatus(Erelia.Battle.Unit.Presenter activeUnit, string fallback)
		{
			if (activeUnit == null || string.IsNullOrEmpty(activeUnit.Creature?.DisplayName))
			{
				return fallback;
			}

			return activeUnit.Creature.DisplayName + "'s turn";
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

		private void BindEndTurnButton()
		{
			if (endTurnButton == null)
			{
				return;
			}

			endTurnButton.onClick.RemoveListener(EndTurn);
			endTurnButton.onClick.AddListener(EndTurn);
			endTurnButton.interactable = true;
			endTurnButton.gameObject.SetActive(true);
		}

		private void UnbindEndTurnButton()
		{
			if (endTurnButton == null)
			{
				return;
			}

			endTurnButton.onClick.RemoveListener(EndTurn);
			endTurnButton.interactable = false;
			endTurnButton.gameObject.SetActive(false);
		}
	}
}
