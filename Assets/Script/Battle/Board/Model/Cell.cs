using System;
using System.Collections.Generic;

namespace Battle.Board.Model
{
	public class Cell
	{
		private readonly List<Battle.Board.Model.CellMask> masks = new List<Battle.Board.Model.CellMask>();
	    public IReadOnlyList<Battle.Board.Model.CellMask> Masks => masks;

		public Cell()
		{
			masks.Clear();
		}

		public bool HasMask(Battle.Board.Model.CellMask mask)
		{
			return masks.Contains(mask);
		}

		public void AddMask(Battle.Board.Model.CellMask mask)
		{
			if (masks.Contains(mask))
			{
				return;
			}

			masks.Add(mask);
		}

		public void RemoveMask(Battle.Board.Model.CellMask mask)
		{
			masks.Remove(mask);
		}

		public void ClearMasks()
		{
			masks.Clear();
		}
	}
}
