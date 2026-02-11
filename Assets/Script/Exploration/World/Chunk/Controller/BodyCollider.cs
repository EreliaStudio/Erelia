using UnityEngine;

namespace World.Chunk.Controller
{
	public class BodyCollider : MonoBehaviour
	{
		[SerializeField] private World.Chunk.Controller.SolidChunkCollider solidCollider = null;
		[SerializeField] private World.Chunk.Controller.BushChunkCollider bushCollider = null;

		public World.Chunk.Model.Coordinates Coordinates { get; private set; }

		private void Awake()
		{
			InitializeSolidCollider();
			InitializeBushCollider();
		}

		private void InitializeSolidCollider()
		{
			var go = new GameObject("SolidChunkCollider");
			go.transform.SetParent(transform, false);
			solidCollider = go.AddComponent<SolidChunkCollider>();
		}

		private void InitializeBushCollider()
		{
			var go = new GameObject("BushChunkCollider");
			go.transform.SetParent(transform, false);
			bushCollider = go.AddComponent<World.Chunk.Controller.BushChunkCollider>();
		}

		public void Initialize(World.Chunk.Model.Coordinates coord, World.Chunk.Model.Data data)
		{
			Coordinates = coord;
			Rebuild(data);
		}

		public void Rebuild(World.Chunk.Model.Data data)
		{
			if (solidCollider != null)
			{
				solidCollider.Rebuild(data);
			}

			if (bushCollider != null)
			{
				bushCollider.Rebuild(data);
			}
		}
	}
}
