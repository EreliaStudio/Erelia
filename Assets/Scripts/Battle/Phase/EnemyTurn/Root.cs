
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.Phase.EnemyTurn
{
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		[SerializeField] private GameObject hudRoot;
		[SerializeField] private GameObject playerHudRoot;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement playerCreatureCardGroup;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement enemyCreatureCardGroup;
		[SerializeField] private Button endTurnButton;
		[SerializeField] private TMP_Text statusText;
		[SerializeField] private float endTurnDelaySeconds = 0.75f;

		[System.NonSerialized] private float remainingDelay;
		[System.NonSerialized] private Erelia.Battle.Orchestrator activeOrchestrator;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.EnemyTurn;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			activeOrchestrator = Orchestrator;
			remainingDelay = UnityEngine.Mathf.Max(0f, endTurnDelaySeconds);

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (activeUnit == null || !activeUnit.IsAlive)
			{
				battleData?.ClearActiveUnit();
				Orchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
				return;
			}

			if (activeUnit.Side == Erelia.Battle.Side.Player)
			{
				Orchestrator?.RequestTransition(Erelia.Battle.Phase.Id.PlayerTurn);
				return;
			}

			PopulateHud(battleData);
			SetHudVisible(true);
			SetPlayerHudVisible(false);
			SetStatus(BuildTurnStatus(activeUnit, "Enemy turn"));
			SetEndTurnButtonVisible(false);
		}

		public override void Exit(Erelia.Battle.Orchestrator Orchestrator)
		{
			activeOrchestrator = null;
			remainingDelay = 0f;
		}

		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			if (battleData?.ActiveUnit == null)
			{
				return;
			}

			if (!battleData.ActiveUnit.IsAlive)
			{
				battleData.ClearActiveUnit();
				activeOrchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
				return;
			}

			remainingDelay -= deltaTime;
			if (remainingDelay > 0f)
			{
				return;
			}

			Erelia.Battle.Unit.Presenter activeUnit = battleData.ActiveUnit;
			activeUnit.EndTurn();
			battleData.ClearActiveUnit();
			activeOrchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
		}

		private static string BuildTurnStatus(Erelia.Battle.Unit.Presenter activeUnit, string fallback)
		{
			if (activeUnit == null || string.IsNullOrEmpty(activeUnit.Creature?.DisplayName))
			{
				return fallback;
			}

			return activeUnit.Creature.DisplayName + " is deciding";
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

		private void SetPlayerHudVisible(bool isVisible)
		{
			GameObject resolvedPlayerHudRoot = ResolvePlayerHudRoot();
			if (resolvedPlayerHudRoot == null || resolvedPlayerHudRoot.activeSelf == isVisible)
			{
				return;
			}

			resolvedPlayerHudRoot.SetActive(isVisible);
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

		private GameObject ResolvePlayerHudRoot()
		{
			if (playerHudRoot == null && hudRoot != null)
			{
				Transform playerHudTransform = hudRoot.transform.Find("PlayerHUD");
				if (playerHudTransform != null)
				{
					playerHudRoot = playerHudTransform.gameObject;
				}
			}

			return playerHudRoot;
		}
	}
}
