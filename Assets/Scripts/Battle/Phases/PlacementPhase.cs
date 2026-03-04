using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Placement phase that computes placement masks and handles unit placement.
	/// Applies placement tiles around centers, updates mask meshes, and clears them on exit.
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
		/// Mask type used for player placement tiles.
		/// </summary>
		private const Erelia.Battle.Voxel.Type PlacementMask = Erelia.Battle.Voxel.Type.Placement;
		/// <summary>
		/// Mask type used for enemy placement tiles.
		/// </summary>
		private const Erelia.Battle.Voxel.Type EnemyPlacementMask = Erelia.Battle.Voxel.Type.EnemyPlacement;

		public override BattlePhaseId Id => BattlePhaseId.Placement;

		/// <summary>
		/// Enters the placement phase and applies placement masks.
		/// </summary>
		public override void Enter(BattleManager manager)
		{
			// Apply placement masks or mark pending if data is missing.
			pendingApply = !TryApplyPlacementMask(manager);
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

			if (!TryGetPlacementCenters(out Vector2Int playerCenter, out Vector2Int enemyCenter))
			{
				return false;
			}

			Erelia.Core.VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;
			if (registry == null)
			{
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] Voxel registry is not assigned.");
				return false;
			}

			ApplyPlacementMask(board, registry, playerCenter, enemyCenter);
			presenter.RebuildMasks();
			return true;
		}

		/// <summary>
		/// Computes placement candidates and assigns mask tiles per team.
		/// </summary>
		private void ApplyPlacementMask(
			Erelia.Battle.Board.Model board,
			Erelia.Core.VoxelKit.Registry registry,
			Vector2Int playerCenter,
			Vector2Int enemyCenter)
		{
			// Compute candidate cells and assign them to player/enemy.
			if (board == null || board.Cells == null || registry == null)
			{
				return;
			}

			int radius = ResolvePlacementRadius();
			int desiredCount = Mathf.Max(0, Mathf.CeilToInt(Mathf.PI * radius * radius));
			if (desiredCount <= 0)
			{
				return;
			}

			var candidates = CollectPlacementCandidates(board.Cells, registry);
			if (candidates.Count == 0)
			{
				return;
			}

			int maxPerTeam = Mathf.Min(desiredCount, candidates.Count / 2);
			if (maxPerTeam <= 0)
			{
				return;
			}

			if (maxPerTeam < desiredCount)
			{
				Debug.LogWarning($"[Erelia.Battle.PlacementPhase] Not enough placement cells for desired count {desiredCount}. Using {maxPerTeam} per team.");
			}

			var assignments = AssignPlacementCells(candidates, playerCenter, enemyCenter, maxPerTeam);
			ApplyAssignments(board.Cells, assignments.Player, PlacementMask);
			ApplyAssignments(board.Cells, assignments.Enemy, EnemyPlacementMask);
		}

		/// <summary>
		/// Applies a placement mask to a list of cell positions.
		/// </summary>
		private static void ApplyAssignments(
			Erelia.Battle.Voxel.Cell[,,] cells,
			List<Vector3Int> positions,
			Erelia.Battle.Voxel.Type maskType)
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
		/// Assigns candidate cells to player and enemy teams.
		/// </summary>
		private static PlacementAssignments AssignPlacementCells(
			List<Vector3Int> candidates,
			Vector2Int playerCenter,
			Vector2Int enemyCenter,
			int countPerTeam)
		{
			// Rank candidates by distance and split between teams.
			var ranked = new List<Candidate>(candidates.Count);
			for (int i = 0; i < candidates.Count; i++)
			{
				Vector3Int pos = candidates[i];
				float playerDist = DistanceSq(playerCenter, pos);
				float enemyDist = DistanceSq(enemyCenter, pos);
				ranked.Add(new Candidate(pos, playerDist, enemyDist));
			}

			ranked.Sort((a, b) =>
			{
				float aBest = Mathf.Min(a.PlayerDistanceSq, a.EnemyDistanceSq);
				float bBest = Mathf.Min(b.PlayerDistanceSq, b.EnemyDistanceSq);
				int bestCompare = aBest.CompareTo(bBest);
				if (bestCompare != 0)
				{
					return bestCompare;
				}

				float aDiff = Mathf.Abs(a.PlayerDistanceSq - a.EnemyDistanceSq);
				float bDiff = Mathf.Abs(b.PlayerDistanceSq - b.EnemyDistanceSq);
				return aDiff.CompareTo(bDiff);
			});

			var player = new List<Vector3Int>(countPerTeam);
			var enemy = new List<Vector3Int>(countPerTeam);
			var used = new HashSet<Vector3Int>();

			for (int i = 0; i < ranked.Count; i++)
			{
				if (player.Count >= countPerTeam && enemy.Count >= countPerTeam)
				{
					break;
				}

				Candidate candidate = ranked[i];
				if (used.Contains(candidate.Position))
				{
					continue;
				}

				bool preferPlayer = candidate.PlayerDistanceSq <= candidate.EnemyDistanceSq;
				if (preferPlayer)
				{
					if (player.Count < countPerTeam)
					{
						player.Add(candidate.Position);
						used.Add(candidate.Position);
					}
					else if (enemy.Count < countPerTeam)
					{
						enemy.Add(candidate.Position);
						used.Add(candidate.Position);
					}
				}
				else
				{
					if (enemy.Count < countPerTeam)
					{
						enemy.Add(candidate.Position);
						used.Add(candidate.Position);
					}
					else if (player.Count < countPerTeam)
					{
						player.Add(candidate.Position);
						used.Add(candidate.Position);
					}
				}
			}

			return new PlacementAssignments(player, enemy);
		}

		/// <summary>
		/// Computes squared distance between a center and a cell.
		/// </summary>
		private static float DistanceSq(Vector2Int center, Vector3Int pos)
		{
			// Use X/Z for grid distance.
			float dx = pos.x - center.x;
			float dz = pos.z - center.y;
			return (dx * dx) + (dz * dz);
		}

		/// <summary>
		/// Resolves placement radius from the encounter table.
		/// </summary>
		private static int ResolvePlacementRadius()
		{
			// Read placement radius from context data.
			Erelia.Battle.Data data = Erelia.Core.Context.Instance?.BattleData;
			Erelia.Core.Encounter.EncounterTable table = data != null ? data.EncounterTable : null;
			if (table == null)
			{
				return 0;
			}

			return Mathf.Max(0, table.PlacementRadius);
		}

		/// <summary>
		/// Tries to resolve placement centers from battle info.
		/// </summary>
		private static bool TryGetPlacementCenters(out Vector2Int playerCenter, out Vector2Int enemyCenter)
		{
			// Read centers from context battle info.
			playerCenter = default;
			enemyCenter = default;

			Erelia.Battle.Info info = Erelia.Core.Context.Instance?.BattleData?.Info;
			if (info == null)
			{
				return false;
			}

			playerCenter = info.PlayerPlacementCenter;
			enemyCenter = info.EnemyPlacementCenter;
			return true;
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
			Erelia.Battle.Voxel.Type placementMask,
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

		/// <summary>
		/// Ranked candidate used to score placement cells.
		/// Stores distances to player and enemy centers for assignment.
		/// </summary>
		private readonly struct Candidate
		{
			/// <summary>
			/// Creates a ranked placement candidate.
			/// </summary>
			public Candidate(Vector3Int position, float playerDistanceSq, float enemyDistanceSq)
			{
				// Store candidate data and distances.
				Position = position;
				PlayerDistanceSq = playerDistanceSq;
				EnemyDistanceSq = enemyDistanceSq;
			}

			/// <summary>
			/// Candidate cell position.
			/// </summary>
			public Vector3Int Position { get; }
			/// <summary>
			/// Squared distance to the player center.
			/// </summary>
			public float PlayerDistanceSq { get; }
			/// <summary>
			/// Squared distance to the enemy center.
			/// </summary>
			public float EnemyDistanceSq { get; }
		}

		/// <summary>
		/// Placement assignments split by team.
		/// Holds the chosen cell lists for player and enemy placements.
		/// </summary>
		private readonly struct PlacementAssignments
		{
			/// <summary>
			/// Creates placement assignments for both teams.
			/// </summary>
			public PlacementAssignments(List<Vector3Int> player, List<Vector3Int> enemy)
			{
				// Store assigned cell lists.
				Player = player;
				Enemy = enemy;
			}

			/// <summary>
			/// Assigned placement cells for the player team.
			/// </summary>
			public List<Vector3Int> Player { get; }
			/// <summary>
			/// Assigned placement cells for the enemy team.
			/// </summary>
			public List<Vector3Int> Enemy { get; }
		}

	}
}
