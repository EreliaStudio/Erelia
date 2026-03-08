using System.Collections.Generic;
using Erelia.Core;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using PhaseId = Erelia.Battle.Phase.Id;
using PhaseRoot = Erelia.Battle.Phase.Root;

namespace Erelia.Battle.Phase.Placement
{
	/// <summary>
	/// Placement phase that applies precomputed placement masks and handles unit placement.
	/// </summary>
	[System.Serializable]
	[MovedFrom(true, sourceNamespace: "Erelia.Battle", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlacementPhase")]
	public sealed class MainRoot : PhaseRoot
	{
		[SerializeField] private GameObject hudRoot = null;

		public override PhaseId Id => PhaseId.Placement;

		/// <summary>
		/// Presenter used to access the battle board.
		/// </summary>
		[SerializeField] private Erelia.Battle.Board.Presenter boardPresenter;

		/// <summary>
		/// Enters the placement phase and applies placement masks.
		/// </summary>
		public override void Enter(BattleManager manager)
		{
			if (hudRoot == null)
			{
				Debug.LogWarning("[Erelia.Battle.Phase.Placement.MainRoot] HUD root can't be empty");
			}

			if (hudRoot != null)
			{
				hudRoot.SetActive(true);
			}

			InitializePlacementMaskCells();
		}

		/// <summary>
		/// Exits the placement phase and clears placement masks.
		/// </summary>
		public override void Exit(BattleManager manager)
		{
			ClearPlacementMaskCells();

			if (hudRoot != null)
			{
				hudRoot.SetActive(false);
			}
		}

		/// <summary>
		/// Ticks the placement phase until masks are applied.
		/// </summary>
		public override void Tick(BattleManager manager, float deltaTime)
		{
		}

		/// <summary>
		/// Handles confirm input during placement.
		/// </summary>
		public override void OnConfirm(Erelia.Battle.Player.BattlePlayerController controller)
		{
		}

		/// <summary>
		/// Handles cancel input during placement.
		/// </summary>
		public override void OnCancel(Erelia.Battle.Player.BattlePlayerController controller)
		{
		}

		private void InitializePlacementMaskCells()
		{
			GetAcceptableCoordinates(out Erelia.Battle.Board.Model board, out IReadOnlyList<Vector3Int> acceptableCoordinates);

			for (int i = 0; i < acceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = acceptableCoordinates[i];
				if (!IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				if (!Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition))
				{
					continue;
				}

				if (!IsInPlacementPolicy(definition, coordinate.x, coordinate.y, coordinate.z) ||
					!HasAvailableSpace(board, coordinate.x, coordinate.y, coordinate.z))
				{
					continue;
				}

				cell.AddMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private void ClearPlacementMaskCells()
		{
			GetAcceptableCoordinates(out Erelia.Battle.Board.Model board, out IReadOnlyList<Vector3Int> acceptableCoordinates);

			for (int i = 0; i < acceptableCoordinates.Count; i++)
			{
				Vector3Int coordinate = acceptableCoordinates[i];
				if (!IsInsideBoard(board, coordinate))
				{
					continue;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[coordinate.x, coordinate.y, coordinate.z];
				cell.RemoveMask(Erelia.Battle.Voxel.Mask.Type.Placement);
			}

			boardPresenter?.RebuildMasks();
		}

		private void GetAcceptableCoordinates(
			out Erelia.Battle.Board.Model board,
			out IReadOnlyList<Vector3Int> acceptableCoordinates)
		{
			Erelia.Battle.Data battleData = Context.Instance.BattleData;
			board = battleData.Board;
			acceptableCoordinates = battleData.PhaseInfo.AcceptableCoordinates;
		}

		private bool IsInPlacementPolicy(Erelia.Core.VoxelKit.Definition definition, int x, int y, int z)
		{
			return z < Context.Instance.BattleData.Board.SizeZ / 2;
		}

		private bool HasAvailableSpace(Erelia.Battle.Board.Model board, int x, int y, int z)
		{
			const int PlayerHeight = 2;
			for (int deltaY = 1; deltaY < PlayerHeight; deltaY++)
			{
				int targetY = y + deltaY;
				if (targetY >= board.SizeY)
				{
					return false;
				}

				Erelia.Battle.Voxel.Cell cell = board.Cells[x, targetY, z];
				if (Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition) &&
					definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Obstacle)
				{
					return false;
				}
			}

			return true;
		}

		private bool IsInsideBoard(Erelia.Battle.Board.Model board, Vector3Int coordinate)
		{
			return coordinate.x >= 0 && coordinate.x < board.SizeX &&
				coordinate.y >= 0 && coordinate.y < board.SizeY &&
				coordinate.z >= 0 && coordinate.z < board.SizeZ;
		}
	}
}
