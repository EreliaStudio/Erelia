using UnityEngine;

namespace Creature
{
	[CreateAssetMenu(menuName = "Creature/Team", fileName = "NewTeam")]
	public sealed class Team : ScriptableObject
	{
		[SerializeField] private Creature.Instance[] slots = new Creature.Instance[6];

		public Creature.Instance[] Slots => slots;
	}
}
