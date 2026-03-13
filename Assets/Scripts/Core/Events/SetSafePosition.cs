using UnityEngine;

namespace Erelia.Core.Event
{
	/// <summary>
	/// Event emitted to update the exploration safe return position.
	/// </summary>
	public sealed class SetSafePosition : GenericEvent
	{
		public Vector3 WorldPosition { get; }

		public SetSafePosition(Vector3 worldPosition)
		{
			WorldPosition = worldPosition;
		}
	}
}
