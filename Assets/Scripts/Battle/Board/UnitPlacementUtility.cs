using UnityEngine;

namespace Erelia.Battle.Board
{
	public static class UnitPlacementUtility
	{
		private static readonly Vector3 DefaultStationaryOffset = new Vector3(0.5f, 1f, 0.5f);

		public static bool IsInsideBoard(Erelia.Battle.Board.Model board, Vector3Int coordinate)
		{
			return board != null &&
				coordinate.x >= 0 && coordinate.x < board.SizeX &&
				coordinate.y >= 0 && coordinate.y < board.SizeY &&
				coordinate.z >= 0 && coordinate.z < board.SizeZ;
		}

		public static bool TryResolveWorldPosition(
			Erelia.Battle.Board.Model board,
			Erelia.Battle.Board.Presenter boardPresenter,
			Vector3Int targetCell,
			out Vector3 worldPosition)
		{
			worldPosition = default;

			if (!IsInsideBoard(board, targetCell))
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cell = board.Cells[targetCell.x, targetCell.y, targetCell.z];
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

		private static Vector3 ResolveStationaryOffset(Erelia.Battle.Voxel.Cell cell)
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
	}
}
