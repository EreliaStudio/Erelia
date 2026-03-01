using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class PlacementPhase : BattlePhase
	{
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

		private readonly List<Erelia.Battle.Unit> placedUnits = new List<Erelia.Battle.Unit>();
		private readonly Dictionary<Vector3Int, Erelia.Battle.Unit> unitsByCell =
			new Dictionary<Vector3Int, Erelia.Battle.Unit>();
		private readonly Dictionary<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit> unitsByCreature =
			new Dictionary<Erelia.Core.Creature.Instance.Model, Erelia.Battle.Unit>();
		private bool pendingApply;
		private const Erelia.Battle.Voxel.Type PlacementMask = Erelia.Battle.Voxel.Type.Placement;
		private const Erelia.Battle.Voxel.Type EnemyPlacementMask = Erelia.Battle.Voxel.Type.EnemyPlacement;

		public override BattlePhaseId Id => BattlePhaseId.Placement;

		public override void Enter(BattleManager manager)
		{
			pendingApply = !TryApplyPlacementMask(manager);
		}

		public override void Exit(BattleManager manager)
		{
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

		public override void Tick(BattleManager manager, float deltaTime)
		{
			if (!pendingApply)
			{
				return;
			}

			pendingApply = !TryApplyPlacementMask(manager);
		}

		private Erelia.Battle.Board.Presenter ResolvePresenter(BattleManager manager)
		{
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

		private bool TryApplyPlacementMask(BattleManager manager)
		{
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

		private void ApplyPlacementMask(
			Erelia.Battle.Board.Model board,
			Erelia.Core.VoxelKit.Registry registry,
			Vector2Int playerCenter,
			Vector2Int enemyCenter)
		{
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

		private static void ApplyAssignments(
			Erelia.Battle.Voxel.Cell[,,] cells,
			List<Vector3Int> positions,
			Erelia.Battle.Voxel.Type maskType)
		{
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

		private static List<Vector3Int> CollectPlacementCandidates(Erelia.Battle.Voxel.Cell[,,] cells, Erelia.Core.VoxelKit.Registry registry)
		{
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

		private static PlacementAssignments AssignPlacementCells(
			List<Vector3Int> candidates,
			Vector2Int playerCenter,
			Vector2Int enemyCenter,
			int countPerTeam)
		{
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

		private static float DistanceSq(Vector2Int center, Vector3Int pos)
		{
			float dx = pos.x - center.x;
			float dz = pos.z - center.y;
			return (dx * dx) + (dz * dz);
		}

		private static int ResolvePlacementRadius()
		{
			Erelia.Battle.Data data = Erelia.Core.Context.Instance?.BattleData;
			Erelia.Core.Encounter.EncounterTable table = data != null ? data.EncounterTable : null;
			if (table == null)
			{
				return 0;
			}

			return Mathf.Max(0, table.PlacementRadius);
		}

		private static bool TryGetPlacementCenters(out Vector2Int playerCenter, out Vector2Int enemyCenter)
		{
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

		private void ClearPlacementMask(Erelia.Battle.Board.Model board)
		{
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

		public IReadOnlyList<Erelia.Battle.Unit> PlacedUnits => placedUnits;

		public bool IsCreaturePlaced(Erelia.Core.Creature.Instance.Model creature)
		{
			if (creature == null)
			{
				return false;
			}

			return unitsByCreature.ContainsKey(creature);
		}

		public bool TryPlaceCreature(
			Erelia.Core.Creature.Instance.Model creature,
			Vector3Int cell,
			Erelia.Battle.Voxel.Type placementMask,
			out Erelia.Battle.Unit unit)
		{
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

		private bool TryGetPlacementPosition(Vector3Int cell, Erelia.Battle.Voxel.Cell cellData, out Vector3 position)
		{
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
				Erelia.Core.VoxelKit.CardinalPoint.Stationary,
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

		private static bool TryGetPlacementSurface(
			Erelia.Battle.Voxel.Cell[,,] cells,
			Erelia.Core.VoxelKit.Registry registry,
			int x,
			int z,
			out int y)
		{
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

		private static bool IsObstacleCell(Erelia.Battle.Voxel.Cell cell, Erelia.Core.VoxelKit.Registry registry)
		{
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

		private readonly struct Candidate
		{
			public Candidate(Vector3Int position, float playerDistanceSq, float enemyDistanceSq)
			{
				Position = position;
				PlayerDistanceSq = playerDistanceSq;
				EnemyDistanceSq = enemyDistanceSq;
			}

			public Vector3Int Position { get; }
			public float PlayerDistanceSq { get; }
			public float EnemyDistanceSq { get; }
		}

		private readonly struct PlacementAssignments
		{
			public PlacementAssignments(List<Vector3Int> player, List<Vector3Int> enemy)
			{
				Player = player;
				Enemy = enemy;
			}

			public List<Vector3Int> Player { get; }
			public List<Vector3Int> Enemy { get; }
		}

	}
}
