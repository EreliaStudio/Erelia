using UnityEngine;

namespace Erelia.Battle
{
	public sealed class Unit
	{
		public Erelia.Core.Creature.Instance.Model Creature { get; }
		public Vector3Int Cell { get; private set; }
		public GameObject View { get; }

		public Unit(Erelia.Core.Creature.Instance.Model creature, Vector3Int cell, GameObject view)
		{
			Creature = creature;
			Cell = cell;
			View = view;
		}
	}
}
