using UnityEngine;

namespace Battle.Board.Controller
{
	public class BodyCollider : MonoBehaviour
	{
		[SerializeField] private Battle.Board.Controller.VoxelCollider voxelCollider = null;

		private void Awake()
		{
			InitializeSolidCollider();
		}

		private void InitializeSolidCollider()
		{
			var go = new GameObject("VoxelCollider");
			go.transform.SetParent(transform, false);
			voxelCollider = go.AddComponent<Battle.Board.Controller.VoxelCollider>();
		}

		public void Rebuild(Battle.Board.Model.Data data)
		{
			voxelCollider.Rebuild(data);
		}
	}
}
