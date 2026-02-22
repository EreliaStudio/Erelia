using UnityEngine;

namespace Erelia.Event
{
	public sealed class PlayerMotion : GenericEvent
	{
		public Vector3 WorldPosition { get; }
		public Vector3Int CellPosition { get; }

		public PlayerMotion(Vector3 worldPosition, Vector3Int cellPosition)
		{
			WorldPosition = worldPosition;
			CellPosition = cellPosition;
		}
	}
}
