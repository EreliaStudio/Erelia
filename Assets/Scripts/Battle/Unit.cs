using UnityEngine;

namespace Erelia.Battle
{
	public sealed class Unit
	{
		public Creature.Instance Creature { get; }
		public Vector3Int Cell { get; private set; }
		public GameObject View { get; }

		public Unit(Creature.Instance creature, Vector3Int cell, GameObject view)
		{
			Creature = creature;
			Cell = cell;
			View = view;
		}
	}
}
