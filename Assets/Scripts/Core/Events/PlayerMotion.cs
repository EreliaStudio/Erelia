using UnityEngine;

namespace Erelia.Core.Event
{
	/// <summary>
	/// Event emitted when the player moves.
	/// </summary>
	/// <remarks>
	/// Includes both world-space and cell-space positions at the time of emission.
	/// </remarks>
	public sealed class PlayerMotion : GenericEvent
	{
		/// <summary>
		/// Player position in world coordinates.
		/// </summary>
		public Vector3 WorldPosition { get; }

		/// <summary>
		/// Player position in cell coordinates.
		/// </summary>
		public Vector3Int CellPosition { get; }

		/// <summary>
		/// Creates a new player motion event.
		/// </summary>
		/// <param name="worldPosition">Player world position.</param>
		/// <param name="cellPosition">Player cell position.</param>
		public PlayerMotion(Vector3 worldPosition, Vector3Int cellPosition)
		{
			WorldPosition = worldPosition;
			CellPosition = cellPosition;
		}
	}
}
