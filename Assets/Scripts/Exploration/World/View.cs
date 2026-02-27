using UnityEngine;

namespace Erelia.Exploration.World
{
	public sealed class View : MonoBehaviour
	{
		[SerializeField] private Erelia.Exploration.World.Chunk.View chunkViewPrefab;
		[SerializeField] private Transform chunkRoot;
		[SerializeField] private Transform viewTarget;
		[SerializeField] private int viewRadius = 6;

		public Transform ViewTarget => viewTarget;
		public int ViewRadius => viewRadius;

		public Erelia.Exploration.World.Chunk.View CreateChunkView(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			if (chunkViewPrefab == null)
			{
				return null;
			}

			Transform parent = chunkRoot != null ? chunkRoot : transform;
			Erelia.Exploration.World.Chunk.View view = Instantiate(chunkViewPrefab, parent);
			if (coordinates != null)
			{
				view.transform.position = coordinates.WorldOrigin();
				view.gameObject.name = $"ChunkView {coordinates}";
			}
			else
			{
				view.gameObject.name = "ChunkView (unknown)";
			}

			return view;
		}
	}
}


