using UnityEngine;

namespace Erelia.Battle.Board
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Battle.Board.View view;
		private Erelia.Battle.Board.BattleBoardState board;

		public Erelia.Battle.Board.BattleBoardState Board => board;

		public void SetBoard(Erelia.Battle.Board.BattleBoardState newBoard)
		{
			board = newBoard;
			RebuildAll();
		}

		private void OnEnable()
		{
			RebuildAll();
		}

		public void RebuildAll()
		{
			if (board == null || view == null)
			{
				return;
			}

			Erelia.Core.Voxel.VoxelRegistry registry = Erelia.Exploration.World.VoxelCatalog.Instance;
			view.SetRenderMesh(Erelia.Core.Voxel.Mesher.BuildRenderMesh(board.Cells, registry, Erelia.Core.Voxel.Mesher.AnyVoxelPredicate));
			view.SetCollisionMesh(Erelia.Core.Voxel.Mesher.BuildCollisionMesh(board.Cells, registry, Erelia.Core.Voxel.Mesher.OnlyObstacleVoxelPredicate));
			RebuildMasks();
		}

		public void RebuildMasks()
		{
			if (board == null || view == null)
			{
				return;
			}

			Erelia.Core.Voxel.VoxelRegistry registry = Erelia.Exploration.World.VoxelCatalog.Instance;
			Mesh maskMesh = Erelia.Battle.Voxel.Mesher.BuildMaskMesh(
				board.Cells,
				registry,
				Erelia.Battle.MaskSpriteRegistry.Instance);

			view.SetMaskMesh(maskMesh);
		}
	}
}


