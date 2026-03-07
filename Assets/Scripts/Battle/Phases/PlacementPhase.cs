using Erelia.Core;
using Erelia.Exploration.World;
using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Placement phase that computes placement masks and handles unit placement.
	/// </summary>
	[System.Serializable]
	public sealed class PlacementPhase : BattlePhase
	{
		[SerializeField] private GameObject hudRoot = null;

		public override BattlePhaseId Id => BattlePhaseId.Placement;

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
				Debug.LogWarning("[Erelia.Battle.PlacementPhase] HUD root can't be empty");
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
			Erelia.Battle.Board.Model board = Erelia.Core.Context.Instance.BattleData.Board;
			if (board == null)
			{
				return;
			}

			for (int x = 0; x < board.SizeX; x++)
			{
				for (int y = 0; y < board.SizeY; y++)
				{
					for (int z = 0; z < board.SizeZ; z++)
					{
						Erelia.Battle.Voxel.Cell cell = Context.Instance.BattleData.Board.Cells[x, y, z];

						if (VoxelRegistry.Instance.TryGet(cell.Id, out Core.VoxelKit.Definition definition))
						{
							if (IsAcceptableAsFloor(definition) &&
								IsInPlacementPolicy(definition, x, y, z) && 
								HasAvailableSpace(definition, x, y, z))
							{
								cell.AddMask(Voxel.Mask.Type.Placement);
							}
						}
					}
				}
			}

			boardPresenter.RebuildMasks();
		}

		private bool IsAcceptableAsFloor(Erelia.Core.VoxelKit.Definition definition)
		{
			return definition.Data.Traversal == Core.VoxelKit.Traversal.Obstacle;
		}

		private bool IsInPlacementPolicy(Erelia.Core.VoxelKit.Definition definition, int x, int y, int z)
		{
			if (z < Erelia.Core.Context.Instance.BattleData.Board.SizeZ / 2)
			{
				return false;
			}

			return true;
		}

		private bool HasAvailableSpace(Erelia.Core.VoxelKit.Definition definition, int x, int y, int z)
		{
			bool availableSpace = true;
			const int PlayerHeight = 2;
			for (int deltaY = 1; deltaY < PlayerHeight; deltaY++)
			{
				Erelia.Battle.Voxel.Cell cell = Context.Instance.BattleData.Board.Cells[x, y, z];

				if (VoxelRegistry.Instance.TryGet(cell.Id, out Core.VoxelKit.Definition tmpDefinition))
				{
					if (tmpDefinition.Data.Traversal == Core.VoxelKit.Traversal.Obstacle)
					{
						availableSpace = false;
					}
				}
			}
			return availableSpace;
		}
	}
}
