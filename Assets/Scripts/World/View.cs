using UnityEngine;

namespace Erelia.World
{
	public sealed class View : MonoBehaviour
	{
		[SerializeField] private Erelia.World.Chunk.View chunkViewPrefab;
		[SerializeField] private Transform chunkRoot;
		[SerializeField] private Transform viewTarget;
		[SerializeField] private int viewRadius = 6;

		public Transform ViewTarget => viewTarget;
		public int ViewRadius => viewRadius;

		public Erelia.World.Chunk.View CreateChunkView(Erelia.World.Chunk.Coordinates coordinates)
		{
			if (chunkViewPrefab == null)
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.View] Chunk view prefab is not assigned.");
				return null;
			}

			Transform parent = chunkRoot != null ? chunkRoot : transform;
			Erelia.World.Chunk.View view = Instantiate(chunkViewPrefab, parent);
			if (coordinates != null)
			{
				view.transform.position = coordinates.WorldOrigin();
			}
			else
			{
				Erelia.Logger.RaiseWarning("[Erelia.World.View] Coordinates were null. Chunk view positioned at prefab default.");
			}

			Erelia.Logger.Log("[Erelia.World.View] Created chunk view" + (coordinates != null ? (" for " + coordinates) : "."));
			return view;
		}
	}
}


