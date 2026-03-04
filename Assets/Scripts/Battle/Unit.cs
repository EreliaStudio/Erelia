using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Battle unit wrapper for a creature, its grid cell, and its spawned view.
	/// Keeps the creature, position, and view grouped for placement and battle logic.
	/// </summary>
	public sealed class Unit
	{
		/// <summary>
		/// Creature instance represented by this unit.
		/// </summary>
		public Erelia.Core.Creature.Instance.Model Creature { get; }
		/// <summary>
		/// Current grid cell occupied by the unit.
		/// </summary>
		public Vector3Int Cell { get; private set; }
		/// <summary>
		/// Spawned view GameObject associated with the unit.
		/// </summary>
		public GameObject View { get; }

		/// <summary>
		/// Creates a unit wrapper from a creature, its starting cell, and its view.
		/// </summary>
		public Unit(Erelia.Core.Creature.Instance.Model creature, Vector3Int cell, GameObject view)
		{
			// Store the unit data.
			Creature = creature;
			Cell = cell;
			View = view;
		}
	}
}
