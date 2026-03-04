using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Initialization phase that prepares battle data and placement centers.
	/// Tries to resolve the board and info, then transitions to the Placement phase.
	/// </summary>
	[System.Serializable]
	public sealed class InitializePhase : BattlePhase
	{
		/// <summary>
		/// Maximum number of attempts when picking placement centers.
		/// </summary>
		private const int MaxPlacementAttempts = 128;
		/// <summary>
		/// Whether initialization is still pending.
		/// </summary>
		private bool pendingSetup;

		public override BattlePhaseId Id => BattlePhaseId.Initialize;

		/// <summary>
		/// Enters the initialize phase and prepares battle info.
		/// </summary>
		public override void Enter(BattleManager manager)
		{
			// Try to initialize battle data and request the next phase.
			pendingSetup = !TrySetupBattleInfo(manager);
			if (!pendingSetup && manager != null)
			{
				manager.RequestTransition(BattlePhaseId.Placement);
			}
		}

		/// <summary>
		/// Ticks the initialize phase until setup succeeds.
		/// </summary>
		public override void Tick(BattleManager manager, float deltaTime)
		{
			// Retry setup while it is still pending.
			if (!pendingSetup)
			{
				return;
			}

			pendingSetup = !TrySetupBattleInfo(manager);
			if (!pendingSetup && manager != null)
			{
				manager.RequestTransition(BattlePhaseId.Placement);
			}
		}

		/// <summary>
		/// Attempts to resolve battle data and compute placement centers.
		/// </summary>
		private static bool TrySetupBattleInfo(BattleManager manager)
		{
			// Resolve battle board and info from context or presenter.
			Erelia.Core.Context context = Erelia.Core.Context.Instance;
			if (context == null)
			{
				return false;
			}

			Erelia.Battle.Data data = context.BattleData;
			Erelia.Battle.Board.Model board = data != null ? data.Board : null;
			if (board == null || board.Cells == null)
			{
				Erelia.Battle.Board.Presenter presenter =
					manager != null ? manager.GetComponentInChildren<Erelia.Battle.Board.Presenter>(true) : null;
				board = presenter != null ? presenter.Model : null;
			}

			if (board == null || board.Cells == null)
			{
				return false;
			}

			Erelia.Battle.Data battleData = context.GetOrCreateBattleData();
			if (battleData.Board == null)
			{
				battleData.Board = board;
			}

			Erelia.Battle.Info info = battleData.Info;
			if (info == null)
			{
				return false;
			}

			ResolvePlacementCenters(board.Cells, info, ResolvePlacementRadius());
			return true;
		}

		/// <summary>
		/// Resolves the placement radius from the encounter table.
		/// </summary>
		private static int ResolvePlacementRadius()
		{
			// Read placement radius from the encounter table.
			Erelia.Battle.Data data = Erelia.Core.Context.Instance?.BattleData;
			Erelia.Core.Encounter.EncounterTable table = data != null ? data.EncounterTable : null;
			if (table == null)
			{
				return 0;
			}

			return Mathf.Max(0, table.PlacementRadius);
		}

		/// <summary>
		/// Computes player and enemy placement centers from the board.
		/// </summary>
		private static void ResolvePlacementCenters(
			Erelia.Battle.Voxel.Cell[,,] cells,
			Erelia.Battle.Info info,
			int radius)
		{
			// Choose placement centers within safe bounds.
			if (cells == null || info == null)
			{
				return;
			}

			int sizeX = cells.GetLength(0);
			int sizeZ = cells.GetLength(2);
			if (sizeX <= 0 || sizeZ <= 0)
			{
				return;
			}

			if (!TryPickPlacementCenter(cells, sizeX, sizeZ, radius, null, out Vector2Int playerCenter))
			{
				playerCenter = new Vector2Int(0, 0);
			}

			if (!TryPickPlacementCenter(cells, sizeX, sizeZ, radius, playerCenter, out Vector2Int enemyCenter))
			{
				enemyCenter = playerCenter;
			}

			info.PlayerPlacementCenter = playerCenter;
			info.EnemyPlacementCenter = enemyCenter;
		}

		/// <summary>
		/// Picks a valid placement center on the board.
		/// </summary>
		private static bool TryPickPlacementCenter(
			Erelia.Battle.Voxel.Cell[,,] cells,
			int sizeX,
			int sizeZ,
			int radius,
			Vector2Int? avoid,
			out Vector2Int center)
		{
			// Try random samples first, then fall back to a full scan.
			center = default;

			int minX = 0;
			int maxX = sizeX - 1;
			int minZ = 0;
			int maxZ = sizeZ - 1;

			int safeMinX = Mathf.Clamp(radius, 0, maxX);
			int safeMaxX = Mathf.Clamp(sizeX - 1 - radius, 0, maxX);
			int safeMinZ = Mathf.Clamp(radius, 0, maxZ);
			int safeMaxZ = Mathf.Clamp(sizeZ - 1 - radius, 0, maxZ);

			if (safeMinX <= safeMaxX)
			{
				minX = safeMinX;
				maxX = safeMaxX;
			}

			if (safeMinZ <= safeMaxZ)
			{
				minZ = safeMinZ;
				maxZ = safeMaxZ;
			}

			int attempts = Mathf.Max(MaxPlacementAttempts, sizeX * sizeZ);
			for (int i = 0; i < attempts; i++)
			{
				int x = Random.Range(minX, maxX + 1);
				int z = Random.Range(minZ, maxZ + 1);
				if (avoid.HasValue && avoid.Value.x == x && avoid.Value.y == z)
				{
					continue;
				}

				if (TryGetPlacementSurface(cells, x, z, out _))
				{
					center = new Vector2Int(x, z);
					return true;
				}
			}

			for (int x = 0; x < sizeX; x++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					if (avoid.HasValue && avoid.Value.x == x && avoid.Value.y == z)
					{
						continue;
					}

					if (TryGetPlacementSurface(cells, x, z, out _))
					{
						center = new Vector2Int(x, z);
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Finds the topmost valid placement surface at X/Z.
		/// </summary>
		private static bool TryGetPlacementSurface(Erelia.Battle.Voxel.Cell[,,] cells, int x, int z, out int y)
		{
			// Scan from top to bottom to find a surface cell.
			y = -1;
			if (cells == null)
			{
				return false;
			}

			int sizeY = cells.GetLength(1);
			for (int yi = sizeY - 1; yi >= 0; yi--)
			{
				Erelia.Battle.Voxel.Cell cell = cells[x, yi, z];
				if (cell == null || cell.Id < 0)
				{
					continue;
				}

				if (yi + 1 < sizeY)
				{
					Erelia.Battle.Voxel.Cell above = cells[x, yi + 1, z];
					if (above != null && above.Id >= 0)
					{
						continue;
					}
				}

				y = yi;
				return true;
			}

			return false;
		}
	}
}
