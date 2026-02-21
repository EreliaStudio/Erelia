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

			if (view != null)
			{
				view.SolidCollisionEntered += OnSolidCollisionEntered;
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.Presenter] Bind called with null view.");
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

			if (view != null)
			{
				view.SolidCollisionEntered -= OnSolidCollisionEntered;
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

			if (!Erelia.Voxel.Mesher.TryBuild(model.Cells, out Mesh renderMesh, out Mesh unusedCollisionMesh, null))
			{
				DestroyMesh(renderMesh);
				DestroyMesh(unusedCollisionMesh);
				view.DisposeMeshes();
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.Presenter] Mesh build failed. Cleared view meshes.");
				return;
			}

			Mesh collisionMesh = null;
			if (Erelia.Voxel.Mesher.TryBuild(model.Cells, out Mesh unusedRenderMesh, out Mesh builtCollisionMesh, IsObstacleCell))
			{
				collisionMesh = builtCollisionMesh;
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.Presenter] Collision mesh build failed. Using render mesh only.");
			}

			DestroyMesh(unusedCollisionMesh);
			DestroyMesh(unusedRenderMesh);

			view.ApplyMeshes(renderMesh, collisionMesh);
			Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Meshes applied to view.");
		}

		private static bool IsObstacleCell(
			Erelia.Voxel.Cell[,,] cells,
			int x,
			int y,
			int z,
			Erelia.Voxel.Cell cell,
			Erelia.Voxel.Definition definition)
		{
			return definition != null
				&& definition.Data != null
				&& definition.Data.Traversal == Erelia.Voxel.Traversal.Obstacle;
		}

		private void OnSolidCollisionEntered(Collision collision)
		{
			Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Solid collision entered.");
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

