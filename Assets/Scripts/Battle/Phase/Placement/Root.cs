using Erelia.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.Phase.Placement
{
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		[SerializeField] private GameObject hudRoot = null;
		[SerializeField] private Erelia.Battle.Phase.Placement.UI.SelectableCreatureCardGroupElement creatureCardGroup = null;
		[SerializeField] private Erelia.Core.UI.CreatureCardGroupElement enemyCreatureCardGroup = null;
		[SerializeField] private Button confirmPlacementButton = null;
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

		[System.NonSerialized] private Erelia.Battle.Orchestrator activeOrchestrator;
		[System.NonSerialized] private List<Vector3Int> playerPlacementCoordinates;
		[System.NonSerialized] private List<Vector3Int> enemyPlacementCoordinates;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Placement;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			activeOrchestrator = Orchestrator;
			BuildPlacementCoordinates();
			BindConfirmPlacementButton();

			if (hudRoot == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Placement.Root] HUD root can't be empty");
			}
			else
			{
				hudRoot.SetActive(true);
			}

			if (creatureCardGroup == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Placement.Root] Creature card group can't be empty");
			}
			else
			{
				creatureCardGroup.PopulateUnits(Context.Instance.BattleData?.PlayerUnits);
			}

			enemyCreatureCardGroup?.PopulateUnits(Context.Instance.BattleData?.EnemyUnits);
			InitializeEnemyUnits();
			RefreshConfirmPlacementButton();
			InitializePlacementMaskCells();
		}

		public override void Exit(Erelia.Battle.Orchestrator Orchestrator)
		{
			ClearPlacementMaskCells();
			playerPlacementCoordinates = null;
			enemyPlacementCoordinates = null;
			UnbindConfirmPlacementButton();
			activeOrchestrator = null;

			if (hudRoot != null)
			{
				hudRoot.SetActive(false);
			}
		}

		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
		}

		public override void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
			Erelia.Battle.Data battleData = Context.Instance.BattleData;
			Erelia.Battle.Unit.Presenter selectedUnit = creatureCardGroup != null ? creatureCardGroup.GetSelectedUnit() : null;
			if (battleData == null || selectedUnit == null)
			{
				return;
			}

			if (!TryGetHoveredPlacementCell(controller, out Vector3Int targetCell))
			{
				return;
			}

			if (battleData.TryGetPlacedUnitAtCell(targetCell, out Erelia.Battle.Unit.Presenter occupyingUnit) &&
				!ReferenceEquals(occupyingUnit, selectedUnit))
			{
				return;
			}

			if (!Erelia.Battle.Board.UnitPlacementUtility.TryResolveWorldPosition(
					battleData.Board,
					boardPresenter,
					targetCell,
					out Vector3 worldPosition))
			{
				return;
			}

			selectedUnit.Place(targetCell, worldPosition);
			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementUnitPlaced(selectedUnit));
			RefreshConfirmPlacementButton();
		}

		public override void OnCancel(Erelia.Battle.Player.BattlePlayerController controller)
		{
			if (TryGetHoveredPlacedPlayerUnit(controller, out Erelia.Battle.Unit.Presenter hoveredUnit))
			{
				UnplaceUnit(hoveredUnit);
				return;
			}

			Erelia.Battle.Unit.Presenter selectedUnit = creatureCardGroup != null
				? creatureCardGroup.GetSelectedUnit()
				: null;
			if (selectedUnit == null)
			{
				return;
			}

			UnplaceUnit(selectedUnit);
		}

		private void InitializePlacementMaskCells()
		{
			Erelia.Battle.Board.Model board = Context.Instance.BattleData?.Board;
			if (playerPlacementCoordinates == null || board == null)
			{
				return;
			}

			for (int i = 0; i < playerPlacementCoordinates.Count; i++)
			{
				Vector3Int coordinate = playerPlacementCoordinates[i];
				if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				cell?.AddMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private void ClearPlacementMaskCells()
		{
			Erelia.Battle.Board.Model board = Context.Instance.BattleData?.Board;
			if (playerPlacementCoordinates == null || board == null)
			{
				return;
			}

			for (int i = 0; i < playerPlacementCoordinates.Count; i++)
			{
				Vector3Int coordinate = playerPlacementCoordinates[i];
				if (!Erelia.Battle.Board.UnitPlacementUtility.IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				cell?.RemoveMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private void BuildPlacementCoordinates()
		{
			playerPlacementCoordinates = null;
			enemyPlacementCoordinates = null;

			Erelia.Battle.Data battleData = Context.Instance.BattleData;
			if (battleData == null)
			{
				return;
			}

			if (!Erelia.Battle.Phase.Placement.PlacementListBuilder.TryBuildFromAcceptableCoordinates(
					battleData.AcceptableCoordinates,
					battleData.Board != null ? battleData.Board.SizeZ : 0,
					out playerPlacementCoordinates,
					out enemyPlacementCoordinates))
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Placement.Root] Failed to build placement coordinates from acceptable cells.");
			}
		}

		private bool TryGetHoveredPlacementCell(
			Erelia.Battle.Player.BattlePlayerController controller,
			out Vector3Int targetCell)
		{
			targetCell = default;
			Erelia.Battle.Board.Model board = Context.Instance.BattleData?.Board;
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
			return cell != null && cell.HasMask(Erelia.Battle.Voxel.Mask.Type.Placement);
		}

		private bool TryGetHoveredPlacedPlayerUnit(
			Erelia.Battle.Player.BattlePlayerController controller,
			out Erelia.Battle.Unit.Presenter unit)
		{
			unit = null;
			Erelia.Battle.Data battleData = Context.Instance.BattleData;
			if (controller == null || !controller.HasHoveredCell() || battleData == null)
			{
				return false;
			}

			if (!battleData.TryGetPlacedUnitAtCell(controller.HoveredCell(), out unit) ||
				unit == null ||
				unit.Side != Erelia.Battle.Side.Player)
			{
				return false;
			}

			return creatureCardGroup != null && creatureCardGroup.ContainsLinkedUnit(unit);
		}

		private void UnplaceUnit(Erelia.Battle.Unit.Presenter unit)
		{
			if (unit == null || unit.Side != Erelia.Battle.Side.Player || !unit.IsPlaced)
			{
				return;
			}

			unit.Unplace();
			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementUnitUnplaced(unit));
			RefreshConfirmPlacementButton();
		}

		private void InitializeEnemyUnits()
		{
			Erelia.Battle.Data battleData = Context.Instance.BattleData;
			IReadOnlyList<Erelia.Battle.Unit.Presenter> enemyUnits = battleData?.EnemyUnits;
			if (battleData == null || enemyUnits == null || enemyPlacementCoordinates == null || enemyPlacementCoordinates.Count == 0)
			{
				return;
			}

			var availableCoordinates = new List<Vector3Int>(enemyPlacementCoordinates.Count);
			for (int i = 0; i < enemyPlacementCoordinates.Count; i++)
			{
				availableCoordinates.Add(enemyPlacementCoordinates[i]);
			}

			ShuffleCoordinates(availableCoordinates);

			for (int i = 0; i < enemyUnits.Count; i++)
			{
				Erelia.Battle.Unit.Presenter unit = enemyUnits[i];
				if (unit == null)
				{
					continue;
				}

				if (unit.IsPlaced)
				{
					RemoveCoordinate(availableCoordinates, unit.Cell);
					continue;
				}

				if (availableCoordinates.Count == 0)
				{
					return;
				}

				Vector3Int targetCell = availableCoordinates[availableCoordinates.Count - 1];
				availableCoordinates.RemoveAt(availableCoordinates.Count - 1);

				if (!Erelia.Battle.Board.UnitPlacementUtility.TryResolveWorldPosition(
						battleData.Board,
						boardPresenter,
						targetCell,
						out Vector3 worldPosition))
				{
					continue;
				}

				unit.Place(targetCell, worldPosition);
			}
		}

		private static void ShuffleCoordinates(List<Vector3Int> coordinates)
		{
			if (coordinates == null)
			{
				return;
			}

			for (int i = coordinates.Count - 1; i > 0; i--)
			{
				int swapIndex = Random.Range(0, i + 1);
				Vector3Int temp = coordinates[i];
				coordinates[i] = coordinates[swapIndex];
				coordinates[swapIndex] = temp;
			}
		}

		private static void RemoveCoordinate(List<Vector3Int> coordinates, Vector3Int coordinate)
		{
			if (coordinates == null)
			{
				return;
			}

			for (int i = 0; i < coordinates.Count; i++)
			{
				if (coordinates[i] != coordinate)
				{
					continue;
				}

				coordinates.RemoveAt(i);
				return;
			}
		}

		private void BindConfirmPlacementButton()
		{
			if (confirmPlacementButton == null)
			{
				return;
			}

			confirmPlacementButton.onClick.RemoveListener(OnConfirmPlacementButtonClicked);
			confirmPlacementButton.onClick.AddListener(OnConfirmPlacementButtonClicked);
		}

		private void UnbindConfirmPlacementButton()
		{
			if (confirmPlacementButton == null)
			{
				return;
			}

			confirmPlacementButton.onClick.RemoveListener(OnConfirmPlacementButtonClicked);
			confirmPlacementButton.interactable = false;
		}

		private void OnConfirmPlacementButtonClicked()
		{
			if (!AreAllPlayerUnitsPlaced())
			{
				return;
			}

			activeOrchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Idle);
		}

		private void RefreshConfirmPlacementButton()
		{
			if (confirmPlacementButton == null)
			{
				return;
			}

			confirmPlacementButton.interactable = AreAllPlayerUnitsPlaced();
		}

		private bool AreAllPlayerUnitsPlaced()
		{
			System.Collections.Generic.IReadOnlyList<Erelia.Battle.Unit.Presenter> playerUnits =
				Context.Instance.BattleData?.PlayerUnits;
			if (playerUnits == null || playerUnits.Count == 0)
			{
				return true;
			}

			for (int i = 0; i < playerUnits.Count; i++)
			{
				Erelia.Battle.Unit.Presenter unit = playerUnits[i];
				if (unit != null && !unit.IsPlaced)
				{
					return false;
				}
			}

			return true;
		}

	}
}
