using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Exploration.World
{
	/// <summary>
	/// Registry mapping biome types to biome data assets.
	/// Loads encounter tables, builds a biome lookup from entries, and serves runtime queries.
	/// </summary>
	[CreateAssetMenu(menuName = "World/BiomeRegistry", fileName = "BiomeRegistry")]
	public sealed class BiomeRegistry : Erelia.Core.SingletonRegistry<BiomeRegistry>
	{
		/// <summary>
		/// Resources path (without extension) for the registry asset.
		/// </summary>
		protected override string ResourcePath => "Biome/BiomeRegistry";

		/// <summary>
		/// Entry binding a biome type to its data.
		/// </summary>
		[Serializable]
		public struct Entry
		{
			/// <summary>
			/// Biome type key.
			/// </summary>
			public BiomeType Type;

			/// <summary>
			/// Biome data for this type.
			/// </summary>
			public BiomeData Data;
		}

		/// <summary>
		/// Serialized biome entries authored in the inspector.
		/// </summary>
		[SerializeField] private List<Entry> entries = new List<Entry>();

		/// <summary>
		/// Optional JSON asset containing the encounter registry.
		/// </summary>
		[SerializeField] private TextAsset encounterRegistryJson;

		/// <summary>
		/// Resources path for the encounter registry JSON (used when no asset is provided).
		/// </summary>
		[SerializeField] private string encounterRegistryResourcePath = "Encounter/EncounterRegistry";

		/// <summary>
		/// Runtime lookup from biome type to data.
		/// </summary>
		[NonSerialized] private readonly Dictionary<BiomeType, BiomeData> biomes =
			new Dictionary<BiomeType, BiomeData>();

		/// <summary>
		/// Gets the runtime biome lookup dictionary.
		/// </summary>
		public IReadOnlyDictionary<BiomeType, BiomeData> Biomes => biomes;

		/// <summary>
		/// Gets the serialized entry list.
		/// </summary>
		public IReadOnlyList<Entry> Entries => entries;

		/// <summary>
		/// Rebuilds runtime data from serialized entries.
		/// </summary>
		protected override void Rebuild()
		{
			// Reset runtime cache.
			biomes.Clear();
			// Ensure encounter registry is loaded.
			if (encounterRegistryJson != null)
			{
				Erelia.Core.Encounter.EncounterTableRegistry.LoadFromJson(encounterRegistryJson.text);
			}
			else
			{
				Erelia.Core.Encounter.EncounterTableRegistry.LoadFromResources(encounterRegistryResourcePath);
			}

			// Populate the biome lookup.
			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];
				if (entry.Data == null)
				{
					continue;
				}

				biomes[entry.Type] = entry.Data;
			}
		}

		/// <summary>
		/// Registers (or overrides) a biome entry at runtime.
		/// </summary>
		/// <param name="type">Biome type key.</param>
		/// <param name="data">Biome data.</param>
		public void Register(BiomeType type, BiomeData data)
		{
			// Ignore null data.
			if (data == null)
			{
				return;
			}

			// Add or overwrite mapping.
			biomes[type] = data;
		}

		/// <summary>
		/// Attempts to resolve a biome data entry.
		/// </summary>
		/// <param name="type">Biome type key.</param>
		/// <param name="data">Resolved data if found.</param>
		/// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
		public bool TryGet(BiomeType type, out BiomeData data)
		{
			// Lookup biome data by type.
			return biomes.TryGetValue(type, out data);
		}
	}
}
