using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class PlacementPhase : BattlePhase
	{
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

		private bool pendingApply;
		private const Erelia.BattleVoxel.Type PlacementMask = Erelia.BattleVoxel.Type.Placement;
		private const Erelia.BattleVoxel.Type EnemyPlacementMask = Erelia.BattleVoxel.Type.EnemyPlacement;

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

			VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;
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
			VoxelKit.Registry registry,
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
			Erelia.BattleVoxel.Cell[,,] cells,
			List<Vector3Int> positions,
			Erelia.BattleVoxel.Type maskType)
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

				Erelia.BattleVoxel.Cell cell = cells[pos.x, pos.y, pos.z];
				if (cell == null || cell.HasMask(maskType))
				{
					continue;
				}

				cell.AddMask(maskType);
			}
		}

		private static List<Vector3Int> CollectPlacementCandidates(Erelia.BattleVoxel.Cell[,,] cells, VoxelKit.Registry registry)
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
			Erelia.Battle.Data data = Erelia.Context.Instance?.BattleData;
			Erelia.Encounter.EncounterTable table = data != null ? data.EncounterTable : null;
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

			Erelia.Battle.Info info = Erelia.Context.Instance?.BattleData?.Info;
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

			Erelia.BattleVoxel.Cell[,,] cells = board.Cells;
			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						Erelia.BattleVoxel.Cell cell = cells[x, y, z];
						if (cell == null || !cell.HasAnyMask())
						{
							continue;
						}

						cell.ClearMasks();
					}
				}
			}
		}

		private static bool TryGetPlacementSurface(
			Erelia.BattleVoxel.Cell[,,] cells,
			VoxelKit.Registry registry,
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
				Erelia.BattleVoxel.Cell cell = cells[x, yi, z];
				if (!IsObstacleCell(cell, registry))
				{
					continue;
				}

				if (yi + 1 < sizeY)
				{
					Erelia.BattleVoxel.Cell above = cells[x, yi + 1, z];
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

		private static bool IsObstacleCell(Erelia.BattleVoxel.Cell cell, VoxelKit.Registry registry)
		{
			if (cell == null || cell.Id < 0)
			{
				return false;
			}

			if (registry == null)
			{
				return true;
			}

			if (!registry.TryGet(cell.Id, out VoxelKit.Definition definition) || definition == null)
			{
				return true;
			}

			return definition.Data != null && definition.Data.Traversal == VoxelKit.Traversal.Obstacle;
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
