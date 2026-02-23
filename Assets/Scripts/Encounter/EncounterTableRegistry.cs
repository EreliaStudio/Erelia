using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Encounter
{
	[CreateAssetMenu(menuName = "Encounter/EncounterTableRegistry", fileName = "EncounterTableRegistry")]
	public sealed class EncounterTableRegistry : Erelia.SingletonRegistry<EncounterTableRegistry>
	{
		protected override string ResourcePath => "EncounterTableRegistry";

		[Serializable]
		public struct Entry
		{
			public int Id;
			public EncounterTable Table;
		}

		[SerializeField] private List<Entry> entries = new List<Entry>();

		[NonSerialized] private Dictionary<int, EncounterTable> registry;

		public IReadOnlyList<Entry> Entries => entries;

		protected override void Rebuild()
		{
			registry = new Dictionary<int, EncounterTable>();
			HashSet<int> seen = new HashSet<int>();

			for (int i = 0; i < entries.Count; i++)
			{
				Entry entry = entries[i];
				if (entry.Table == null)
				{
					continue;
				}

				if (!seen.Add(entry.Id))
				{
					Debug.LogError($"EncounterTableRegistry '{name}' has a duplicate id '{entry.Id}'.");
					continue;
				}

				registry[entry.Id] = entry.Table;
			}
		}

		public bool TryGet(int id, out EncounterTable table)
		{
			if (registry == null)
			{
				Rebuild();
			}

			return registry.TryGetValue(id, out table);
		}
	}
}