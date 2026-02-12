using System.Collections.Generic;

namespace Mask.Model
{
	public class Cell
	{
		private readonly List<Mask.Model.Value> masks = new List<Mask.Model.Value>();
		public IReadOnlyList<Mask.Model.Value> Masks => masks;

		public bool HasMask(Mask.Model.Value mask)
		{
			return masks.Contains(mask);
		}

		public bool HasAnyMask()
		{
			return masks.Count > 0;
		}

		public void AddMask(Mask.Model.Value mask)
		{
			if (masks.Contains(mask))
			{
				return;
			}

			masks.Add(mask);
		}

		public void RemoveMask(Mask.Model.Value mask)
		{
			masks.Remove(mask);
		}

		public void ClearMasks()
		{
			masks.Clear();
		}
	}
}
