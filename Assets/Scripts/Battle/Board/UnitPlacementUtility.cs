using UnityEngine;

namespace Erelia.Battle.Board
{
	public static class UnitPlacementUtility
	{
		private const float HorizontalAlignmentEpsilon = 0.001f;

		private static readonly Vector3 DefaultStationaryOffset = new Vector3(0.5f, 1f, 0.5f);
		private static readonly Vector3 DefaultPositiveXOffset = new Vector3(1f, 1f, 0.5f);
		private static readonly Vector3 DefaultNegativeXOffset = new Vector3(0f, 1f, 0.5f);
		private static readonly Vector3 DefaultPositiveZOffset = new Vector3(0.5f, 1f, 1f);
		private static readonly Vector3 DefaultNegativeZOffset = new Vector3(0.5f, 1f, 0f);

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
			return TryResolveWorldPosition(
				board,
				boardPresenter,
				targetCell,
				Erelia.Battle.Voxel.CardinalPoint.Stationary,
				out worldPosition);
		}

		public static bool TryResolveWorldPosition(
			Erelia.Battle.Board.Model board,
			Erelia.Battle.Board.Presenter boardPresenter,
			Vector3Int targetCell,
			Erelia.Battle.Voxel.CardinalPoint point,
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

			Vector3 localPosition = new Vector3(targetCell.x, targetCell.y, targetCell.z) + ResolveCardinalOffset(cell, point);
			worldPosition = boardPresenter != null
				? boardPresenter.transform.TransformPoint(localPosition)
				: localPosition;
			return true;
		}

		public static bool TryResolveMovementStepWorldPositions(
			Erelia.Battle.Board.Model board,
			Erelia.Battle.Board.Presenter boardPresenter,
			Vector3Int currentCell,
			Vector3Int nextCell,
			out Vector3 nextEntryWorldPosition,
			out Vector3 nextStationaryWorldPosition)
		{
			nextEntryWorldPosition = default;
			nextStationaryWorldPosition = default;

			if (!TryResolveEntryPointForStep(currentCell, nextCell, out Erelia.Battle.Voxel.CardinalPoint entryPoint))
			{
				return false;
			}

			return TryResolveWorldPosition(board, boardPresenter, nextCell, entryPoint, out nextEntryWorldPosition) &&
				TryResolveWorldPosition(board, boardPresenter, nextCell, Erelia.Battle.Voxel.CardinalPoint.Stationary, out nextStationaryWorldPosition);
		}

		public static bool TryCanTraverseMovementStep(
			Erelia.Battle.Board.Model board,
			Vector3Int currentCell,
			Vector3Int nextCell,
			float maximumVerticalGap,
			out Vector3 pointDelta)
		{
			pointDelta = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			if (!TryResolveMovementTransitionLocalPositions(
					board,
					currentCell,
					nextCell,
					out Vector3 currentExitLocalPosition,
					out Vector3 nextEntryLocalPosition))
			{
				return false;
			}

			pointDelta = nextEntryLocalPosition - currentExitLocalPosition;
			return Mathf.Abs(pointDelta.x) <= HorizontalAlignmentEpsilon &&
				Mathf.Abs(pointDelta.z) <= HorizontalAlignmentEpsilon &&
				Mathf.Abs(pointDelta.y) <= maximumVerticalGap;
		}

		private static bool TryResolveEntryPointForStep(
			Vector3Int currentCell,
			Vector3Int nextCell,
			out Erelia.Battle.Voxel.CardinalPoint entryPoint)
		{
			if (TryResolveTransitionPoints(currentCell, nextCell, out _, out entryPoint))
			{
				return true;
			}

			entryPoint = Erelia.Battle.Voxel.CardinalPoint.Stationary;
			return false;
		}

