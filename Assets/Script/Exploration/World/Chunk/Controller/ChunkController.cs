using UnityEngine;

namespace World.Controller
{
	public class ChunkController : MonoBehaviour
	{
		[SerializeField] private SolidChunkCollider solidCollider = null;
		[SerializeField] private Utils.Mesher.BushChunkCollider bushCollider = null;

		public World.Chunk.Model.Coordinates Coordinates { get; private set; }

		private void Awake()
		{
			CacheComponents();
		}

		private void Reset()
		{
			CacheComponents();
		}

		private void CacheComponents()
		{
			if (solidCollider == null)
			{
				solidCollider = GetComponentInChildren<SolidChunkCollider>();
				if (solidCollider == null)
				{
					var go = new GameObject("SolidChunkCollider");
					go.transform.SetParent(transform, false);
					solidCollider = go.AddComponent<SolidChunkCollider>();
				}
			}

			if (bushCollider == null)
			{
				bushCollider = GetComponentInChildren<Utils.Mesher.BushChunkCollider>();
				if (bushCollider == null)
				{
					var go = new GameObject("BushChunkCollider");
					go.transform.SetParent(transform, false);
					bushCollider = go.AddComponent<Utils.Mesher.BushChunkCollider>();
				}
			}
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
