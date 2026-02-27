using UnityEngine;

namespace Erelia.Battle.Board
{
	public sealed class Presenter : MonoBehaviour
	{
		[SerializeField] private Erelia.Battle.Board.View view;
		[SerializeField] private bool rebuildOnEnable = true;
		[SerializeField] private bool alignToModelOrigin = true;
		private Erelia.Battle.Board.Model model;

		public Erelia.Battle.Board.Model Model => model;

		public void SetModel(Erelia.Battle.Board.Model newModel)
		{
			model = newModel;
			RebuildAll();
		}

		private void OnEnable()
		{
			if (rebuildOnEnable)
			{
				RebuildAll();
			}
		}

		public void RebuildAll()
		{
			if (model == null || view == null)
			{
				return;
			}

			if (alignToModelOrigin)
			{
				view.transform.position = model.Origin;
			}

			VoxelKit.Registry registry = Erelia.VoxelRegistry.Instance;
			view.SetRenderMesh(VoxelKit.Mesher.BuildRenderMesh(model.Cells, registry, VoxelKit.Mesher.AnyVoxelPredicate));
			view.SetCollisionMesh(VoxelKit.Mesher.BuildCollisionMesh(model.Cells, registry, VoxelKit.Mesher.OnlyObstacleVoxelPredicate));
			RebuildMasks();
		}

		public void RebuildMasks()
		{
			if (model == null || view == null)
			{
				return;
			}

			VoxelKit.Registry registry = Erelia.VoxelRegistry.Instance;
			Mesh maskMesh = Erelia.BattleVoxel.Mesher.BuildMaskMesh(
				model.Cells,
				registry);

			view.SetMaskMesh(maskMesh);
		}
	}
}
