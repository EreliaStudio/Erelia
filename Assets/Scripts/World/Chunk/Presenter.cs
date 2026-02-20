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
				view.BushTriggerEntered += OnBushTriggerEntered;
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
				view.BushTriggerEntered -= OnBushTriggerEntered;
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

			if (!Erelia.Voxel.Mesher.TryBuild(model.Cells, out Mesh solidRenderMesh, out Mesh solidCollisionMesh, IsSolidCell))
			{
				Erelia.Logger.RaiseError("[Erelia.World.Chunk.Presenter] Mesher failed to build solid chunk meshes.");
				return;
			}

			if (!Erelia.Voxel.Mesher.TryBuild(model.Cells, out Mesh bushRenderMesh, out Mesh bushCollisionMesh, IsBushCell))
			{
				Erelia.Logger.RaiseError("[Erelia.World.Chunk.Presenter] Mesher failed to build bush chunk meshes.");
				return;
			}

			view.ApplyMeshes(solidRenderMesh, solidCollisionMesh, bushRenderMesh, bushCollisionMesh);
			Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Meshes applied to view.");
		}

		private static bool IsSolidCell(
			Erelia.Voxel.Cell[,,] cells,
			int x,
			int y,
			int z,
			Erelia.Voxel.Cell cell,
			Erelia.Voxel.Definition definition)
		{
			return definition != null
				&& definition.Data != null
				&& definition.Data.Collision == Erelia.Voxel.Collision.Solid;
		}

		private static bool IsBushCell(
			Erelia.Voxel.Cell[,,] cells,
			int x,
			int y,
			int z,
			Erelia.Voxel.Cell cell,
			Erelia.Voxel.Definition definition)
		{
			return definition != null
				&& definition.Data != null
				&& definition.Data.Collision == Erelia.Voxel.Collision.Bush;
		}

		private void OnSolidCollisionEntered(Collision collision)
		{
			Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Solid collision entered.");
		}

		private void OnBushTriggerEntered(Collider collider)
		{
			Erelia.Logger.Log("[Erelia.World.Chunk.Presenter] Bush trigger entered.");
		}
	}
}

