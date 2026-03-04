using UnityEngine;
using UnityEngine.InputSystem;

namespace Erelia.Battle.Player
{
	/// <summary>
	/// Handles player input for hover/confirm/cancel during battle placement.
	/// Tracks the hovered cell, applies selection masks, and requests creature placement.
	/// </summary>
	public sealed class BattlePlayerController : MonoBehaviour
	{
		/// <summary>
		/// Cursor used to detect hovered board cells.
		/// </summary>
		[SerializeField] private Erelia.Battle.Player.Camera.MouseBoardCellCursor cursor;
		/// <summary>
		/// Presenter that owns the battle board model.
		/// </summary>
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;
		/// <summary>
		/// Battle manager used to query the current phase.
		/// </summary>
		[SerializeField] private Erelia.Battle.BattleManager battleManager;
		/// <summary>
		/// Team used for placement actions.
		/// </summary>
		private Erelia.Core.Creature.Team team;
		/// <summary>
		/// Input action used to confirm placement.
		/// </summary>
		[SerializeField] private InputActionReference confirmAction;
		/// <summary>
		/// Input action used to cancel placement.
		/// </summary>
		[SerializeField] private InputActionReference cancelAction;

		/// <summary>
		/// Resolved input action for confirm.
		/// </summary>
		private InputAction resolvedConfirmAction;
		/// <summary>
		/// Resolved input action for cancel.
		/// </summary>
		private InputAction resolvedCancelAction;
		/// <summary>
		/// Whether the cursor currently hovers a valid cell.
		/// </summary>
		private bool hasHoveredCell;
		/// <summary>
		/// Currently hovered cell coordinate.
		/// </summary>
		private Vector3Int hoveredCell;
		/// <summary>
		/// Next team slot index to consider for placement.
		/// </summary>
		private int nextTeamIndex;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Validate references and resolve input/team data.
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

		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Subscribe to cursor events and enable input.
			if (cursor != null)
			{
				cursor.CellChanged += OnCellChanged;
				cursor.HoverCleared += OnHoverCleared;
			}

			resolvedConfirmAction?.Enable();
			resolvedCancelAction?.Enable();
			ResolveTeam();
		}

		/// <summary>
		/// Unity callback invoked when the component is disabled.
		/// </summary>
		private void OnDisable()
		{
			// Unsubscribe from events and reset selection state.
			if (cursor != null)
			{
				cursor.CellChanged -= OnCellChanged;
				cursor.HoverCleared -= OnHoverCleared;
			}

			resolvedConfirmAction?.Disable();
			resolvedCancelAction?.Disable();
			ClearSelection();
		}

		/// <summary>
		/// Unity update loop.
		/// </summary>
		private void Update()
		{
			// Poll confirm/cancel input and act accordingly.
			if (IsConfirmTriggered())
			{
				HandleConfirm();
			}

			if (IsCancelTriggered())
			{
				HandleCancel();
			}
		}

		/// <summary>
		/// Returns whether a cell is currently hovered.
		/// </summary>
		public bool HasHoveredCell()
		{
			// Expose hover state to callers.
			return hasHoveredCell;
		}

		/// <summary>
		/// Returns the currently hovered cell.
		/// </summary>
		public Vector3Int HoveredCell()
		{
			// Expose the last hovered cell.
			return hoveredCell;
		}

