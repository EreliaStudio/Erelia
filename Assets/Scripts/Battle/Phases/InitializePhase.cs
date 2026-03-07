using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Initialization phase that prepares battle data.
	/// Resolves the board, computes shared acceptable floor coordinates, then transitions to the Placement phase.
	/// </summary>
	[System.Serializable]
	public sealed class InitializePhase : BattlePhase
	{
		/// <summary>
		/// Whether initialization is still pending.
		/// </summary>
		private bool pendingSetup;

		public override BattlePhaseId Id => BattlePhaseId.Initialize;

		/// <summary>
		/// Enters the initialize phase and prepares battle data.
		/// </summary>
		public override void Enter(BattleManager manager)
		{
			// Try to initialize battle data and request the next phase.
			pendingSetup = !TrySetupBattleData();
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

			pendingSetup = !TrySetupBattleData();
			if (!pendingSetup && manager != null)
			{
				manager.RequestTransition(BattlePhaseId.Placement);
			}
		}

		/// <summary>
		/// Attempts to resolve battle data for the current encounter.
		/// </summary>
		private bool TrySetupBattleData()
		{
			Erelia.Battle.Data battleData = Erelia.Core.Context.Instance.BattleData;

			for (int x = 0; x < battleData.Board.SizeX; x++)
			{
				for (int y = 0; y < battleData.Board.SizeY; y++)
				{
					for (int z = 0; z < battleData.Board.SizeZ; z++)
					{
						if (!IsAcceptableCoordinate(battleData.Board, x, y, z))
						{
							continue;
						}

						battleData.PhaseInfo.AddAcceptableCoordinate(new Vector3Int(x, y, z));
					}
				}
			}

			return true;
		}

		private bool IsAcceptableCoordinate(Erelia.Battle.Board.Model board, int x, int y, int z)
		{
			Erelia.Battle.Voxel.Cell cell = board.Cells[x, y, z];
			if (!Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition))
			{
				return false;
			}

			return IsAcceptableAsFloor(definition) && HasAirOrWalkableBlockOnTop(board, x, y, z);
		}

		private bool IsAcceptableAsFloor(Erelia.Core.VoxelKit.Definition definition)
		{
			return definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Obstacle;
		}

		private bool HasAirOrWalkableBlockOnTop(Erelia.Battle.Board.Model board, int x, int y, int z)
		{
			int targetY = y + 1;
			if (targetY >= board.SizeY)
			{
				return false;
			}

			Erelia.Battle.Voxel.Cell cell = board.Cells[x, targetY, z];
			if (!Erelia.Exploration.World.VoxelRegistry.Instance.TryGet(cell.Id, out Erelia.Core.VoxelKit.Definition definition))
			{
				return true;
			}

			return definition.Data.Traversal == Erelia.Core.VoxelKit.Traversal.Walkable;
		}
	}
}
