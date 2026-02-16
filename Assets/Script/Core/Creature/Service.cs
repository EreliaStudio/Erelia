using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Creature
{
	[Serializable]
	public class Service
	{
		private readonly Dictionary<string, Core.Creature.Model.Species> data = new Dictionary<string, Core.Creature.Model.Species>();
		public IReadOnlyDictionary<string, Core.Creature.Model.Species> Data => data;

		public Service(List<Core.Creature.Model.Species> speciesList)
		{
			RebuildDictionary(speciesList);
		}

		public bool TryGetSpecies(string familyName, out Core.Creature.Model.Species species)
		{
			return data.TryGetValue(familyName, out species);
		}

		private void RebuildDictionary(List<Core.Creature.Model.Species> speciesList)
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
