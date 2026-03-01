using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player
{
	public sealed class BattlePlayerController : MonoBehaviour
	{
		[SerializeField] private Erelia.Battle.Player.Camera.MouseBoardCellCursor cursor;
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		[SerializeField] private Erelia.Battle.BattleManager battleManager;
		private Erelia.Core.Creature.Team team;
		[SerializeField] private InputActionReference confirmAction;
		[SerializeField] private InputActionReference cancelAction;

		private InputAction resolvedConfirmAction;
		private InputAction resolvedCancelAction;
		private bool hasHoveredCell;
		private Vector3Int hoveredCell;
		private int nextTeamIndex;

		private void Awake()
		{
			if (cursor == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.BattlePlayerController] Mouse board cell cursor is not assigned.");
			}

			if (boardPresenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.BattlePlayerController] Board presenter is not assigned.");
			}

			if (battleManager == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.BattlePlayerController] Battle manager is not assigned.");
			}

			ResolveActions();
			ResolveTeam();
		}

		private void OnEnable()
		{
			if (cursor != null)
			{
				cursor.CellChanged += OnCellChanged;
				cursor.HoverCleared += OnHoverCleared;
			}

			resolvedConfirmAction?.Enable();
			resolvedCancelAction?.Enable();
			ResolveTeam();
		}

		private void OnDisable()
		{
			if (cursor != null)
			{
				cursor.CellChanged -= OnCellChanged;
				cursor.HoverCleared -= OnHoverCleared;
			}

			resolvedConfirmAction?.Disable();
			resolvedCancelAction?.Disable();
			ClearSelection();
		}

		private void Update()
		{
			if (IsConfirmTriggered())
			{
				HandleConfirm();
			}

			if (IsCancelTriggered())
			{
				HandleCancel();
			}
		}

		public bool HasHoveredCell()
		{
			return hasHoveredCell;
		}

		public Vector3Int HoveredCell()
		{
			return hoveredCell;
		}

		private void ResolveActions()
		{
			if (confirmAction.action == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.BattlePlayerController] Confirm action is not assigned.");
			}

			resolvedConfirmAction = confirmAction.action;

			if (cancelAction.action == null)
			{
				throw new System.Exception("[Erelia.Battle.Player.BattlePlayerController] Cancel action is not assigned.");
			}

			resolvedCancelAction = cancelAction.action;
		}

		private void HandleConfirm()
		{
			if (!hasHoveredCell)
			{
				return;
			}

			Erelia.Battle.PlacementPhase placementPhase = ResolvePlacementPhase();
			if (placementPhase == null)
			{
				Debug.LogWarning("[Erelia.Battle.Player.BattlePlayerController] Placement phase not active.");
				return;
			}

			if (!TryGetNextCreature(placementPhase, out Erelia.Core.Creature.Instance.Model creature))
			{
				Debug.LogWarning("[Erelia.Battle.Player.BattlePlayerController] No available creature to place.");
				return;
			}

			if (!placementPhase.TryPlaceCreature(creature, hoveredCell, Erelia.Battle.Voxel.Type.Placement, out Erelia.Battle.Unit _))
			{
				return;
			}

			string creatureLabel = !string.IsNullOrEmpty(creature.Nickname)
				? creature.Nickname
				: ResolveSpeciesName(creature);
			Debug.Log($"[Erelia.Battle.Player.BattlePlayerController] Placed '{creatureLabel}' at cell {hoveredCell.x}/{hoveredCell.y}/{hoveredCell.z}.");
		}

		private void HandleCancel()
		{
			if (hasHoveredCell)
			{
				Debug.Log($"[Erelia.Battle.Player.BattlePlayerController] Player cancelled action at cell {hoveredCell.x}/{hoveredCell.y}/{hoveredCell.z}.");
			}

			ClearSelection();
		}

		private void OnCellChanged(Vector3Int cell)
		{
			if (!TryResolveBoardCell(cell, out Erelia.Battle.Voxel.Cell targetCell))
			{
				ClearSelection();
				return;
			}

			if (hasHoveredCell && cell == hoveredCell)
			{
				return;
			}

			RemoveSelectionMask();
			targetCell.AddMask(Erelia.Battle.Voxel.Type.Selected);
			hoveredCell = cell;
			hasHoveredCell = true;
			RebuildMasks();
		}

		private void OnHoverCleared()
		{
			ClearSelection();
		}

		private void ClearSelection()
		{
			if (!hasHoveredCell)
			{
				return;
			}

			RemoveSelectionMask();
			hasHoveredCell = false;
			RebuildMasks();
		}

		private void RemoveSelectionMask()
		{
			if (!hasHoveredCell)
			{
				return;
			}

			if (!TryResolveBoardCell(hoveredCell, out Erelia.Battle.Voxel.Cell cell))
			{
				return;
			}

			cell.RemoveMask(Erelia.Battle.Voxel.Type.Selected);
		}

		private bool TryResolveBoardCell(Vector3Int cell, out Erelia.Battle.Voxel.Cell resolvedCell)
		{
			resolvedCell = null;

			Erelia.Battle.Board.Model board = boardPresenter != null ? boardPresenter.Model : null;
			Erelia.Battle.Voxel.Cell[,,] cells = board != null ? board.Cells : null;
			if (cells == null)
			{
				return false;
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			if (cell.x < 0 || cell.x >= sizeX ||
				cell.y < 0 || cell.y >= sizeY ||
				cell.z < 0 || cell.z >= sizeZ)
			{
				return false;
			}

			resolvedCell = cells[cell.x, cell.y, cell.z];
			if (resolvedCell == null || resolvedCell.Id < 0)
			{
				resolvedCell = null;
				return false;
			}

			return true;
		}

		private void RebuildMasks()
		{
			if (boardPresenter == null)
			{
				return;
			}

			boardPresenter.RebuildMasks();
		}

		private Erelia.Battle.PlacementPhase ResolvePlacementPhase()
		{
			if (battleManager == null)
			{
				return null;
			}

			return battleManager.CurrentPhase as Erelia.Battle.PlacementPhase;
		}

		private bool TryGetNextCreature(Erelia.Battle.PlacementPhase placementPhase, out Erelia.Core.Creature.Instance.Model creature)
		{
			creature = null;
			Erelia.Core.Creature.Team resolvedTeam = ResolveTeam();
			if (resolvedTeam == null || resolvedTeam.Slots == null || resolvedTeam.Slots.Length == 0)
			{
				return false;
			}

			int total = resolvedTeam.Slots.Length;
			for (int i = 0; i < total; i++)
			{
				int index = (nextTeamIndex + i) % total;
				Erelia.Core.Creature.Instance.Model candidate = resolvedTeam.Slots[index];
				if (candidate == null)
				{
					continue;
				}

				if (placementPhase != null && placementPhase.IsCreaturePlaced(candidate))
				{
					continue;
				}

				creature = candidate;
				nextTeamIndex = (index + 1) % total;
				return true;
			}

			return false;
		}

		public void SetTeam(Erelia.Core.Creature.Team newTeam)
		{
			team = newTeam;
			nextTeamIndex = 0;
		}

		private Erelia.Core.Creature.Team ResolveTeam()
		{
			if (team != null)
			{
				return team;
			}

			Erelia.Core.SystemData systemData = Erelia.Core.Context.Instance?.SystemData;
			team = systemData != null ? systemData.PlayerTeam : null;
			return team;
		}

		private static string ResolveSpeciesName(Erelia.Core.Creature.Instance.Model creature)
		{
			if (creature == null)
			{
				return "Creature";
			}

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			if (registry != null && registry.TryGet(creature.SpeciesId, out Erelia.Core.Creature.Species species) && species != null)
			{
				return string.IsNullOrEmpty(species.DisplayName) ? "Creature" : species.DisplayName;
			}

			return "Creature";
		}

		private bool IsConfirmTriggered()
		{
			if (resolvedConfirmAction != null)
			{
				return resolvedConfirmAction.WasPerformedThisFrame();
			}

			Mouse mouse = Mouse.current;
			return mouse != null && mouse.leftButton.wasPressedThisFrame;
		}

		private bool IsCancelTriggered()
		{
			if (resolvedCancelAction != null)
			{
				return resolvedCancelAction.WasPerformedThisFrame();
			}

			Mouse mouse = Mouse.current;
			return mouse != null && mouse.rightButton.wasPressedThisFrame;
		}
	}
}
