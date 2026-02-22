using UnityEngine;

namespace Erelia.World.Chunk
{
	public sealed class Presenter
	{
		private readonly Erelia.World.Chunk.Model model;
		private readonly Erelia.World.Chunk.View view;

		public Presenter(Erelia.World.Chunk.Model model, Erelia.World.Chunk.View view)
		{
			this.model = model;
			this.view = view;
		}

		public void Bind()
		{
			if (model != null)
			{
				model.Validated += OnValidated;
				Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Bound to model validation.");
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.Presenter] Bind called with null model.");
			}
		}

		public void Unbind()
		{
			if (model != null)
			{
				model.Validated -= OnValidated;
				Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Unbound from model validation.");
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.Presenter] Unbind called with null model.");
			}
		}

		public void ForceRebuild()
		{
			Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Force rebuild requested.");
			RebuildMeshes();
		}

		private void OnValidated(Erelia.World.Chunk.Model validatedModel)
		{
			Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Model validated. Rebuilding meshes.");
			RebuildMeshes();
		}

		private void RebuildMeshes()
		{
			if (model == null || view == null)
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.Presenter] Cannot rebuild meshes. Model or view is null.");
				return;
			}

			view.SetRenderMesh(Erelia.Voxel.Mesher.BuildRenderMesh(model.Cells, Erelia.Voxel.Mesher.AnyVoxelPredicate));
			view.SetCollisionMesh(Erelia.Voxel.Mesher.BuildCollisionMesh(model.Cells, Erelia.Voxel.Mesher.OnlyObstacleVoxelPredicate));

			Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Meshes applied to view.");
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