		private static bool TryResolveTransitionPoints(
			Vector3Int currentCell,
			Vector3Int nextCell,
			out Erelia.Battle.Voxel.CardinalPoint exitPoint,
			out Erelia.Battle.Voxel.CardinalPoint entryPoint)
		{
			Vector3Int delta = nextCell - currentCell;
			bool isPositiveXMove = delta.x == 1 && delta.z == 0;
			bool isNegativeXMove = delta.x == -1 && delta.z == 0;
			bool isPositiveZMove = delta.z == 1 && delta.x == 0;
			bool isNegativeZMove = delta.z == -1 && delta.x == 0;

			switch (true)
			{
				case true when isPositiveXMove:
					exitPoint = Erelia.Battle.Voxel.CardinalPoint.PositiveX;
					entryPoint = Erelia.Battle.Voxel.CardinalPoint.NegativeX;
					return true;
				case true when isNegativeXMove:
					exitPoint = Erelia.Battle.Voxel.CardinalPoint.NegativeX;
					entryPoint = Erelia.Battle.Voxel.CardinalPoint.PositiveX;
					return true;
				case true when isPositiveZMove:
					exitPoint = Erelia.Battle.Voxel.CardinalPoint.PositiveZ;
					entryPoint = Erelia.Battle.Voxel.CardinalPoint.NegativeZ;
					return true;
				case true when isNegativeZMove:
					exitPoint = Erelia.Battle.Voxel.CardinalPoint.NegativeZ;
					entryPoint = Erelia.Battle.Voxel.CardinalPoint.PositiveZ;
					return true;
				default:
					exitPoint = Erelia.Battle.Voxel.CardinalPoint.Stationary;
					entryPoint = Erelia.Battle.Voxel.CardinalPoint.Stationary;
					return false;
			}
		}

		private static bool TryResolveMovementTransitionLocalPositions(
			Erelia.Battle.Board.Model board,
			Vector3Int currentCell,
			Vector3Int nextCell,
			out Vector3 currentExitLocalPosition,
			out Vector3 nextEntryLocalPosition)
		{
			currentExitLocalPosition = default;
			nextEntryLocalPosition = default;
			if (!TryResolveTransitionPoints(currentCell, nextCell, out Erelia.Battle.Voxel.CardinalPoint exitPoint, out Erelia.Battle.Voxel.CardinalPoint entryPoint))
			{
				return false;
			}

			return TryResolveLocalPosition(board, currentCell, exitPoint, out currentExitLocalPosition) &&
				TryResolveLocalPosition(board, nextCell, entryPoint, out nextEntryLocalPosition);
		}

		private static bool TryResolveLocalPosition(
			Erelia.Battle.Board.Model board,
			Vector3Int targetCell,
			Erelia.Battle.Voxel.CardinalPoint point,
			out Vector3 localPosition)
		{
			localPosition = default;
			if (!IsInsideBoard(board, targetCell))
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cell = board.Cells[targetCell.x, targetCell.y, targetCell.z];
			if (cell == null)
			{
				return false;
			}

			localPosition = new Vector3(targetCell.x, targetCell.y, targetCell.z) + ResolveCardinalOffset(cell, point);
			return true;
		}

		private static Vector3 ResolveCardinalOffset(
			Erelia.Battle.Voxel.Cell cell,
			Erelia.Battle.Voxel.CardinalPoint point)
		{
			if (cell == null)
			{
				return ResolveDefaultOffset(point);
			}

			if (!Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition))
			{
				return ResolveDefaultOffset(point);
			}

			if (definition is Erelia.Battle.Voxel.Definition battleDefinition &&
				battleDefinition.MaskShape != null)
			{
				return battleDefinition.MaskShape.GetCardinalPoint(
					point,
					cell.Orientation,
					cell.FlipOrientation);
			}

			return ResolveDefaultOffset(point);
		}

		private static Vector3 ResolveDefaultOffset(Erelia.Battle.Voxel.CardinalPoint point)
		{
			switch (point)
			{
				case Erelia.Battle.Voxel.CardinalPoint.PositiveX:
					return DefaultPositiveXOffset;
				case Erelia.Battle.Voxel.CardinalPoint.NegativeX:
					return DefaultNegativeXOffset;
				case Erelia.Battle.Voxel.CardinalPoint.PositiveZ:
					return DefaultPositiveZOffset;
				case Erelia.Battle.Voxel.CardinalPoint.NegativeZ:
					return DefaultNegativeZOffset;
				case Erelia.Battle.Voxel.CardinalPoint.Stationary:
				default:
					return DefaultStationaryOffset;
			}
		}
	}
}
