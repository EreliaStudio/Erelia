using System.Collections.Generic;
using UnityEngine;

namespace Battle.Context.Model
{
	public class TeamPlacement
	{
		private readonly Core.Creature.Model.Team team;
		private readonly int maxPlacements;
		private readonly List<CreatureInstance> instances = new List<CreatureInstance>();

		public IReadOnlyList<CreatureInstance> Instances => instances;
		public int SlotCount => instances.Count;
		public int MaxPlacements => maxPlacements;
		public int PlacedCount => CountPlaced();

		public TeamPlacement(Core.Creature.Model.Team team, Side side, int maxPlacements)
		{
			this.team = team;
			this.maxPlacements = Mathf.Max(0, maxPlacements);

			int count = team != null ? team.Count : 0;
			for (int i = 0; i < count; i++)
			{
				instances.Add(new CreatureInstance(team.GetAt(i), side, i));
			}
		}

		public bool TryPlace(int slotIndex, Vector3Int cell)
		{
			if (!IsValidSlot(slotIndex))
			{
				return false;
			}

			if (!instances[slotIndex].HasPlacement && PlacedCount >= maxPlacements)
			{
				return false;
			}

			if (IsCellOccupied(cell, slotIndex))
			{
				return false;
			}

			instances[slotIndex].SetPlacement(cell);
			return true;
		}

		public bool TryClear(int slotIndex)
		{
			if (!IsValidSlot(slotIndex))
			{
				return false;
			}

			instances[slotIndex].ClearPlacement();
			return true;
		}

		public bool TryClearAtCell(Vector3Int cell, out int slotIndex)
		{
			slotIndex = -1;

			for (int i = 0; i < instances.Count; i++)
			{
				if (!instances[i].HasPlacement)
				{
					continue;
				}

				if (instances[i].Cell != cell)
				{
					continue;
				}

				instances[i].ClearPlacement();
				slotIndex = i;
				return true;
			}

			return false;
		}

		public bool TryGetPlacement(int slotIndex, out Vector3Int cell)
		{
			cell = default;

			if (!IsValidSlot(slotIndex))
			{
				return false;
			}

			if (!instances[slotIndex].HasPlacement)
			{
				return false;
			}

			cell = instances[slotIndex].Cell;
			return true;
		}

		public bool IsCellOccupied(Vector3Int cell, int ignoreSlotIndex = -1)
		{
			for (int i = 0; i < instances.Count; i++)
			{
				if (i == ignoreSlotIndex)
				{
					continue;
				}

				if (!instances[i].HasPlacement)
				{
					continue;
				}

				if (instances[i].Cell == cell)
				{
					return true;
				}
			}

			return false;
		}

		private bool IsValidSlot(int slotIndex)
		{
			return slotIndex >= 0 && slotIndex < instances.Count;
		}

		private int CountPlaced()
		{
			int count = 0;
			for (int i = 0; i < instances.Count; i++)
			{
				if (instances[i].HasPlacement)
				{
					count++;
				}
			}

			return count;
		}
	}
}
