using UnityEngine;

namespace Core.Creature
{
	[CreateAssetMenu(menuName = "Creature/Definition", fileName = "CreatureDefinition")]
	public class Definition : ScriptableObject
	{
		[SerializeField] private Core.Creature.Species.Definition speciesDefinition = null;
		[SerializeField] private Core.Creature.Model.Data data = new Core.Creature.Model.Data();

		public Core.Creature.Species.Definition SpeciesDefinition => speciesDefinition;
		public Core.Creature.Model.Data Data => data;

		public string DisplayName
		{
			get
			{
				if (data != null && !string.IsNullOrWhiteSpace(data.NickName))
				{
					return data.NickName;
				}

				if (speciesDefinition != null && speciesDefinition.Data != null && !string.IsNullOrWhiteSpace(speciesDefinition.Data.FamilyName))
				{
					return speciesDefinition.Data.FamilyName;
				}

				return "Unknown";
			}
		}
	}
}
