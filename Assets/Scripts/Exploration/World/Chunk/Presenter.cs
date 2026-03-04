using UnityEngine;

namespace Erelia.Exploration.World.Chunk
{
	/// <summary>
	/// Presenter that binds a chunk model to its view and rebuilds meshes on updates.
	/// Subscribes to model validation and rebuilds render/collision meshes using the voxel registry.
	/// </summary>
	public sealed class Presenter
	{
		/// <summary>
		/// Chunk model backing this presenter.
		/// </summary>
		private readonly Erelia.Exploration.World.Chunk.Model model;

		/// <summary>
		/// Chunk view used for rendering.
		/// </summary>
		private readonly Erelia.Exploration.World.Chunk.View view;

		/// <summary>
		/// Creates a new chunk presenter.
		/// </summary>
		/// <param name="model">Chunk model.</param>
		/// <param name="view">Chunk view.</param>
		public Presenter(Erelia.Exploration.World.Chunk.Model model, Erelia.Exploration.World.Chunk.View view)
		{
			this.model = model;
			this.view = view;
		}

		/// <summary>
		/// Subscribes to model events.
		/// </summary>
		public void Bind()
		{
			// Listen to model validation to rebuild meshes.
			if (model != null)
			{
				model.Validated += OnValidated;
			}
		}

		/// <summary>
		/// Unsubscribes from model events.
		/// </summary>
		public void Unbind()
		{
			// Stop listening to model validation.
			if (model != null)
			{
				model.Validated -= OnValidated;
			}
		}

		/// <summary>
		/// Forces a mesh rebuild immediately.
		/// </summary>
		public void ForceRebuild()
		{
			// Rebuild render and collision meshes.
			RebuildMeshes();
		}

		/// <summary>
		/// Handler invoked when the model is validated.
		/// </summary>
		/// <param name="validatedModel">Validated model instance.</param>
		private void OnValidated(Erelia.Exploration.World.Chunk.Model validatedModel)
		{
			// Rebuild when the model signals changes.
			RebuildMeshes();
		}

		/// <summary>
		/// Rebuilds render and collision meshes for the chunk.
		/// </summary>
		private void RebuildMeshes()
		{
			if (model == null || view == null)
			{
				return;
			}

			// Resolve voxel registry for meshing.
			Erelia.Core.VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;

			// Build meshes from voxel cells.
			view.SetRenderMesh(Erelia.Core.VoxelKit.Mesher.BuildRenderMesh(model.Cells, registry, Erelia.Core.VoxelKit.Mesher.AnyVoxelPredicate));
			view.SetCollisionMesh(Erelia.Core.VoxelKit.Mesher.BuildCollisionMesh(model.Cells, registry, Erelia.Core.VoxelKit.Mesher.OnlyObstacleVoxelPredicate));

		}

		/// <summary>
		/// Destroys a mesh using the proper Unity API depending on play mode.
		/// </summary>
		/// <param name="mesh">Mesh to destroy.</param>
		private static void DestroyMesh(Mesh mesh)
		{
			// Ignore null meshes.
			if (mesh == null)
			{
				return;
			}

			// Use the correct destroy method depending on play mode.
			if (Application.isPlaying)
			{
				Object.Destroy(mesh);
			}
			else
			{
				Object.DestroyImmediate(mesh);
			}
		}

	}
}


