using UnityEngine;

namespace Erelia.Battle.Unit
{
	public readonly struct Snapshot
	{
		public Snapshot(
			Sprite icon,
			string displayName,
			bool isPlaced,
			Erelia.Battle.Side side,
			Vector3Int cell)
		{
			Icon = icon;
			DisplayName = displayName;
			IsPlaced = isPlaced;
			Side = side;
			Cell = cell;
		}

		public Sprite Icon { get; }
		public string DisplayName { get; }
		public bool IsPlaced { get; }
		public Erelia.Battle.Side Side { get; }
		public Vector3Int Cell { get; }
	}
}
