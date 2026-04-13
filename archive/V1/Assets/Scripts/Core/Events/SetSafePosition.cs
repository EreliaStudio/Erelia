using UnityEngine;

namespace Erelia.Core.Event
{
	public sealed class SetSafePosition : GenericEvent
	{
		public Vector3 WorldPosition { get; }

		public SetSafePosition(Vector3 worldPosition)
		{
			WorldPosition = worldPosition;
		}
	}
}
