using UnityEngine;

namespace Erelia.Exploration.Player
{
	/// <summary>
	/// Emits a <see cref="Erelia.Core.Event.PlayerMotion"/> event when the player moves between cells.
	/// Tracks the player cell each frame and emits when the cell changes.
	/// </summary>
	public sealed class PlayerMotionEmitter : MonoBehaviour
	{
		/// <summary>
		/// Last known cell position.
		/// </summary>
		private Vector3Int lastCell;

		/// <summary>
		/// Unity callback invoked on object initialization.
		/// </summary>
		private void Awake()
		{
			// Initialize to a different cell so the first update triggers an event.
			Vector3 worldPosition = transform.position;
			lastCell = WorldToCell(worldPosition) - Vector3Int.one;
		}

		/// <summary>
		/// Unity update loop.
		/// </summary>
		private void Update()
		{
			// Compute current cell position from transform.
			Vector3 worldPosition = transform.position;
			Vector3Int cell = WorldToCell(worldPosition);
			if (cell == lastCell)
			{
				return;
			}

			// Update cache and emit motion event.
			lastCell = cell;
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.PlayerMotion(worldPosition, cell));
		}

		/// <summary>
		/// Converts a world position to integer cell coordinates.
		/// </summary>
		/// <param name="worldPosition">World-space position.</param>
		/// <returns>Cell-space coordinates.</returns>
		private static Vector3Int WorldToCell(Vector3 worldPosition)
		{
			return new Vector3Int(
				Mathf.FloorToInt(worldPosition.x),
				Mathf.FloorToInt(worldPosition.y),
				Mathf.FloorToInt(worldPosition.z));
		}
	}
}
