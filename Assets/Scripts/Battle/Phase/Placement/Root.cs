using System.Collections.Generic;
using Erelia.Core;
using UnityEngine;

namespace Erelia.Battle.Phase.Placement
{
	/// <summary>
	/// Placement phase that applies precomputed placement masks and handles unit placement.
	/// </summary>
	[System.Serializable]
	public sealed class Root : Erelia.Battle.Phase.Root
	{
		private static readonly Vector3 DefaultStationaryOffset = new Vector3(0.5f, 1f, 0.5f);

		[SerializeField] private GameObject hudRoot = null;
		[SerializeField] private Erelia.Battle.Phase.Placement.UI.SelectableCreatureCardGroupElement creatureCardGroup = null;

		public override Erelia.Battle.Phase.Id Id => Erelia.Battle.Phase.Id.Placement;

		/// <summary>
		/// Presenter used to access the battle Context.Instance.BattleData.Board.
		/// </summary>
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[System.NonSerialized] private Dictionary<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit> unitsByCreature;
		[System.NonSerialized] private Dictionary<Vector3Int, Erelia.Battle.Unit> placedUnitsByCell;
		[System.NonSerialized] private Erelia.Battle.Board.Model runtimeBoard;

		/// <summary>
		/// Enters the placement phase and applies placement masks.
		/// </summary>
		public override void Enter(Erelia.Battle.Orchestrator Orchestrator)
		{
			EnsureRuntimeState();

			if (hudRoot == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Placement.Root] HUD root can't be empty");
			}

			if (hudRoot != null)
			{
				hudRoot.SetActive(true);
			}

			if (creatureCardGroup == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Placement.Root] Creature card group can't be empty");
			}
			else
			{
				creatureCardGroup.PopulateCreatureCards(Context.Instance.SystemData?.PlayerTeam);
				SyncPlacedCardState();
			}

