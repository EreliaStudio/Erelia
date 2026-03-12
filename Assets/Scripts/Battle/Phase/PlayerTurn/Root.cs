
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.Phase.PlayerTurn
{
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		private const string PlayerHudObjectName = "PlayerHUD";
		private const string HealthPanelObjectName = "HealthPanel";
		private const string PmPanelObjectName = "PMPanel";

		[SerializeField] private GameObject hudRoot;
		[SerializeField] private GameObject playerHudRoot;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement playerCreatureCardGroup;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement enemyCreatureCardGroup;
		[SerializeField] private Button endTurnButton;
		[SerializeField] private TMP_Text statusText;
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[SerializeField] private Erelia.Core.UI.ProgressBarView healthPanel;
		[SerializeField] private Erelia.Core.UI.ProgressBarView pmPanel;

		[System.NonSerialized] private Erelia.Battle.Orchestrator activeOrchestrator;
		[System.NonSerialized] private readonly List<Vector3Int> movementMaskCoordinates = new List<Vector3Int>();
		[System.NonSerialized] private Dictionary<Vector3Int, List<Vector3Int>> reachablePathsByCell =
			new Dictionary<Vector3Int, List<Vector3Int>>();
		[System.NonSerialized] private bool isMovementInProgress;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.PlayerTurn;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			activeOrchestrator = Orchestrator;

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (activeUnit == null || !activeUnit.IsAlive)
			{
				battleData?.ClearActiveUnit();
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
			SetPlayerHudVisible(true);
			RefreshPlayerHud(activeUnit);
			SetStatus(BuildTurnStatus(activeUnit, "Player turn"));
			BindEndTurnButton();
			isMovementInProgress = false;
			ResolveBoardPresenter();
			RefreshMovementRange(activeUnit);
		}

		public override void Exit(Erelia.Battle.Orchestrator Orchestrator)
		{
			ClearMovementRange();
			isMovementInProgress = false;
			UnbindEndTurnButton();
			SetPlayerHudVisible(false);
			activeOrchestrator = null;
		}

		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (activeUnit == null)
			{
				return;
			}

			if (!activeUnit.IsAlive)
			{
				battleData.ClearActiveUnit();
				activeOrchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
				return;
			}

			RefreshPlayerHud(activeUnit);
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

		public override void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
			if (isMovementInProgress)
			{
				return;
			}

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter activeUnit = battleData?.ActiveUnit;
			if (battleData == null ||
				activeUnit == null ||
				!activeUnit.IsAlive ||
				activeUnit.Side != Erelia.Battle.Side.Player ||
				activeUnit.RemainingMovementPoints <= 0)
			{
				return;
			}

			if (!TryGetHoveredReachableCell(controller, out Vector3Int targetCell) ||
				!reachablePathsByCell.TryGetValue(targetCell, out List<Vector3Int> path) ||
				path == null ||
				path.Count == 0)
			{
				return;
			}

			ClearMovementRange();
			isMovementInProgress = true;
			SetStatus(BuildMovementStatus(activeUnit));
			RefreshEndTurnButtonInteractivity();
			activeUnit.MoveAlongPath(
				path,
				battleData.Board,
				ResolveBoardPresenter(),
				() => OnMovementCompleted(activeUnit, path.Count));
		}

		private static string BuildTurnStatus(Erelia.Battle.Unit.Presenter activeUnit, string fallback)
		{
			if (activeUnit == null || string.IsNullOrEmpty(activeUnit.Creature?.DisplayName))
			{
				return fallback;
			}

			return activeUnit.Creature.DisplayName + "'s turn";
		}

		private static string BuildMovementStatus(Erelia.Battle.Unit.Presenter activeUnit)
		{
			if (activeUnit == null || string.IsNullOrEmpty(activeUnit.Creature?.DisplayName))
			{
				return "Moving";
			}

			return activeUnit.Creature.DisplayName + " is moving";
		}

		private static string BuildMovementBudgetStatus(Erelia.Battle.Unit.Presenter activeUnit)
		{
			if (activeUnit == null)
			{
				return "No movement remaining";
			}

			string displayName = !string.IsNullOrEmpty(activeUnit.Creature?.DisplayName)
				? activeUnit.Creature.DisplayName
				: "Unit";
			return $"{displayName}: {activeUnit.RemainingMovementPoints} movement points left";
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

		private void RefreshPlayerHud(Erelia.Battle.Unit.Presenter activeUnit)
		{
			UpdateProgressBar(ResolveHealthPanel(), activeUnit?.CurrentHealth ?? 0, activeUnit?.MaxHealth ?? 0);
			UpdateProgressBar(ResolvePmPanel(), activeUnit?.RemainingMovementPoints ?? 0, activeUnit?.MovementPoints ?? 0);
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
			endTurnButton.gameObject.SetActive(true);
			RefreshEndTurnButtonInteractivity();
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

		private void RefreshMovementRange(Erelia.Battle.Unit.Presenter activeUnit)
		{
			ClearMovementRange();

			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			Erelia.Battle.Board.Model board = battleData?.Board;
			if (battleData == null ||
				board == null ||
				activeUnit == null ||
				!activeUnit.IsAlive ||
				!activeUnit.IsPlaced ||
				activeUnit.RemainingMovementPoints <= 0)
			{
				return;
			}

			Erelia.Battle.MaskSpriteRegistry maskSpriteRegistry = Erelia.Battle.MaskSpriteRegistry.Instance;
			if (maskSpriteRegistry == null ||
				!maskSpriteRegistry.TryGetSprite(Erelia.Battle.Voxel.Mask.Type.MovementRange, out _))
			{
				Debug.LogWarning("[Erelia.Battle.Phase.PlayerTurn.Root] MovementRange mask sprite is missing from the mask sprite registry.");
			}

			reachablePathsByCell = Erelia.Battle.Board.MovementPathfinder.BuildReachablePaths(
				battleData,
				activeUnit);

			foreach (KeyValuePair<Vector3Int, List<Vector3Int>> entry in reachablePathsByCell)
			{
				Vector3Int coordinate = entry.Key;
				if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				if (cell == null)
				{
					continue;
				}

				cell.AddMask(Erelia.Battle.Voxel.Mask.Type.MovementRange);
				movementMaskCoordinates.Add(coordinate);
			}

			Erelia.Battle.Board.Presenter resolvedBoardPresenter = ResolveBoardPresenter();
			if (resolvedBoardPresenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.PlayerTurn.Root] Cannot rebuild movement masks because the board presenter is missing.");
				return;
			}

			resolvedBoardPresenter.RebuildMasks();
		}

		private void ClearMovementRange()
		{
			Erelia.Battle.Board.Model board = Erelia.Core.Context.Instance.BattleData?.Board;
			if (board != null)
			{
				for (int i = 0; i < movementMaskCoordinates.Count; i++)
				{
					Vector3Int coordinate = movementMaskCoordinates[i];
					if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
					{
						continue;
					}

					Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
					cell?.RemoveMask(Erelia.Battle.Voxel.Mask.Type.MovementRange);
				}

				ResolveBoardPresenter()?.RebuildMasks();
			}

			movementMaskCoordinates.Clear();
			reachablePathsByCell?.Clear();
		}

		private void OnMovementCompleted(Erelia.Battle.Unit.Presenter movedUnit, int pathCost)
		{
			isMovementInProgress = false;
			if (movedUnit == null)
			{
				RefreshEndTurnButtonInteractivity();
				return;
			}

			if (!movedUnit.TryConsumeMovementPoints(pathCost))
			{
				Debug.LogWarning(
					$"[Erelia.Battle.Phase.PlayerTurn.Root] Failed to consume {pathCost} movement points for '{movedUnit.Creature?.DisplayName ?? movedUnit.name}'.");
			}

			RefreshPlayerHud(movedUnit);
			RefreshEndTurnButtonInteractivity();

			if (movedUnit.RemainingMovementPoints > 0)
			{
				SetStatus(BuildMovementBudgetStatus(movedUnit));
				RefreshMovementRange(movedUnit);
				return;
			}

			SetStatus(BuildTurnStatus(movedUnit, "Player turn"));
		}

		private bool TryGetHoveredReachableCell(
			Erelia.Battle.Player.BattlePlayerController controller,
			out Vector3Int targetCell)
		{
			targetCell = default;
			Erelia.Battle.Board.Model board = Erelia.Core.Context.Instance.BattleData?.Board;
			if (controller == null || !controller.HasHoveredCell() || board == null)
			{
				return false;
			}

			targetCell = controller.HoveredCell();
			if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, targetCell))
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cell = board.Cells[targetCell.x, targetCell.y, targetCell.z];
			return cell != null && cell.HasMask(Erelia.Battle.Voxel.Mask.Type.MovementRange);
		}

		private Erelia.Battle.Board.Presenter ResolveBoardPresenter()
		{
			if (boardPresenter == null)
			{
				boardPresenter = Object.FindFirstObjectByType<Erelia.Battle.Board.Presenter>();
			}

			return boardPresenter;
		}

		private static string FormatCoordinates(IReadOnlyList<Vector3Int> coordinates)
		{
			if (coordinates == null || coordinates.Count == 0)
			{
				return "(none)";
			}

			var parts = new string[coordinates.Count];
			for (int i = 0; i < coordinates.Count; i++)
			{
				parts[i] = coordinates[i].ToString();
			}

			return string.Join(" -> ", parts);
		}

		private void RefreshEndTurnButtonInteractivity()
		{
			if (endTurnButton == null)
			{
				return;
			}

			endTurnButton.interactable = !isMovementInProgress;
		}

		private GameObject ResolvePlayerHudRoot()
		{
			if (playerHudRoot == null && hudRoot != null)
			{
				Transform playerHudTransform = hudRoot.transform.Find(PlayerHudObjectName);
				if (playerHudTransform != null)
				{
					playerHudRoot = playerHudTransform.gameObject;
				}
			}

			return playerHudRoot;
		}

		private Erelia.Core.UI.ProgressBarView ResolveHealthPanel()
		{
			if (healthPanel == null)
			{
				healthPanel = ResolveProgressBar(HealthPanelObjectName);
			}

			return healthPanel;
		}

		private Erelia.Core.UI.ProgressBarView ResolvePmPanel()
		{
			if (pmPanel == null)
			{
				pmPanel = ResolveProgressBar(PmPanelObjectName);
			}

			return pmPanel;
		}

		private Erelia.Core.UI.ProgressBarView ResolveProgressBar(string panelObjectName)
		{
			GameObject resolvedPlayerHudRoot = ResolvePlayerHudRoot();
			if (resolvedPlayerHudRoot == null)
			{
				return null;
			}

			Transform panelTransform = resolvedPlayerHudRoot.transform.Find(panelObjectName);
			return panelTransform != null
				? panelTransform.GetComponent<Erelia.Core.UI.ProgressBarView>()
				: null;
		}

		private static void UpdateProgressBar(
			Erelia.Core.UI.ProgressBarView progressBar,
			int currentValue,
			int maxValue)
		{
			if (progressBar == null)
			{
				return;
			}

			int clampedMaxValue = Mathf.Max(0, maxValue);
			int clampedCurrentValue = Mathf.Clamp(currentValue, 0, clampedMaxValue);
			float ratio01 = clampedMaxValue > 0
				? (float)clampedCurrentValue / clampedMaxValue
				: 0f;

			progressBar.SetProgress(ratio01);
			progressBar.SetLabel($"{clampedCurrentValue}/{clampedMaxValue}");
		}
	}
}
