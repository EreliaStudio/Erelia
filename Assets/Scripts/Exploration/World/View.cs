using UnityEngine;

namespace Erelia.Exploration.World
{
	/// <summary>
	/// View component responsible for creating chunk views and defining view radius.
	/// Instantiates chunk view prefabs at chunk origins and exposes view target/radius.
	/// </summary>
	public sealed class View : MonoBehaviour
	{
		/// <summary>
		/// Prefab used to instantiate chunk views.
		/// </summary>
		[SerializeField] private Erelia.Exploration.World.Chunk.View chunkViewPrefab;

		/// <summary>
		/// Optional parent transform for spawned chunk views.
		/// </summary>
		[SerializeField] private Transform chunkRoot;

		/// <summary>
		/// Transform used as the view center (typically the player).
		/// </summary>
		[SerializeField] private Transform viewTarget;

		/// <summary>
		/// View radius in chunks.
		/// </summary>
		[SerializeField] private int viewRadius = 6;

		/// <summary>
		/// Gets the view target transform.
		/// </summary>
		public Transform ViewTarget => viewTarget;

		/// <summary>
		/// Gets the view radius in chunks.
		/// </summary>
		public int ViewRadius => viewRadius;

		/// <summary>
		/// Instantiates a chunk view and positions it at the chunk origin.
		/// </summary>
		/// <param name="coordinates">Chunk coordinates.</param>
		/// <returns>Instantiated chunk view, or <c>null</c> if prefab is missing.</returns>
		public Erelia.Exploration.World.Chunk.View CreateChunkView(Erelia.Exploration.World.Chunk.Coordinates coordinates)
		{
			// Ensure prefab is assigned.
			if (chunkViewPrefab == null)
			{
				return null;
			}

			// Choose parent and instantiate the view.
			Transform parent = chunkRoot != null ? chunkRoot : transform;
			Erelia.Exploration.World.Chunk.View view = Instantiate(chunkViewPrefab, parent);
			if (coordinates != null)
			{
				// Place view at chunk origin and name it.
				view.transform.position = coordinates.WorldOrigin();
				view.gameObject.name = $"ChunkView {coordinates}";
			}
			else
			{
				// Fallback name when coordinates are unavailable.
				view.gameObject.name = "ChunkView (unknown)";
			}

			return view;
		}
	}
}


