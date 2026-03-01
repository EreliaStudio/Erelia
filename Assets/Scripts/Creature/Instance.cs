using UnityEngine;

namespace Creature
{
	[CreateAssetMenu(menuName = "Creature/Instance", fileName = "NewCreatureInstance")]
	public class Instance : ScriptableObject
	{
		[SerializeField] private Creature.Species species;
		[SerializeField] private string nickname;
		[SerializeField] private int level = 1;

		public Creature.Species Species => species;
		public string Nickname => nickname;
		public int Level => level;
	}
}
