using UnityEngine;

namespace Erelia.Exploration.World.Chunk
{
	public sealed class Presenter
	{
		private readonly Erelia.Exploration.World.Chunk.Model model;
		private readonly Erelia.Exploration.World.Chunk.View view;

		public Presenter(Erelia.Exploration.World.Chunk.Model model, Erelia.Exploration.World.Chunk.View view)
		{
			this.model = model;
			this.view = view;
		}

		public void Bind()
		{
			if (model != null)
			{
				model.Validated += OnValidated;
			}
		}

		public void Unbind()
		{
			if (model != null)
			{
				model.Validated -= OnValidated;
			}
		}

		public void ForceRebuild()
		{
			RebuildMeshes();
		}

		private void OnValidated(Erelia.Exploration.World.Chunk.Model validatedModel)
		{
			RebuildMeshes();
		}

		private void RebuildMeshes()
		{
			if (model == null || view == null)
			{
				return;
			}

			VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;

			view.SetRenderMesh(VoxelKit.Mesher.BuildRenderMesh(model.Cells, registry, VoxelKit.Mesher.AnyVoxelPredicate));
			view.SetCollisionMesh(VoxelKit.Mesher.BuildCollisionMesh(model.Cells, registry, VoxelKit.Mesher.OnlyObstacleVoxelPredicate));

		}

		private static void DestroyMesh(Mesh mesh)
		{
			if (mesh == null)
			{
				return;
			}

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


