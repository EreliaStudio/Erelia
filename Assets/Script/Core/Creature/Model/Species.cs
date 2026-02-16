using UnityEngine;

namespace Core.Creature.Model
{
	[CreateAssetMenu(menuName = "Creature/Species", fileName = "CreatureSpecies")]
	public class Species : ScriptableObject
	{
		[SerializeField] private string familyName = "DefaultName";
		[SerializeField] private Sprite sprite = null;

		public string FamilyName => familyName;
		public Sprite Sprite => sprite;
	}
}
