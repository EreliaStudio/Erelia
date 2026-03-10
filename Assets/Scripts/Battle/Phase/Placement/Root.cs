using System.Collections.Generic;
using Erelia.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Erelia.Battle.Phase.Placement
{
	/// <summary>
	/// Placement phase that computes placement zones and moves prebuilt units onto the board.
	/// </summary>
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		private static readonly Vector3 DefaultStationaryOffset = new Vector3(0.5f, 1f, 0.5f);
		private const float ReserveSideOffset = 2.5f;
		private const float ReserveHeight = 1f;

		[SerializeField] private GameObject hudRoot = null;
		[SerializeField] private Erelia.Battle.Phase.Placement.UI.SelectableCreatureCardGroupElement creatureCardGroup = null;
		[SerializeField] private Erelia.Battle.Phase.Core.UI.CreatureCardGroupElement enemyCreatureCardGroup = null;
		[SerializeField] private Button confirmPlacementButton = null;
		[SerializeField] private Erelia.Battle.Phase.Placement.PlacementMode placementMode =
			Erelia.Battle.Phase.Placement.PlacementMode.HalfBoard;
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

		[System.NonSerialized] private Dictionary<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit.Presenter> unitsByCreature;
		[System.NonSerialized] private Dictionary<Vector3Int, Erelia.Battle.Unit.Presenter> placedUnitsByCell;
		[System.NonSerialized] private Erelia.Battle.Orchestrator activeOrchestrator;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Placement;

		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			activeOrchestrator = Orchestrator;
			EnsureRuntimeLookups();
			BindConfirmPlacementButton();

			if (confirmPlacementButton != null)
			{
				confirmPlacementButton.gameObject.SetActive(true);
			}

			if (hudRoot != null)
			{
				hudRoot.SetActive(true);
			}

			PopulateCardGroups();
			TrySetupPlacementAreas();
			AutoPlaceEnemyUnits();
			SyncPlacedCardState();
			RefreshConfirmPlacementButton();
			InitializePlacementMaskCells();
		}

		public override void Exit(Erelia.Battle.Orchestrator Orchestrator)
		{
			ClearPlacementMaskCells();
			UnbindConfirmPlacementButton();
			creatureCardGroup?.ClearUnits();
			enemyCreatureCardGroup?.ClearUnits();
			activeOrchestrator = null;

			if (confirmPlacementButton != null)
			{
				confirmPlacementButton.gameObject.SetActive(false);
			}

			if (hudRoot != null)
			{
				hudRoot.SetActive(false);
			}
		}

		public override void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
			EnsureRuntimeLookups();

			Erelia.Battle.Unit.Presenter selectedUnit = creatureCardGroup != null
				? creatureCardGroup.GetSelectedUnit()
				: null;
			if (selectedUnit?.Model == null)
			{
				return;
			}

			if (!TryGetHoveredPlacementCell(controller, out Vector3Int targetCell))
			{
				return;
			}

			if (placedUnitsByCell.TryGetValue(targetCell, out Erelia.Battle.Unit.Presenter occupyingUnit) &&
				!ReferenceEquals(occupyingUnit, selectedUnit))
			{
				return;
			}

			if (!TryResolvePlacementWorldPosition(targetCell, out Vector3 worldPosition))
			{
				return;
			}

			if (selectedUnit.Model.HasCell)
			{
				placedUnitsByCell.Remove(selectedUnit.Model.Cell);
			}

			selectedUnit.Place(targetCell, worldPosition);
			placedUnitsByCell[targetCell] = selectedUnit;
			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced(selectedUnit));
			RefreshConfirmPlacementButton();
		}

		public override void OnCancel(Erelia.Battle.Player.BattlePlayerController controller)
		{
			EnsureRuntimeLookups();

			if (TryGetHoveredPlacedUnit(controller, out Erelia.Battle.Unit.Presenter hoveredUnit))
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

		private void PopulateCardGroups()
		{
			IReadOnlyList<Erelia.Battle.Unit.Presenter> units = Context.Instance.BattleData?.Units;
			PopulateCardGroup(
				creatureCardGroup,
				GetUnitsForTeam(units, Erelia.Battle.Unit.Team.Player));
			PopulateCardGroup(
				enemyCreatureCardGroup,
				GetUnitsForTeam(units, Erelia.Battle.Unit.Team.Enemy));
		}

		private void EnsureRuntimeLookups()
		{
			unitsByCreature = new Dictionary<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit.Presenter>();
			placedUnitsByCell = new Dictionary<Vector3Int, Erelia.Battle.Unit.Presenter>();

			IReadOnlyList<Erelia.Battle.Unit.Presenter> units = Context.Instance.BattleData?.Units;
			if (units == null)
			{
				return;
			}

			for (int i = 0; i < units.Count; i++)
			{
				Erelia.Battle.Unit.Presenter presenter = units[i];
				Erelia.Battle.Unit.Model model = presenter?.Model;
				if (model?.Creature == null)
				{
					continue;
				}

				unitsByCreature[model.Creature] = presenter;
				if (model.HasCell)
				{
					placedUnitsByCell[model.Cell] = presenter;
				}
			}
		}

		private void TrySetupPlacementAreas()
		{
			Erelia.Battle.Data battleData = Context.Instance.BattleData;
			Erelia.Battle.Phase.Info phaseInfo = battleData?.PhaseInfo;
			if (battleData?.Board == null || phaseInfo == null)
			{
				return;
			}

			if (!Erelia.Battle.Phase.Placement.PlacementCoordinateGenerator.TryGenerate(
					battleData.Board,
					phaseInfo.AcceptableCoordinates,
					placementMode,
					out List<Vector3Int> playerCoordinates,
					out List<Vector3Int> enemyCoordinates))
			{
				return;
			}

			phaseInfo.SetPlayerPlacementCoordinates(playerCoordinates);
			phaseInfo.SetEnemyPlacementCoordinates(enemyCoordinates);
		}

		private void InitializePlacementMaskCells()
		{
			IReadOnlyList<Vector3Int> playerPlacementCoordinates =
				Context.Instance.BattleData?.PhaseInfo?.PlayerPlacementCoordinates;
			if (playerPlacementCoordinates == null)
			{
				return;
			}

			for (int i = 0; i < playerPlacementCoordinates.Count; i++)
			{
				Vector3Int coordinate = playerPlacementCoordinates[i];
				if (!IsInsideBoard(coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = Context.Instance.BattleData.Board.Cells[coordinate.x, coordinate.y, coordinate.z];
				cell?.AddMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private void ClearPlacementMaskCells()
		{
			IReadOnlyList<Vector3Int> playerPlacementCoordinates =
				Context.Instance.BattleData?.PhaseInfo?.PlayerPlacementCoordinates;
			if (playerPlacementCoordinates == null)
			{
				return;
			}

			for (int i = 0; i < playerPlacementCoordinates.Count; i++)
			{
				Vector3Int coordinate = playerPlacementCoordinates[i];
				if (!IsInsideBoard(coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = Context.Instance.BattleData.Board.Cells[coordinate.x, coordinate.y, coordinate.z];
				cell.RemoveMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private bool TryGetHoveredPlacementCell(
			Erelia.Battle.Player.BattlePlayerController controller,
			out Vector3Int targetCell)
		{
			targetCell = default;

			if (controller == null || !controller.HasHoveredCell())
			{
				return false;
			}

			targetCell = controller.HoveredCell();
			if (!IsInsideBoard(targetCell))
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cell = Context.Instance.BattleData.Board.Cells[targetCell.x, targetCell.y, targetCell.z];
			return cell != null && cell.HasMask(Erelia.Battle.Voxel.Mask.Type.Placement);
		}

		private bool TryResolvePlacementWorldPosition(Vector3Int targetCell, out Vector3 worldPosition)
		{
			worldPosition = default;

			if (Context.Instance.BattleData.Board == null || !IsInsideBoard(targetCell))
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cell = Context.Instance.BattleData.Board.Cells[targetCell.x, targetCell.y, targetCell.z];
			if (cell == null)
			{
				return false;
			}

			Vector3 localPosition = new Vector3(targetCell.x, targetCell.y, targetCell.z) + ResolveStationaryOffset(cell);
			worldPosition = boardPresenter != null
				? boardPresenter.transform.TransformPoint(localPosition)
				: localPosition;
			return true;
		}

		private bool TryGetHoveredPlacedUnit(
			Erelia.Battle.Player.BattlePlayerController controller,
			out Erelia.Battle.Unit.Presenter unit)
		{
			unit = null;

			if (controller == null || !controller.HasHoveredCell() || placedUnitsByCell == null)
			{
				return false;
			}

			Vector3Int hoveredCell = controller.HoveredCell();
			if (!placedUnitsByCell.TryGetValue(hoveredCell, out unit) ||
				unit?.Model == null ||
				!unit.Model.HasCell)
			{
				return false;
			}

			return creatureCardGroup != null && creatureCardGroup.ContainsLinkedUnit(unit);
		}

		private Vector3 ResolveStationaryOffset(Erelia.Battle.Voxel.Cell cell)
		{
			if (cell == null)
			{
				return DefaultStationaryOffset;
			}

			if (!Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition))
			{
				return DefaultStationaryOffset;
			}

			if (definition is Erelia.Battle.Voxel.Definition battleDefinition &&
				battleDefinition.MaskShape != null)
			{
				return battleDefinition.MaskShape.GetCardinalPoint(
					Erelia.Battle.Voxel.CardinalPoint.Stationary,
					cell.Orientation,
					cell.FlipOrientation);
			}

			return DefaultStationaryOffset;
		}

		private void AutoPlaceEnemyUnits()
		{
			Erelia.Core.Creature.Instance.Model[] enemySlots = Context.Instance.BattleData?.EnemyTeam?.Slots;
			IReadOnlyList<Vector3Int> enemyPlacementCoordinates =
				Context.Instance.BattleData?.PhaseInfo?.EnemyPlacementCoordinates;
			if (enemySlots == null || enemyPlacementCoordinates == null || enemyPlacementCoordinates.Count == 0)
			{
				return;
			}

			var availableCoordinates = new List<Vector3Int>(enemyPlacementCoordinates.Count);
			for (int i = 0; i < enemyPlacementCoordinates.Count; i++)
			{
				availableCoordinates.Add(enemyPlacementCoordinates[i]);
			}

			ShuffleCoordinates(availableCoordinates);

			for (int i = 0; i < enemySlots.Length; i++)
			{
				Erelia.Core.Creature.Instance.Model creature = enemySlots[i];
				if (creature == null || creature.IsEmpty)
				{
					continue;
				}

				if (!TryGetUnitPresenter(creature, out Erelia.Battle.Unit.Presenter unit) || unit == null)
				{
					continue;
				}

				if (unit.Model.HasCell)
				{
					placedUnitsByCell[unit.Model.Cell] = unit;
					RemoveCoordinate(availableCoordinates, unit.Model.Cell);
					continue;
				}

				if (availableCoordinates.Count == 0)
				{
					break;
				}

				Vector3Int targetCell = availableCoordinates[availableCoordinates.Count - 1];
				availableCoordinates.RemoveAt(availableCoordinates.Count - 1);

				if (!TryResolvePlacementWorldPosition(targetCell, out Vector3 worldPosition))
				{
					continue;
				}

				unit.Place(targetCell, worldPosition);
				placedUnitsByCell[targetCell] = unit;
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

		private void UnplaceUnit(Erelia.Battle.Unit.Presenter unit)
		{
			if (unit?.Model == null ||
				!unit.Model.HasCell)
			{
				return;
			}

			placedUnitsByCell.Remove(unit.Model.Cell);
			unit.Stage(ResolveReserveWorldPosition(unit));
			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced(unit));
			RefreshConfirmPlacementButton();

			Erelia.Battle.Unit.Presenter selectedUnit = creatureCardGroup != null
				? creatureCardGroup.GetSelectedUnit()
				: null;
			if (ReferenceEquals(selectedUnit, unit))
			{
				Erelia.Core.Event.Bus.Emit(
					new Erelia.Battle.Phase.Placement.Event.PlacementCreatureSelected(null));
			}
		}

		private void SyncPlacedCardState()
		{
			if (unitsByCreature == null)
			{
				return;
			}

			foreach (KeyValuePair<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit.Presenter> pair in unitsByCreature)
			{
				Erelia.Battle.Unit.Model model = pair.Value?.Model;
				if (model == null || !model.HasCell)
				{
					continue;
				}

				Erelia.Core.Event.Bus.Emit(
					new Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced(pair.Value));
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
			if (!AreAllPlayerCreaturesPlaced())
			{
				return;
			}

			activeOrchestrator?.RequestTransition(Erelia.Battle.Phase.Id.Timeline);
		}

		private void RefreshConfirmPlacementButton()
		{
			if (confirmPlacementButton == null)
			{
				return;
			}

			confirmPlacementButton.interactable = AreAllPlayerCreaturesPlaced();
		}

		private bool AreAllPlayerCreaturesPlaced()
		{
			Erelia.Core.Creature.Instance.Model[] playerSlots = Context.Instance.SystemData?.PlayerTeam?.Slots;
			if (playerSlots == null || playerSlots.Length == 0)
			{
				return true;
			}

			int requiredCount = 0;
			int placedCount = 0;
			for (int i = 0; i < playerSlots.Length; i++)
			{
				Erelia.Core.Creature.Instance.Model creature = playerSlots[i];
				if (creature == null || creature.IsEmpty)
				{
					continue;
				}

				requiredCount++;
				if (TryGetUnitPresenter(creature, out Erelia.Battle.Unit.Presenter unit) &&
					unit?.Model != null &&
					unit.Model.HasCell)
				{
					placedCount++;
				}
			}

			return placedCount >= requiredCount;
		}

		private bool TryGetUnitPresenter(
			Erelia.Core.Creature.Instance.Model creature,
			out Erelia.Battle.Unit.Presenter presenter)
		{
			presenter = null;
			return creature != null &&
				unitsByCreature != null &&
				unitsByCreature.TryGetValue(creature, out presenter) &&
				presenter != null;
		}

		private Vector3 ResolveReserveWorldPosition(Erelia.Battle.Unit.Presenter presenter)
		{
			Erelia.Battle.Unit.Model model = presenter?.Model;
			return Erelia.Battle.Unit.StagingPositionUtility.ResolveWorldPosition(
				boardPresenter,
				Context.Instance.BattleData?.Board,
				model != null ? model.Team : Erelia.Battle.Unit.Team.Player,
				model != null ? model.TeamIndex : 0,
				GetTeamUnitCount(model != null ? model.Team : Erelia.Battle.Unit.Team.Player),
				ReserveSideOffset,
				ReserveHeight);
		}

		private int GetTeamUnitCount(Erelia.Battle.Unit.Team team)
		{
			IReadOnlyList<Erelia.Battle.Unit.Presenter> units = Context.Instance.BattleData?.Units;
			if (units == null)
			{
				return 0;
			}

			int count = 0;
			for (int i = 0; i < units.Count; i++)
			{
				if (units[i]?.Model != null && units[i].Model.Team == team)
				{
					count++;
				}
			}

			return count;
		}

		private bool IsInsideBoard(Vector3Int coordinate)
		{
			Erelia.Battle.Board.Model board = Context.Instance.BattleData?.Board;
			return board != null &&
				coordinate.x >= 0 && coordinate.x < board.SizeX &&
				coordinate.y >= 0 && coordinate.y < board.SizeY &&
				coordinate.z >= 0 && coordinate.z < board.SizeZ;
		}

		private static List<Erelia.Battle.Unit.Presenter> GetUnitsForTeam(
			IReadOnlyList<Erelia.Battle.Unit.Presenter> units,
			Erelia.Battle.Unit.Team team)
		{
			var filteredUnits = new List<Erelia.Battle.Unit.Presenter>();
			if (units == null)
			{
				return filteredUnits;
			}

			for (int i = 0; i < units.Count; i++)
			{
				Erelia.Battle.Unit.Presenter presenter = units[i];
				if (presenter?.Model != null && presenter.Model.Team == team)
				{
					filteredUnits.Add(presenter);
				}
			}

			return filteredUnits;
		}

		private static void PopulateCardGroup(
			Erelia.Battle.Phase.Core.UI.CreatureCardGroupElement cardGroup,
			IReadOnlyList<Erelia.Battle.Unit.Presenter> units)
		{
			if (cardGroup == null)
			{
				return;
			}

			cardGroup.ClearUnits();
			if (units == null)
			{
				return;
			}

			for (int i = 0; i < units.Count; i++)
			{
				cardGroup.AddUnit(units[i]);
			}
		}
	}
}
