using UnityEngine;

namespace Core.Creature.Model
{
	[CreateAssetMenu(menuName = "Creature/Species", fileName = "CreatureSpecies")]
	public class Species : ScriptableObject
	{
		[SerializeField] private string familyName = "DefaultName";

		public string FamilyName => familyName;
	}
}
