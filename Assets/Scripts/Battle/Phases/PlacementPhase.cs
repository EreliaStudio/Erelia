using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Placement phase that computes placement masks and handles unit placement.
	/// Splits the board along Z for player/enemy placement, updates mask meshes, and clears them on exit.
	/// </summary>
	[System.Serializable]
	public sealed class PlacementPhase : BattlePhase
	{
		/// <summary>
		/// Presenter used to access the battle board.
		/// </summary>
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

		/// <summary>
		/// List of units placed during this phase.
		/// </summary>
		private readonly List<Erelia.Battle.Unit> placedUnits = new List<Erelia.Battle.Unit>();
		/// <summary>
		/// Lookup of placed units by cell position.
		/// </summary>
		private readonly Dictionary<Vector3Int, Erelia.Battle.Unit> unitsByCell =
			new Dictionary<Vector3Int, Erelia.Battle.Unit>();
		/// <summary>
		/// Lookup of placed units by creature instance.
		/// </summary>
		private readonly Dictionary<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit> unitsByCreature =
			new Dictionary<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit>();
		/// <summary>
		/// Whether mask application is still pending.
		/// </summary>
		private bool pendingApply;
		/// <summary>
		/// Cached player team used for placement.
		/// </summary>
		private Erelia.Core.Creature.Team playerTeam;
		/// <summary>
		/// Next team slot index to consider for placement.
		/// </summary>
		private int nextTeamIndex;
		/// <summary>
		/// Mask type used for player placement tiles.
		/// </summary>
		private const Erelia.Battle.Voxel.Mask.Type PlacementMask = Erelia.Battle.Voxel.Mask.Type.Placement;
		/// <summary>
		/// Mask type used for enemy placement tiles.
		/// </summary>
		private const Erelia.Battle.Voxel.Mask.Type EnemyPlacementMask = Erelia.Battle.Voxel.Mask.Type.EnemyPlacement;
		public override BattlePhaseId Id => BattlePhaseId.Placement;

		/// <summary>
		/// Enters the placement phase and applies placement masks.
		/// </summary>
		public override void Enter(BattleManager manager)
		{
			// Apply placement masks or mark pending if data is missing.
			pendingApply = !TryApplyPlacementMask(manager);
			playerTeam = null;
			nextTeamIndex = 0;
		}

		/// <summary>
		/// Exits the placement phase and clears placement masks.
		/// </summary>
		public override void Exit(BattleManager manager)
		{
			// Clear placement masks when leaving the phase.
			pendingApply = false;

			Erelia.Battle.Board.Presenter presenter = ResolvePresenter(manager);
			Erelia.Battle.Board.Model board = presenter != null ? presenter.Model : null;
			if (board == null || board.Cells == null)
			{
				return;
			}

			ClearPlacementMask(board);
			presenter.RebuildMasks();
		}

		/// <summary>
		/// Ticks the placement phase until masks are applied.
		/// </summary>
		public override void Tick(BattleManager manager, float deltaTime)
		{
			// Retry mask application while pending.
			if (!pendingApply)
			{
				return;
			}

			pendingApply = !TryApplyPlacementMask(manager);
		}

		/// <summary>
		/// Handles confirm input during placement.
		/// </summary>
		public override void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
			// Attempt to place the next creature on the hovered cell.
			if (controller == null || !controller.HasHoveredCell())
			{
				return;
			}

			if (!TryGetNextCreature(out Erelia.Core.Creature.Instance.Model creature))
			{
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] No available creature to place.");
				return;
			}

			Vector3Int cell = controller.HoveredCell();
			if (!TryPlaceCreature(creature, cell, Erelia.Battle.Voxel.Mask.Type.Placement, out Erelia.Battle.Unit _))
			{
				return;
			}

			string creatureLabel = !string.IsNullOrEmpty(creature.Nickname)
				? creature.Nickname
				: ResolveSpeciesName(creature);
			Debug.Log($"[Erelia.Battle.PlacementPhase] Placed '{creatureLabel}' at cell {cell.x}/{cell.y}/{cell.z}.");
		}

		/// <summary>
		/// Handles cancel input during placement.
		/// </summary>
		public override void OnCancel(Erelia.Battle.Player.BattlePlayerController controller)
		{
			// Clear selection when the player cancels.
			if (controller == null)
			{
				return;
			}

			if (controller.HasHoveredCell())
			{
				Vector3Int cell = controller.HoveredCell();
				Debug.Log($"[Erelia.Battle.PlacementPhase] Player cancelled action at cell {cell.x}/{cell.y}/{cell.z}.");
			}

			controller.ClearSelection();
		}

		/// <summary>
		/// Resolves the battle board presenter from field or manager.
		/// </summary>
		private Erelia.Battle.Board.Presenter ResolvePresenter(BattleManager manager)
		{
			// Prefer the serialized presenter, fallback to manager lookup.
			if (boardPresenter != null)
			{
				return boardPresenter;
			}

			if (manager != null)
			{
				boardPresenter = manager.GetComponentInChildren<Erelia.Battle.Board.Presenter>(true);
			}

			if (boardPresenter == null)
			{
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] Battle board presenter is not assigned.");
			}

			return boardPresenter;
		}

		/// <summary>
		/// Builds and applies placement masks to the board.
		/// </summary>
		private bool TryApplyPlacementMask(BattleManager manager)
		{
			// Resolve board and placement centers, then apply masks.
			Erelia.Battle.Board.Presenter presenter = ResolvePresenter(manager);
			Erelia.Battle.Board.Model board = presenter != null ? presenter.Model : null;
			if (board == null || board.Cells == null)
			{
				return false;
			}

			Erelia.Core.VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;
			if (registry == null)
			{
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] Voxel registry is not assigned.");
				return false;
			}

			ApplyPlacementMask(board, registry);
			presenter.RebuildMasks();
			return true;
		}

		/// <summary>
		/// Computes placement candidates and assigns mask tiles per team.
		/// </summary>
		private void ApplyPlacementMask(
			Erelia.Battle.Board.Model board,
			Erelia.Core.VoxelKit.Registry registry)
		{
			// Compute candidate cells and assign them to the player half of the board.
			if (board == null || board.Cells == null || registry == null)
			{
				return;
			}

			var candidates = CollectPlacementCandidates(board.Cells, registry);
			if (candidates.Count == 0)
			{
				return;
			}

			int sizeZ = board.Cells.GetLength(2);
			int splitZ = sizeZ / 2;

			var playerPositions = new List<Vector3Int>();
			var enemyPositions = new List<Vector3Int>();
			for (int i = 0; i < candidates.Count; i++)
			{
				Vector3Int pos = candidates[i];
				if (pos.z < splitZ)
				{
					playerPositions.Add(pos);
				}
				else
				{
					enemyPositions.Add(pos);
				}
			}

			ApplyAssignments(board.Cells, playerPositions, PlacementMask);
			ApplyAssignments(board.Cells, enemyPositions, EnemyPlacementMask);
		}

		/// <summary>
		/// Applies a placement mask to a list of cell positions.
		/// </summary>
		private static void ApplyAssignments(
			Erelia.Battle.Voxel.Cell[,,] cells,
			List<Vector3Int> positions,
			Erelia.Battle.Voxel.Mask.Type maskType)
		{
			// Add the mask to each valid cell position.
			if (cells == null || positions == null || positions.Count == 0)
			{
				return;
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int i = 0; i < positions.Count; i++)
			{
				Vector3Int pos = positions[i];
				if (pos.x < 0 || pos.x >= sizeX || pos.y < 0 || pos.y >= sizeY || pos.z < 0 || pos.z >= sizeZ)
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = cells[pos.x, pos.y, pos.z];
				if (cell == null || cell.HasMask(maskType))
				{
					continue;
				}

				cell.AddMask(maskType);
			}
		}

		/// <summary>
		/// Collects candidate placement positions from the board.
		/// </summary>
		private static List<Vector3Int> CollectPlacementCandidates(Erelia.Battle.Voxel.Cell[,,] cells, Erelia.Core.VoxelKit.Registry registry)
		{
			// Scan the grid for valid placement surfaces.
			var candidates = new List<Vector3Int>();
			if (cells == null || registry == null)
			{
				return candidates;
			}

			int sizeX = cells.GetLength(0);
			int sizeZ = cells.GetLength(2);
			for (int x = 0; x < sizeX; x++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					if (TryGetPlacementSurface(cells, registry, x, z, out int y))
					{
						candidates.Add(new Vector3Int(x, y, z));
					}
				}
			}

			return candidates;
		}

		/// <summary>
		/// Clears all placement masks from the board.
		/// </summary>
		private void ClearPlacementMask(Erelia.Battle.Board.Model board)
		{
			// Remove masks from every cell.
			if (board == null || board.Cells == null)
			{
				return;
			}

			Erelia.Battle.Voxel.Cell[,,] cells = board.Cells;
			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						Erelia.Battle.Voxel.Cell cell = cells[x, y, z];
						if (cell == null || !cell.HasAnyMask())
						{
							continue;
						}

						cell.ClearMasks();
					}
				}
			}
		}

		/// <summary>
		/// Gets the list of units placed during this phase.
		/// </summary>
		public IReadOnlyList<Erelia.Battle.Unit> PlacedUnits => placedUnits;

		/// <summary>
		/// Checks whether a creature is already placed.
		/// </summary>
		public bool IsCreaturePlaced(Erelia.Core.Creature.Instance.Model creature)
		{
			// Look up creature placement.
			if (creature == null)
			{
				return false;
			}

			return unitsByCreature.ContainsKey(creature);
		}

		/// <summary>
		/// Attempts to place a creature on the board at the given cell.
		/// </summary>
		public bool TryPlaceCreature(
			Erelia.Core.Creature.Instance.Model creature,
			Vector3Int cell,
			Erelia.Battle.Voxel.Mask.Type placementMask,
			out Erelia.Battle.Unit unit)
		{
			// Validate placement request and spawn the creature view.
			unit = null;
			if (creature == null)
			{
				return false;
			}

			if (IsCreaturePlaced(creature))
			{
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] Creature is already placed.");
				return false;
			}

			Erelia.Battle.Board.Presenter presenter = ResolvePresenter(null);
			Erelia.Battle.Board.Model board = presenter != null ? presenter.Model : null;
			Erelia.Battle.Voxel.Cell[,,] cells = board != null ? board.Cells : null;
			if (cells == null)
			{
				return false;
			}

			if (unitsByCell.ContainsKey(cell))
			{
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] Cell is already occupied.");
				return false;
			}

			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);
			if (cell.x < 0 || cell.x >= sizeX || cell.y < 0 || cell.y >= sizeY || cell.z < 0 || cell.z >= sizeZ)
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cellData = cells[cell.x, cell.y, cell.z];
			if (cellData == null || cellData.Id < 0)
			{
				return false;
			}

			if (!cellData.HasMask(placementMask))
			{
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] Cell is not a valid placement tile.");
				return false;
			}

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			if (registry == null)
			{
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] SpeciesRegistry is missing.");
				return false;
			}

			if (!registry.TryGet(creature.SpeciesId, out Erelia.Core.Creature.Species species) || species == null || species.Prefab == null)
			{
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] Creature species or prefab is missing.");
				return false;
			}

			if (!TryGetPlacementPosition(cell, cellData, out Vector3 position))
			{
				return false;
			}

			GameObject view = Object.Instantiate(
				species.Prefab,
				position,
				Quaternion.identity,
				presenter != null ? presenter.transform : null);

			unit = new Erelia.Battle.Unit(creature, cell, view);
			placedUnits.Add(unit);
			unitsByCell[cell] = unit;
			unitsByCreature[creature] = unit;

			cellData.RemoveMask(placementMask);
			presenter?.RebuildMasks();
			return true;
		}

		/// <summary>
		/// Picks the next creature to place for the player team.
		/// </summary>
		private bool TryGetNextCreature(out Erelia.Core.Creature.Instance.Model creature)
		{
			// Iterate team slots and find an unplaced creature.
			creature = null;
			Erelia.Core.Creature.Team team = ResolvePlayerTeam();
			if (team == null || team.Slots == null || team.Slots.Length == 0)
			{
				return false;
			}

			int total = team.Slots.Length;
			for (int i = 0; i < total; i++)
			{
				int index = (nextTeamIndex + i) % total;
				Erelia.Core.Creature.Instance.Model candidate = team.Slots[index];
				if (candidate == null)
				{
					continue;
				}

				if (IsCreaturePlaced(candidate))
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
		/// Assigns the player team used for placement.
		/// </summary>
		public void SetPlayerTeam(Erelia.Core.Creature.Team team)
		{
			// Store team and reset index.
			playerTeam = team;
			nextTeamIndex = 0;
		}

		/// <summary>
		/// Resolves the player team from context if not explicitly set.
		/// </summary>
		private Erelia.Core.Creature.Team ResolvePlayerTeam()
		{
			// Use the provided team or fall back to system data.
			if (playerTeam != null)
			{
				return playerTeam;
			}

			Erelia.Core.SystemData systemData = Erelia.Core.Context.Instance?.SystemData;
			playerTeam = systemData != null ? systemData.PlayerTeam : null;
			return playerTeam;
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
		/// Computes a world position for a creature placed on a cell.
		/// </summary>
		private bool TryGetPlacementPosition(Vector3Int cell, Erelia.Battle.Voxel.Cell cellData, out Vector3 position)
		{
			// Resolve the mask shape and compute an offset position.
			position = default;
			if (cellData == null)
			{
				return false;
			}

			Erelia.Core.VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;
			if (registry == null)
			{
				return false;
			}

			if (!registry.TryGet(cellData.Id, out Erelia.Core.VoxelKit.Definition definition) || definition == null)
			{
				return false;
			}

			if (!(definition is Erelia.Battle.Voxel.Definition battleDefinition) || battleDefinition.MaskShape == null)
			{
				return false;
			}

			Vector3 localOffset = battleDefinition.MaskShape.GetCardinalPoint(
				Erelia.Battle.Voxel.CardinalPoint.Stationary,
				cellData.Orientation,
				cellData.FlipOrientation);
			Vector3 localPosition = new Vector3(cell.x, cell.y, cell.z) + localOffset;

			if (boardPresenter != null)
			{
				position = boardPresenter.transform.TransformPoint(localPosition);
			}
			else
			{
				position = localPosition;
			}

			return true;
		}

		/// <summary>
		/// Finds a valid placement surface cell at X/Z.
		/// </summary>
		private static bool TryGetPlacementSurface(
			Erelia.Battle.Voxel.Cell[,,] cells,
			Erelia.Core.VoxelKit.Registry registry,
			int x,
			int z,
			out int y)
		{
			// Scan downward for the first obstacle cell with empty space above.
			y = -1;
			if (cells == null)
			{
				return false;
			}

			int sizeY = cells.GetLength(1);
			for (int yi = sizeY - 1; yi >= 0; yi--)
			{
				Erelia.Battle.Voxel.Cell cell = cells[x, yi, z];
				if (!IsObstacleCell(cell, registry))
				{
					continue;
				}

				if (yi + 1 < sizeY)
				{
					Erelia.Battle.Voxel.Cell above = cells[x, yi + 1, z];
					if (IsObstacleCell(above, registry))
					{
						continue;
					}
				}

				y = yi;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines whether a cell is an obstacle for placement.
		/// </summary>
		private static bool IsObstacleCell(Erelia.Battle.Voxel.Cell cell, Erelia.Core.VoxelKit.Registry registry)
		{
			// Treat missing registry/definition as obstacle by default.
			if (cell == null || cell.Id < 0)
			{
				return false;
			}

			if (registry == null)
			{
				return true;
			}

			if (!registry.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition) || definition == null)
			{
				return true;
			}

			return definition.Data != null && definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Obstacle;
		}

	}
}
