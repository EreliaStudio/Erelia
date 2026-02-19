using System.Collections.Generic;

namespace Core.Mask.Model
{
	public class Cell
	{
		private readonly List<Core.Mask.Model.Value> masks = new List<Core.Mask.Model.Value>();
		public IReadOnlyList<Core.Mask.Model.Value> Masks => masks;

		public bool HasMask(Core.Mask.Model.Value mask)
		{
			return masks.Contains(mask);
		}

		public bool HasAnyMask()
		{
			return masks.Count > 0;
		}

		public void AddMask(Core.Mask.Model.Value mask)
		{
			if (masks.Contains(mask))
			{
				return;
			}

			masks.Add(mask);
		}

		public void RemoveMask(Core.Mask.Model.Value mask)
		{
			masks.Remove(mask);
		}

		public void ClearMasks()
		{
			masks.Clear();
		}
	}
}
