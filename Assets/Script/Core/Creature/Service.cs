using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Creature
{
	public class Service
	{
		[Serializable]
		public struct Entry
		{
			public int Id;
			public Core.Creature.Model.Data data;
		}

		private readonly Dictionary<int, Core.Creature.Model.Data> creatures = new Dictionary<int, Core.Creature.Model.Data>();
		public IReadOnlyDictionary<int, Core.Creature.Model.Data> Creatures => creatures;

		public Service(List<Entry> entries)
		{
			RebuildDictionary(entries);
		}

		public bool TryGetDefinition(int id, out Core.Creature.Model.Data data)
		{
			if (creatures.TryGetValue(id, out data) == false)
			{
				return false;
			}
			return true;
		}

		private void RebuildDictionary(List<Entry> entries)
		{
			creatures.Clear();

			if (entries == null)
			{
				return ;
			}

			for (int i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];

				creatures[entry.Id] = entry.data;
			}
		}
	}
}