using UnityEngine;

namespace Battle.Context.Model
{
	public class CreatureInstance
	{
		public Core.Creature.Model.Definition Source { get; }
		public Side Side { get; }
		public int SlotIndex { get; }

		private bool hasPlacement;
		private Vector3Int cell;

		public bool HasPlacement => hasPlacement; 
		public Vector3Int Cell => cell;

		public CreatureInstance(Core.Creature.Model.Definition source, Side side, int slotIndex)
		{
			Source = source;
			Side = side;
			SlotIndex = slotIndex;
		}

		public void SetPlacement(Vector3Int cell)
		{
			this.cell = cell;
			hasPlacement = true;
		}

		public void ClearPlacement()
		{
			hasPlacement = false;
		}
	}
}
