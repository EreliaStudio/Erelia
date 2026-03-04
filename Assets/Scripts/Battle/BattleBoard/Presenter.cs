using UnityEngine;

namespace Erelia.Battle.Board
{
	/// <summary>
	/// Presenter that binds a battle board model to its view.
	/// Rebuilds render, collision, and mask meshes when the model is set or enabled.
	/// </summary>
	public sealed class Presenter : MonoBehaviour
	{
		/// <summary>
		/// View used to display the battle board.
		/// </summary>
		[SerializeField] private Erelia.Battle.Board.View view;
		/// <summary>
		/// Current board model bound to the presenter.
		/// </summary>
		private Erelia.Battle.Board.Model model;

		/// <summary>
		/// Gets the currently assigned board model.
		/// </summary>
		public Erelia.Battle.Board.Model Model => model;

		/// <summary>
		/// Assigns a new board model and rebuilds meshes.
		/// </summary>
		public void SetModel(Erelia.Battle.Board.Model newModel)
		{
			// Store the model and rebuild visuals.
			model = newModel;
			RebuildAll();
		}

		/// <summary>
		/// Unity callback invoked when the component is enabled.
		/// </summary>
		private void OnEnable()
		{
			// Rebuild visuals on enable in case the model was set earlier.
			RebuildAll();
		}

		/// <summary>
		/// Rebuilds render, collision, and mask meshes.
		/// </summary>
		public void RebuildAll()
		{
			// Skip rebuild if prerequisites are missing.
			if (model == null || view == null)
			{
				return;
			}

			Erelia.Core.VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;
			view.SetRenderMesh(Erelia.Core.VoxelKit.Mesher.BuildRenderMesh(model.Cells, registry, Erelia.Core.VoxelKit.Mesher.AnyVoxelPredicate));
			view.SetCollisionMesh(Erelia.Core.VoxelKit.Mesher.BuildCollisionMesh(model.Cells, registry, Erelia.Core.VoxelKit.Mesher.OnlyObstacleVoxelPredicate));
			RebuildMasks();
		}

		/// <summary>
		/// Rebuilds the mask mesh for overlays.
		/// </summary>
		public void RebuildMasks()
		{
			// Build and apply the mask mesh when possible.
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
