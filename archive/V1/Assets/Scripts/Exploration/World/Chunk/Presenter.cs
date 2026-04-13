using UnityEngine;

namespace Erelia.Exploration.World.Chunk
{
	public sealed class Presenter
	{
		private readonly Erelia.Exploration.World.Chunk.ChunkData model;

		private readonly Erelia.Exploration.World.Chunk.View view;

		public Presenter(Erelia.Exploration.World.Chunk.ChunkData model, Erelia.Exploration.World.Chunk.View view)
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

		private void OnValidated(Erelia.Exploration.World.Chunk.ChunkData validatedModel)
		{
			RebuildMeshes();
		}

		private void RebuildMeshes()
		{
			if (model == null || view == null)
			{
				return;
			}

			Erelia.Core.Voxel.VoxelRegistry registry = Erelia.Exploration.World.VoxelCatalog.Instance;

			view.SetRenderMesh(Erelia.Core.Voxel.Mesher.BuildRenderMesh(model.Cells, registry, Erelia.Core.Voxel.Mesher.AnyVoxelPredicate));
			view.SetCollisionMesh(Erelia.Core.Voxel.Mesher.BuildCollisionMesh(model.Cells, registry, Erelia.Core.Voxel.Mesher.OnlyObstacleVoxelPredicate));

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



