using UnityEngine;

namespace Erelia.Battle.Board
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Battle.Board.View view;
		private Erelia.Battle.Board.Model model;

		public Erelia.Battle.Board.Model Model => model;

		public void SetModel(Erelia.Battle.Board.Model newModel)
		{
			model = newModel;
			RebuildAll();
		}

		private void OnEnable()
		{
			RebuildAll();
		}

		public void RebuildAll()
		{
			if (model == null || view == null)
			{
				return;
			}

			Erelia.Core.VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;
			view.SetRenderMesh(Erelia.Core.VoxelKit.Mesher.BuildRenderMesh(model.Cells, registry, Erelia.Core.VoxelKit.Mesher.AnyVoxelPredicate));
			view.SetCollisionMesh(Erelia.Core.VoxelKit.Mesher.BuildCollisionMesh(model.Cells, registry, Erelia.Core.VoxelKit.Mesher.OnlyObstacleVoxelPredicate));
			RebuildMasks();
		}

		public void RebuildMasks()
		{
			if (model == null || view == null)
			{
				return;
			}

			Erelia.Core.VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;
			Mesh maskMesh = Erelia.Battle.Voxel.Mesher.BuildMaskMesh(
				model.Cells,
				registry,
				Erelia.Battle.MaskSpriteRegistry.Instance);

			view.SetMaskMesh(maskMesh);
		}
	}
}
