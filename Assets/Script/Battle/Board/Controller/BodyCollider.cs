using UnityEngine;

namespace Battle.Board.Controller
{
	public class BodyCollider : MonoBehaviour
	{
		[SerializeField] private Battle.Board.Controller.VoxelCollider voxelCollider = null;
		[SerializeField] private Battle.Board.Controller.MaskCollider maskCollider = null;

		private void Awake()
		{
			InitializeSolidCollider();
			InitializeMaskCollider();
		}

		private void InitializeSolidCollider()
		{
			var go = new GameObject("VoxelCollider");
			go.transform.SetParent(transform, false);
			voxelCollider = go.AddComponent<Battle.Board.Controller.VoxelCollider>();
		}

		private void InitializeMaskCollider()
		{
			var go = new GameObject("BushChunkCollider");
			go.transform.SetParent(transform, false);
			maskCollider = go.AddComponent<Battle.Board.Controller.MaskCollider>();
		}

		public void Rebuild(Battle.Board.Model.Data data)
		{
			// voxelCollider.Rebuild(data);
			// maskCollider.Rebuild(data);
		}
	}
}