		/// <summary>
		/// Resolves input actions from references.
		/// </summary>
		private void ResolveActions()
		{
			// Resolve confirm and cancel actions.
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

		/// <summary>
		/// Handles the confirm action by placing a creature.
		/// </summary>
		private void HandleConfirm()
		{
			// Try to place the next creature on the hovered cell.
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

			if (!placementPhase.TryPlaceCreature(creature, hoveredCell, Erelia.Battle.Voxel.Mask.Type.Placement, out Erelia.Battle.Unit _))
			{
				return;
			}

			string creatureLabel = !string.IsNullOrEmpty(creature.Nickname)
				? creature.Nickname
				: ResolveSpeciesName(creature);
			Debug.Log($"[Erelia.Battle.Player.BattlePlayerController] Placed '{creatureLabel}' at cell {hoveredCell.x}/{hoveredCell.y}/{hoveredCell.z}.");
		}

		/// <summary>
		/// Handles the cancel action by clearing selection.
		/// </summary>
		private void HandleCancel()
		{
			// Clear selection when the player cancels.
			if (hasHoveredCell)
			{
				Debug.Log($"[Erelia.Battle.Player.BattlePlayerController] Player cancelled action at cell {hoveredCell.x}/{hoveredCell.y}/{hoveredCell.z}.");
			}

			ClearSelection();
		}

		/// <summary>
		/// Handles hover changes from the cursor.
		/// </summary>
		private void OnCellChanged(Vector3Int cell)
		{
			// Update selection mask for the hovered cell.
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
			targetCell.AddMask(Erelia.Battle.Voxel.Mask.Type.Selected);
			hoveredCell = cell;
			hasHoveredCell = true;
			RebuildMasks();
		}

		/// <summary>
		/// Handles hover cleared events.
		/// </summary>
		private void OnHoverCleared()
		{
			// Clear selection when the cursor leaves the board.
			ClearSelection();
		}

		/// <summary>
		/// Clears the current selection mask.
		/// </summary>
		private void ClearSelection()
		{
			// Remove the selection mask and update state.
			if (!hasHoveredCell)
			{
				return;
			}

			RemoveSelectionMask();
			hasHoveredCell = false;
			RebuildMasks();
		}

		/// <summary>
		/// Removes the selection mask from the hovered cell.
		/// </summary>
		private void RemoveSelectionMask()
		{
			// Remove the mask from the previous hovered cell.
			if (!hasHoveredCell)
			{
				return;
			}

			if (!TryResolveBoardCell(hoveredCell, out Erelia.Battle.Voxel.Cell cell))
			{
				return;
			}

			cell.RemoveMask(Erelia.Battle.Voxel.Mask.Type.Selected);
		}

		/// <summary>
		/// Tries to resolve a valid board cell at the given position.
		/// </summary>
		private bool TryResolveBoardCell(Vector3Int cell, out Erelia.Battle.Voxel.Cell resolvedCell)
		{
			// Validate board bounds and return a non-empty cell.
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

		/// <summary>
		/// Rebuilds mask meshes on the board presenter.
		/// </summary>
		private void RebuildMasks()
		{
			// Trigger mask mesh rebuild.
			if (boardPresenter == null)
			{
				return;
			}

			boardPresenter.RebuildMasks();
		}

		/// <summary>
		/// Resolves the active placement phase from the battle manager.
		/// </summary>
		private Erelia.Battle.PlacementPhase ResolvePlacementPhase()
		{
			// Return the current phase if it is placement.
			if (battleManager == null)
			{
				return null;
			}

			return battleManager.CurrentPhase as Erelia.Battle.PlacementPhase;
		}

		/// <summary>
		/// Picks the next creature to place for the current team.
		/// </summary>
		private bool TryGetNextCreature(Erelia.Battle.PlacementPhase placementPhase, out Erelia.Core.Creature.Instance.Model creature)
		{
			// Iterate team slots and find an unplaced creature.
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

		/// <summary>
		/// Assigns the active team for placement.
		/// </summary>
		public void SetTeam(Erelia.Core.Creature.Team newTeam)
		{
			// Store team and reset index.
			team = newTeam;
			nextTeamIndex = 0;
		}

		/// <summary>
		/// Resolves the team from context if not explicitly set.
		/// </summary>
		private Erelia.Core.Creature.Team ResolveTeam()
		{
			// Use the provided team or fall back to system data.
			if (team != null)
			{
				return team;
			}

			Erelia.Core.SystemData systemData = Erelia.Core.Context.Instance?.SystemData;
			team = systemData != null ? systemData.PlayerTeam : null;
			return team;
		}

		/// <summary>
		/// Resolves a display name for a creature.
		/// </summary>
		private static string ResolveSpeciesName(Erelia.Core.Creature.Instance.Model creature)
		{
			// Use nickname, then species display name, then fallback.
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

		/// <summary>
		/// Checks whether confirm input was triggered this frame.
		/// </summary>
		private bool IsConfirmTriggered()
		{
			// Prefer input action; fallback to mouse click.
			if (resolvedConfirmAction != null)
			{
				return resolvedConfirmAction.WasPerformedThisFrame();
			}

			Mouse mouse = Mouse.current;
			return mouse != null && mouse.leftButton.wasPressedThisFrame;
		}

		/// <summary>
		/// Checks whether cancel input was triggered this frame.
		/// </summary>
		private bool IsCancelTriggered()
		{
			// Prefer input action; fallback to mouse click.
			if (resolvedCancelAction != null)
			{
				return resolvedCancelAction.WasPerformedThisFrame();
			}

			Mouse mouse = Mouse.current;
			return mouse != null && mouse.rightButton.wasPressedThisFrame;
		}
	}
}
