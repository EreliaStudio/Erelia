
using System.Collections.Generic;
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
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[SerializeField] private bool enableMovementDebugLogs = true;

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
			SetStatus(BuildTurnStatus(activeUnit, "Player turn"));
			BindEndTurnButton();
			isMovementInProgress = false;
			ResolveBoardPresenter();
			LogDebug(
				$"Entering player turn for '{activeUnit.Creature?.DisplayName ?? activeUnit.name}' at {activeUnit.Cell} " +
				$"with {activeUnit.RemainingMovementPoints}/{activeUnit.MovementPoints} movement points.");
			RefreshMovementRange(activeUnit);
		}

		public override void Exit(Erelia.Battle.Orchestrator Orchestrator)
		{
			ClearMovementRange();
			isMovementInProgress = false;
			UnbindEndTurnButton();
			activeOrchestrator = null;
		}

		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;
			if (battleData?.ActiveUnit == null || battleData.ActiveUnit.IsAlive)
			{
				return;
			}

			battleData.ClearActiveUnit();
			activeOrchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
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
				if (controller != null && controller.HasHoveredCell())
				{
					LogDebug($"Confirm ignored for hovered cell {controller.HoveredCell()}: no reachable path was found.");
				}

				return;
			}

			LogDebug($"Starting movement to {targetCell}. Path: {FormatCoordinates(path)}");
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
				LogDebug("Movement range refresh aborted because battle data, board, active unit, or remaining movement is invalid.");
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
				activeUnit,
				enableMovementDebugLogs);
			LogDebug($"Pathfinder returned {reachablePathsByCell.Count} reachable cells.");
			foreach (KeyValuePair<Vector3Int, List<Vector3Int>> entry in reachablePathsByCell)
			{
				Vector3Int coordinate = entry.Key;
				if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
				{
					LogDebug($"Skipping movement mask at {coordinate}: outside board bounds.");
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				if (cell == null)
				{
					LogDebug($"Skipping movement mask at {coordinate}: board cell is null.");
					continue;
				}

				cell.AddMask(Erelia.Battle.Voxel.Mask.Type.MovementRange);
				movementMaskCoordinates.Add(coordinate);
				LogDebug($"Applied MovementRange mask to {coordinate}.");
			}

			Erelia.Battle.Board.Presenter resolvedBoardPresenter = ResolveBoardPresenter();
			if (resolvedBoardPresenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.PlayerTurn.Root] Cannot rebuild movement masks because the board presenter is missing.");
				return;
			}

			LogDebug($"Rebuilding board masks for {movementMaskCoordinates.Count} movement cells.");
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

			RefreshEndTurnButtonInteractivity();
			LogDebug(
				$"Movement finished at {movedUnit.Cell}. Consumed {pathCost} points. " +
				$"Remaining movement: {movedUnit.RemainingMovementPoints}/{movedUnit.MovementPoints}.");

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

		private void LogDebug(string message)
		{
			if (!enableMovementDebugLogs)
			{
				return;
			}

			Debug.Log("[Erelia.Battle.Phase.PlayerTurn.Root] " + message);
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
	}
}
