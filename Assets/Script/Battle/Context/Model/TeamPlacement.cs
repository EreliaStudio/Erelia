using System.Collections.Generic;
using UnityEngine;

namespace Battle.Context.Model
{
	public class TeamPlacement
	{
		private readonly Core.Creature.Model.Team team;
		private readonly List<CreatureInstance> instances = new List<CreatureInstance>();

		public IReadOnlyList<CreatureInstance> Instances => instances;
		public int SlotCount => instances.Count;

		public TeamPlacement(Core.Creature.Model.Team team, Side side)
		{
			this.team = team;

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
	}
}
