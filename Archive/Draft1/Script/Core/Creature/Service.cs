using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Creature
{
	[Serializable]
	public class Service
	{
		private readonly Dictionary<string, Core.Creature.Species.Definition> data = new Dictionary<string, Core.Creature.Species.Definition>();
		public IReadOnlyDictionary<string, Core.Creature.Species.Definition> Data => data;

		public Service(List<Core.Creature.Species.Definition> speciesList)
		{
			RebuildDictionary(speciesList);
		}

		public bool TryGetSpecies(string familyName, out Core.Creature.Species.Definition species)
		{
			return data.TryGetValue(familyName, out species);
		}

		private void RebuildDictionary(List<Core.Creature.Species.Definition> speciesList)
		{
			data.Clear();

			if (speciesList == null)
			{
				return;
			}

			for (int i = 0; i < speciesList.Count; i++)
			{
				var species = speciesList[i];
				if (species == null)
				{
					Debug.LogError("Core.Creature.Service: species is null.");
					continue;
				}

				if (string.IsNullOrWhiteSpace(species.FamilyName))
				{
					Debug.LogError("Core.Creature.Service: species family name is empty.");
					continue;
				}

				if (data.ContainsKey(species.FamilyName))
				{
					Debug.LogError($"Core.Creature.Service: duplicate species name '{species.FamilyName}'.");
					continue;
				}

				data[species.FamilyName] = species;
			}
		}
	}
}
