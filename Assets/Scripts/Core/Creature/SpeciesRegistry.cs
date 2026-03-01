using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Core.Creature
{
	[CreateAssetMenu(menuName = "Creature/Species Registry", fileName = "SpeciesRegistry")]
	public sealed class SpeciesRegistry : Erelia.Core.SingletonRegistry<SpeciesRegistry>
	{
		public const int EmptySpeciesId = -1;

		protected override string ResourcePath => "Creature/SpeciesRegistry";

		[Serializable]
		public struct Entry
		{
			public int Id;
			public Erelia.Core.Creature.Species Species;
		}

		[SerializeField] private List<Entry> entries = new List<Entry>();

		[NonSerialized] private readonly Dictionary<int, Erelia.Core.Creature.Species> byId =
			new Dictionary<int, Erelia.Core.Creature.Species>();
		[NonSerialized] private readonly Dictionary<Erelia.Core.Creature.Species, int> bySpecies =
			new Dictionary<Erelia.Core.Creature.Species, int>();

		public IReadOnlyList<Entry> Entries => entries;

		protected override void Rebuild()
		{
			byId.Clear();
			bySpecies.Clear();

			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];
				if (entry.Species == null)
				{
					continue;
				}

				if (byId.ContainsKey(entry.Id))
				{
					Debug.LogWarning($"[Erelia.Core.Creature.SpeciesRegistry] Duplicate id {entry.Id} for {entry.Species.name}.");
					continue;
				}

				byId.Add(entry.Id, entry.Species);
				bySpecies.Add(entry.Species, entry.Id);
			}
		}

		public bool TryGet(int id, out Erelia.Core.Creature.Species species)
		{
			return byId.TryGetValue(id, out species);
		}

		public bool TryGetId(Erelia.Core.Creature.Species species, out int id)
		{
			return bySpecies.TryGetValue(species, out id);
		}
	}
}
