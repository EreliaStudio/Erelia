using UnityEngine;

namespace Erelia.Core.Creature
{
	/// <summary>
	/// Defines a creature species as a reusable data asset.
	/// </summary>
	/// <remarks>
	/// A <see cref="Species"/> asset stores the base, species-wide information used when spawning or
	/// instantiating creatures (e.g. which prefab to instantiate and the baseline stats to start from).
	/// Instances of creatures in the world/battle typically reference a <see cref="Species"/> and then
	/// apply per-instance progression
	/// </remarks>
	[CreateAssetMenu(menuName = "Creature/Species", fileName = "NewSpecies")]
	public sealed class Species : ScriptableObject
	{
		/// <summary>
		/// Sprite displayed by UI elements representing this species.
		/// </summary>
		[SerializeField] private Sprite icon;

		/// <summary>
		/// Prefab used to instantiate the creature representation in the scene.
		/// </summary>
		/// <remarks>
		/// Usually contains the visuals (renderers/animator) and any required components for runtime behavior.
		/// </remarks>
		[SerializeField] private GameObject prefab;

		/// <summary>
		/// Human-readable name displayed to the player.
		/// </summary>
		[SerializeField] private string displayName;

		/// <summary>
		/// Base stats shared by every instance of this species.
		/// </summary>
		[SerializeField] private Erelia.Core.Stats.Values baseStats;

		/// <summary>
		/// Gets the sprite displayed by UI elements representing this species.
		/// </summary>
		public Sprite Icon => icon;

		/// <summary>
		/// Gets the prefab used to instantiate this species.
		/// </summary>
		public GameObject Prefab => prefab;

		/// <summary>
		/// Gets the display name of the species.
		/// </summary>
		public string DisplayName => displayName;

		/// <summary>
		/// Gets the base stats shared by this species.
		/// </summary>
		public Erelia.Core.Stats.Values BaseStats => baseStats;
	}
}