			InitializePlacementMaskCells();
		}

		/// <summary>
		/// Exits the placement phase and clears placement masks.
		/// </summary>
		public override void Exit(Erelia.Battle.Orchestrator Orchestrator)
		{
			ClearPlacementMaskCells();

			if (hudRoot != null)
			{
				hudRoot.SetActive(false);
			}
		}

		/// <summary>
		/// Ticks the placement phase until masks are applied.
		/// </summary>
		public override void Tick(Erelia.Battle.Orchestrator Orchestrator, float deltaTime)
		{
		}

		/// <summary>
		/// Handles confirm input during placement.
		/// </summary>
		public override void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
			EnsureRuntimeState();

			Erelia.Core.Creature.Instance.Model selectedCreature = creatureCardGroup != null ? creatureCardGroup.GetSelectedCreature() : null;
			if (selectedCreature == null)
			{
				return;
			}

			if (!TryGetHoveredPlacementCell(controller, out Vector3Int targetCell))
			{
				return;
			}

			if (placedUnitsByCell.TryGetValue(targetCell, out Erelia.Battle.Unit occupyingUnit) &&
				!ReferenceEquals(occupyingUnit.Creature, selectedCreature))
			{
				return;
			}

			if (!TryResolvePlacementWorldPosition(targetCell, out Vector3 worldPosition))
			{
				return;
			}

			if (!unitsByCreature.TryGetValue(selectedCreature, out Erelia.Battle.Unit unit) || unit == null)
			{
				if (!TryCreateUnit(selectedCreature, targetCell, worldPosition, out unit))
				{
					return;
				}

				unitsByCreature[selectedCreature] = unit;
			}
			else
			{
				if (unit.IsPlaced)
				{
					placedUnitsByCell.Remove(unit.Cell);
				}

				unit.Place(targetCell, worldPosition);
			}

			placedUnitsByCell[targetCell] = unit;
			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced(selectedCreature));
		}

		/// <summary>
		/// Handles cancel input during placement.
		/// </summary>
		public override void OnCancel(Erelia.Battle.Player.BattlePlayerController controller)
		{
			EnsureRuntimeState();

			if (TryGetHoveredPlacedCreature(controller, out Erelia.Core.Creature.Instance.Model hoveredCreature))
			{
				UnplaceCreature(hoveredCreature);
				return;
			}

			Erelia.Core.Creature.Instance.Model selectedCreature = creatureCardGroup != null
				? creatureCardGroup.GetSelectedCreature()
				: null;
			if (selectedCreature == null)
			{
				return;
			}

			UnplaceCreature(selectedCreature);
		}

		private void InitializePlacementMaskCells()
		{
			for (int i = 0; i < Context.Instance.BattleData.PhaseInfo.AcceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = Context.Instance.BattleData.PhaseInfo.AcceptableCoordinates[i];
				if (!IsInsideBoard(coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = Context.Instance.BattleData.Board.Cells[coordinate.x, coordinate.y, coordinate.z];
				if (!Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition))
				{
					continue;
				}

				if (!IsInPlacementPolicy(definition, coordinate.x, coordinate.y, coordinate.z) ||
					!HasAvailableSpace(coordinate.x, coordinate.y, coordinate.z))
				{
					continue;
				}

				cell.AddMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private void ClearPlacementMaskCells()
		{
			for (int i = 0; i < Context.Instance.BattleData.PhaseInfo.AcceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = Context.Instance.BattleData.PhaseInfo.AcceptableCoordinates[i];
				if (!IsInsideBoard(coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = Context.Instance.BattleData.Board.Cells[coordinate.x, coordinate.y, coordinate.z];
				cell.RemoveMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private void EnsureRuntimeState()
		{
			Erelia.Battle.Board.Model currentBoard = Context.Instance.BattleData != null
				? Context.Instance.BattleData.Board
				: null;

			if (unitsByCreature == null || placedUnitsByCell == null || !ReferenceEquals(runtimeBoard, currentBoard))
			{
				DisposeRuntimeUnits();
				unitsByCreature = new Dictionary<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit>();
				placedUnitsByCell = new Dictionary<Vector3Int, Erelia.Battle.Unit>();
				runtimeBoard = currentBoard;
			}
		}

		private void DisposeRuntimeUnits()
		{
			if (unitsByCreature == null)
			{
				return;
			}

			foreach (KeyValuePair<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit> pair in unitsByCreature)
			{
				Erelia.Battle.Unit unit = pair.Value;
				if (unit?.View == null)
				{
					continue;
				}

				if (Application.isPlaying)
				{
					Object.Destroy(unit.View);
				}
				else
				{
					Object.DestroyImmediate(unit.View);
				}
			}
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
			if (cell == null || !cell.HasMask(Erelia.Battle.Voxel.Mask.Type.Placement))
			{
				return false;
			}

			return true;
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

		private bool TryGetHoveredPlacedCreature(
			Erelia.Battle.Player.BattlePlayerController controller,
			out Erelia.Core.Creature.Instance.Model creature)
		{
			creature = null;

			if (controller == null || !controller.HasHoveredCell() || placedUnitsByCell == null)
			{
				return false;
			}

			Vector3Int hoveredCell = controller.HoveredCell();
			if (!placedUnitsByCell.TryGetValue(hoveredCell, out Erelia.Battle.Unit unit) ||
				unit == null ||
				!unit.IsPlaced)
			{
				return false;
			}

			creature = unit.Creature;
			return creature != null;
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

		private bool TryCreateUnit(
			Erelia.Core.Creature.Instance.Model creature,
			Vector3Int targetCell,
			Vector3 worldPosition,
			out Erelia.Battle.Unit unit)
		{
			unit = null;

			if (creature == null)
			{
				return false;
			}

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			if (registry == null ||
				!registry.TryGet(creature.SpeciesId, out Erelia.Core.Creature.Species species) ||
				species == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Placement.Root] Failed to resolve creature species.");
				return false;
			}

			if (species.Prefab == null)
			{
				Debug.LogWarning($"[Erelia.Battle.Phase.Placement.Root] Species '{species.DisplayName}' has no prefab.");
				return false;
			}

			GameObject view = Object.Instantiate(species.Prefab, GetUnitParent());
			view.name = species.DisplayName;

			Erelia.Core.Creature.Instance.Presenter presenter =
				view.GetComponent<Erelia.Core.Creature.Instance.Presenter>() ??
				view.GetComponentInChildren<Erelia.Core.Creature.Instance.Presenter>(true);
			if (presenter != null)
			{
				presenter.SetModel(creature);
			}

			unit = new Erelia.Battle.Unit(creature, targetCell, view);
			unit.Place(targetCell, worldPosition);
			return true;
		}

		private void UnplaceCreature(Erelia.Core.Creature.Instance.Model creature)
		{
			if (creature == null ||
				unitsByCreature == null ||
				!unitsByCreature.TryGetValue(creature, out Erelia.Battle.Unit unit) ||
				unit == null ||
				!unit.IsPlaced)
			{
				return;
			}

			placedUnitsByCell?.Remove(unit.Cell);
			unit.Unplace();
			Erelia.Core.Event.Bus.Emit(
				new Erelia.Battle.Phase.Placement.Event.PlacementCreatureUnplaced(creature));

			Erelia.Core.Creature.Instance.Model selectedCreature = creatureCardGroup != null
				? creatureCardGroup.GetSelectedCreature()
				: null;
			if (ReferenceEquals(selectedCreature, creature))
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

			foreach (KeyValuePair<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit> pair in unitsByCreature)
			{
				Erelia.Battle.Unit unit = pair.Value;
				if (unit == null || !unit.IsPlaced)
				{
					continue;
				}

				Erelia.Core.Event.Bus.Emit(
					new Erelia.Battle.Phase.Placement.Event.PlacementCreaturePlaced(pair.Key));
			}
		}

		private Transform GetUnitParent()
		{
			return boardPresenter != null ? boardPresenter.transform : null;
		}

		private bool IsInPlacementPolicy(Erelia.Core.VoxelKit.Definition definition, int x, int y, int z)
		{
			return z < Context.Instance.BattleData.Board.SizeZ / 2;
		}

		private bool HasAvailableSpace(int x, int y, int z)
		{
			const int PlayerHeight = 2;
			for (int deltaY = 1; deltaY < PlayerHeight; deltaY++)
			{
				int targetY = y + deltaY;
				if (targetY >= Context.Instance.BattleData.Board.SizeY)
				{
					return false;
				}

				Erelia.Battle.Voxel.Cell cell = Context.Instance.BattleData.Board.Cells[x, targetY, z];
				if (Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition) &&
					definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Obstacle)
				{
					return false;
				}
			}

			return true;
		}

		private bool IsInsideBoard(Vector3Int coordinate)
		{
			return coordinate.x >= 0 && coordinate.x < Context.Instance.BattleData.Board.SizeX &&
				coordinate.y >= 0 && coordinate.y < Context.Instance.BattleData.Board.SizeY &&
				coordinate.z >= 0 && coordinate.z < Context.Instance.BattleData.Board.SizeZ;
		}
	}
}
