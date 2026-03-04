using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Creature
{
	/// <summary>
	/// Singleton registry mapping integer ids to <see cref="Species"/> assets.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This registry is a <see cref="Erelia.Core.SingletonRegistry{TRegistryType}"/> and is loaded from
	/// <c>Resources</c> at <c>Resources/Creature/SpeciesRegistry.asset</c>.
	/// </para>
	/// <para>
	/// It provides two lookup directions:
	/// </para>
	/// <list type="bullet">
	/// <item><description><c>id → species</c> via <see cref="TryGet"/>.</description></item>
	/// <item><description><c>species → id</c> via <see cref="TryGetId"/>.</description></item>
	/// </list>
	/// <para>
	/// The internal dictionaries are rebuilt from <see cref="Entries"/> when the asset is enabled/validated
	/// or when <see cref="Erelia.Core.SingletonRegistry{TRegistryType}.Instance"/> is loaded.
	/// </para>
	/// </remarks>
	[CreateAssetMenu(menuName = "Creature/Species Registry", fileName = "SpeciesRegistry")]
	public sealed class SpeciesRegistry : Erelia.Core.SingletonRegistry<SpeciesRegistry>
	{
		/// <summary>
		/// Reserved id value representing "no species".
		/// </summary>
		public const int EmptySpeciesId = -1;

		/// <summary>
		/// Resources-relative path (without extension) used by the singleton loader.
		/// </summary>
		protected override string ResourcePath => "Creature/SpeciesRegistry";

		/// <summary>
		/// Serialized entry binding an integer id to a <see cref="Species"/> asset.
		/// </summary>
		[Serializable]
		public struct Entry
		{
			/// <summary>
			/// Stable integer identifier for the species.
			/// </summary>
			public int Id;

			/// <summary>
			/// Species asset referenced by this entry.
			/// </summary>
			public Erelia.Core.Creature.Species Species;
		}

		/// <summary>
		/// List of serialized registry entries authored in the inspector.
		/// </summary>
		[SerializeField] private List<Entry> entries = new List<Entry>();

		/// <summary>
		/// Runtime lookup from id to species.
		/// </summary>
		[NonSerialized] private readonly Dictionary<int, Erelia.Core.Creature.Species> byId =
			new Dictionary<int, Erelia.Core.Creature.Species>();

		/// <summary>
		/// Runtime lookup from species to id.
		/// </summary>
		[NonSerialized] private readonly Dictionary<Erelia.Core.Creature.Species, int> bySpecies =
			new Dictionary<Erelia.Core.Creature.Species, int>();

		/// <summary>
		/// Gets the serialized entries of the registry.
		/// </summary>
		public IReadOnlyList<Entry> Entries => entries;

		/// <summary>
		/// Rebuilds runtime lookup dictionaries from the serialized <see cref="Entries"/>.
		/// </summary>
		/// <remarks>
		/// Null species are skipped. Duplicate ids are ignored and reported as warnings.
		/// </remarks>
		protected override void Rebuild()
		{
			// Reset runtime caches so they match the serialized list.
			byId.Clear();
			bySpecies.Clear();

			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];

				// Skip incomplete entries.
				if (entry.Species == null)
				{
					continue;
				}

				// Enforce unique ids. Keep the first occurrence and warn for duplicates.
				if (byId.ContainsKey(entry.Id))
				{
					Debug.LogWarning(
						$"[Erelia.Core.Creature.SpeciesRegistry] Duplicate id {entry.Id} for '{entry.Species.name}'. " +
						"Keeping the first occurrence.");
					continue;
				}

				// Populate both directions.
				byId.Add(entry.Id, entry.Species);

				// Species duplicates (same Species asset present multiple times) are also possible.
				// Keep the first occurrence and ignore later ones to keep the mapping deterministic.
				if (!bySpecies.ContainsKey(entry.Species))
				{
					bySpecies.Add(entry.Species, entry.Id);
				}
			}
		}

		/// <summary>
		/// Attempts to get a <see cref="Species"/> for the provided id.
		/// </summary>
		/// <param name="id">Species integer id.</param>
		/// <param name="species">Resolved species asset when found; otherwise <c>null</c>.</param>
		/// <returns><c>true</c> if a mapping exists; otherwise <c>false</c>.</returns>
		public bool TryGet(int id, out Erelia.Core.Creature.Species species)
		{
			return byId.TryGetValue(id, out species);
		}

		/// <summary>
		/// Attempts to get the integer id associated with a <see cref="Species"/> asset.
		/// </summary>
		/// <param name="species">Species asset to resolve.</param>
		/// <param name="id">Resolved id when found.</param>
		/// <returns><c>true</c> if a mapping exists; otherwise <c>false</c>.</returns>
		public bool TryGetId(Erelia.Core.Creature.Species species, out int id)
		{
			return bySpecies.TryGetValue(species, out id);
		}
	}
}