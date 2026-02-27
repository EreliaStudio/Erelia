using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.World
{
	[CreateAssetMenu(menuName = "World/BiomeRegistry", fileName = "BiomeRegistry")]
	public sealed class BiomeRegistry : Erelia.SingletonRegistry<BiomeRegistry>
	{
		protected override string ResourcePath => "Biome/BiomeRegistry";

		[Serializable]
		public struct Entry
		{
			public BiomeType Type;
			public BiomeData Data;
		}

		[SerializeField] private List<Entry> entries = new List<Entry>();

		[NonSerialized] private readonly Dictionary<BiomeType, BiomeData> biomes =
			new Dictionary<BiomeType, BiomeData>();

		public IReadOnlyDictionary<BiomeType, BiomeData> Biomes => biomes;
		public IReadOnlyList<Entry> Entries => entries;

		protected override void Rebuild()
		{
			biomes.Clear();
			Erelia.EncounterTableRegistry.Clear();

			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];
				if (entry.Data == null)
				{
					continue;
				}

				biomes[entry.Type] = entry.Data;
				if (entry.Data.EncounterTable != null)
				{
					Erelia.EncounterTableRegistry.Register(entry.Data.EncounterTable);
				}
			}
		}

		public void Register(BiomeType type, BiomeData data)
		{
			if (data == null)
			{
				return;
			}

			biomes[type] = data;
		}

		public bool TryGet(BiomeType type, out BiomeData data)
		{
			return biomes.TryGetValue(type, out data);
		}
	}
}
