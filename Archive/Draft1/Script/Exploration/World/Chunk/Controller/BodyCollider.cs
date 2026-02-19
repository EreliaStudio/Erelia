using UnityEngine;

namespace Exploration.World.Chunk.Controller
{
	public class BodyCollider : MonoBehaviour
	{
		[SerializeField] private Exploration.World.Chunk.Controller.SolidChunkCollider solidCollider = null;
		[SerializeField] private Exploration.World.Chunk.Controller.BushChunkCollider bushCollider = null;

		public Exploration.World.Chunk.Model.Coordinates Coordinates { get; private set; }

		private void Awake()
		{
			InitializeSolidCollider();
			InitializeBushCollider();
		}

		private void InitializeSolidCollider()
		{
			var go = new GameObject("SolidChunkCollider");
			go.transform.SetParent(transform, false);
			solidCollider = go.AddComponent<Exploration.World.Chunk.Controller.SolidChunkCollider>();
		}

		private void InitializeBushCollider()
		{
			var go = new GameObject("BushChunkCollider");
			go.transform.SetParent(transform, false);
			bushCollider = go.AddComponent<Exploration.World.Chunk.Controller.BushChunkCollider>();
		}

		public void Initialize(Exploration.World.Chunk.Model.Coordinates coord, Exploration.World.Chunk.Model.Data data)
		{
			Coordinates = coord;
			Rebuild(data);
		}

		public void Rebuild(Exploration.World.Chunk.Model.Data data)
		{
			solidCollider.Rebuild(data);
			bushCollider.Rebuild(data);
		}
	}
}
