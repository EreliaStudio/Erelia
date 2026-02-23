using System.Collections.Generic;
using UnityEngine;

namespace Erelia.BattleVoxel
{
	[System.Serializable]
	public class Cell
	{
		private List<Erelia.BattleVoxel.Type> masks = new List<Erelia.BattleVoxel.Type>();

		public IReadOnlyList<Erelia.BattleVoxel.Type> Masks => masks;

		public bool HasMask(Erelia.BattleVoxel.Type mask)
		{
			return masks.Contains(mask);
		}

		public bool HasAnyMask()
		{
			return masks.Count > 0;
		}

		public void AddMask(Erelia.BattleVoxel.Type mask)
		{
			if (masks.Contains(mask))
			{
				return;
			}

			masks.Add(mask);
		}

		public void RemoveMask(Erelia.BattleVoxel.Type mask)
		{
			masks.Remove(mask);
		}

		public void ClearMasks()
		{
			masks.Clear();
		}
	}
}
