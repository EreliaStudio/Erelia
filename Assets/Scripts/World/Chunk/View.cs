using UnityEngine;

namespace Erelia.World.Chunk
{
	public sealed class View : MonoBehaviour
	{
		[SerializeField] private Erelia.World.Chunk.SolidView solidView;
		[SerializeField] private Erelia.World.Chunk.BushView bushView;

		public event System.Action<Collision> SolidCollisionEntered;
		public event System.Action<Collider> BushTriggerEntered;

		private void OnEnable()
		{
			if (solidView != null)
			{
				solidView.CollisionEntered += OnSolidCollisionEntered;
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.View] Solid view is not assigned.");
			}

			if (bushView != null)
			{
				bushView.TriggerEntered += OnBushTriggerEntered;
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.View] Bush view is not assigned.");
			}
		}

		private void OnDisable()
		{
			if (solidView != null)
			{
				solidView.CollisionEntered -= OnSolidCollisionEntered;
			}

			if (bushView != null)
			{
				bushView.TriggerEntered -= OnBushTriggerEntered;
			}
		}

		public void ApplyMeshes(Mesh solidRenderMesh, Mesh solidCollisionMesh, Mesh bushRenderMesh, Mesh bushCollisionMesh)
		{
			DisposeMeshes();

			if (solidView != null)
			{
				solidView.ApplyMeshes(solidRenderMesh, solidCollisionMesh);
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.View] Solid view is not assigned.");
			}

			if (bushView != null)
			{
				bushView.ApplyMeshes(bushRenderMesh, bushCollisionMesh);
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.Chunk.View] Bush view is not assigned.");
			}

			Erelia.Logger.Log("[Erelia.World.Chunk.View] Meshes applied.");
		}

		public void DisposeMeshes()
		{
			if (solidView != null)
			{
				solidView.DisposeMeshes();
			}

			if (bushView != null)
			{
				bushView.DisposeMeshes();
			}
		}

		private void OnSolidCollisionEntered(Collision collision)
		{
			SolidCollisionEntered?.Invoke(collision);
		}

		private void OnBushTriggerEntered(Collider collider)
		{
			BushTriggerEntered?.Invoke(collider);
		}
	}
}

