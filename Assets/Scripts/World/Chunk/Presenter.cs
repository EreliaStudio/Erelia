using UnityEngine;

namespace Erelia.World.Chunk
{
	public sealed class Presenter
	{
		private readonly Erelia.World.Chunk.Model model;
		private readonly Erelia.World.Chunk.View view;
		private readonly VoxelKit.Registry registry;

		public Presenter(Erelia.World.Chunk.Model model, Erelia.World.Chunk.View view, VoxelKit.Registry registry)
		{
			this.model = model;
			this.view = view;
			this.registry = registry;
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

		private void OnValidated(Erelia.World.Chunk.Model validatedModel)
		{
			RebuildMeshes();
		}

		private void RebuildMeshes()
		{
			if (model == null || view == null)
			{
				return;
			}

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


