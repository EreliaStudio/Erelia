using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	[System.Serializable]
	public sealed class PlacementPhase : BattlePhase
	{
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

		private readonly List<Vector3Int> maskedCells = new List<Vector3Int>();
		private bool pendingApply;
		private const Erelia.BattleVoxel.Type PlacementMask = Erelia.BattleVoxel.Type.Placement;

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

			maskedCells.Clear();
			ApplyPlacementMask(board, playerCenter, enemyCenter);
			presenter.RebuildMasks();
			return true;
		}

		private void ApplyPlacementMask(
			Erelia.Battle.Board.Model board,
			Vector2Int playerCenter,
			Vector2Int enemyCenter)
		{
			if (board == null || board.Cells == null)
			{
				return;
			}

			Erelia.BattleVoxel.Cell[,,] cells = board.Cells;
			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
			{
				return;
			}

			int radius = ResolvePlacementRadius();
			ApplyPlacementCircle(cells, sizeX, sizeZ, playerCenter, radius);
			ApplyPlacementCircle(cells, sizeX, sizeZ, enemyCenter, radius);
		}

		private void ApplyPlacementCircle(
			Erelia.BattleVoxel.Cell[,,] cells,
			int sizeX,
			int sizeZ,
			Vector2Int center,
			int radius)
		{
			if (cells == null || sizeX <= 0 || sizeZ <= 0)
			{
				return;
			}

			int minX = Mathf.Max(0, center.x - radius);
			int maxX = Mathf.Min(sizeX - 1, center.x + radius);
			int minZ = Mathf.Max(0, center.y - radius);
			int maxZ = Mathf.Min(sizeZ - 1, center.y + radius);
			int radiusSq = radius * radius;

			for (int x = minX; x <= maxX; x++)
			{
				int dx = x - center.x;
				for (int z = minZ; z <= maxZ; z++)
				{
					int dz = z - center.y;
					if ((dx * dx + dz * dz) > radiusSq)
					{
						continue;
					}

					if (!TryGetPlacementSurface(cells, x, z, out int y))
					{
						continue;
					}

					Erelia.BattleVoxel.Cell cell = cells[x, y, z];
					if (cell == null || cell.HasMask(PlacementMask))
					{
						continue;
					}

					cell.AddMask(PlacementMask);
					maskedCells.Add(new Vector3Int(x, y, z));
				}
			}
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
			if (board == null || board.Cells == null || maskedCells.Count == 0)
			{
				maskedCells.Clear();
				return;
			}

			Erelia.BattleVoxel.Cell[,,] cells = board.Cells;
			int sizeX = cells.GetLength(0);
			int sizeY = cells.GetLength(1);
			int sizeZ = cells.GetLength(2);

			for (int i = 0; i < maskedCells.Count; i++)
			{
				Vector3Int pos = maskedCells[i];
				if (pos.x < 0 || pos.x >= sizeX || pos.y < 0 || pos.y >= sizeY || pos.z < 0 || pos.z >= sizeZ)
				{
					continue;
				}

				Erelia.BattleVoxel.Cell cell = cells[pos.x, pos.y, pos.z];
				if (cell == null)
				{
					continue;
				}

				cell.RemoveMask(PlacementMask);
			}

			maskedCells.Clear();
		}

		private static bool TryGetPlacementSurface(Erelia.BattleVoxel.Cell[,,] cells, int x, int z, out int y)
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
				if (cell == null || cell.Id < 0)
				{
					continue;
				}

				if (yi + 1 < sizeY)
				{
					Erelia.BattleVoxel.Cell above = cells[x, yi + 1, z];
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
