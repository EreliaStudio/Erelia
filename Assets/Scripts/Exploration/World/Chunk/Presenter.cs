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

			Erelia.Core.VoxelKit.Registry registry = Erelia.Exploration.World.VoxelRegistry.Instance;

			view.SetRenderMesh(Erelia.Core.VoxelKit.Mesher.BuildRenderMesh(model.Cells, registry, Erelia.Core.VoxelKit.Mesher.AnyVoxelPredicate));
			view.SetCollisionMesh(Erelia.Core.VoxelKit.Mesher.BuildCollisionMesh(model.Cells, registry, Erelia.Core.VoxelKit.Mesher.OnlyObstacleVoxelPredicate));

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


