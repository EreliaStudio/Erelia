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

			if (!Erelia.Voxel.Mesher.TryBuild(model.Cells, out Mesh renderMesh, out Mesh collisionMesh))
			{
				Erelia.Logger.RaiseError("[Erelia.World.Chunk.Presenter] Mesher failed to build chunk meshes.");
				return;
			}

			view.ApplyMeshes(renderMesh, collisionMesh);
			Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Meshes applied to view.");
		}
	}
}

