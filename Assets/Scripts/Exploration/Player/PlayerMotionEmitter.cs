using UnityEngine;

namespace Erelia.Exploration.Player
{
	public sealed class PlayerMotionEmitter : MonoBehaviour
	{
		private Vector3Int lastCell;

		private void Awake()
		{
			Vector3 worldPosition = transform.position;
			lastCell = WorldToCell(worldPosition) - Vector3Int.one;
		}

		private void Update()
		{
			Vector3 worldPosition = transform.position;
			Vector3Int cell = WorldToCell(worldPosition);
			if (cell == lastCell)
			{
				return;
			}

			lastCell = cell;
			Erelia.Core.Event.Bus.Emit(new Erelia.Core.Event.PlayerMotion(worldPosition, cell));
		}

		private static Vector3Int WorldToCell(Vector3 worldPosition)
		{
			return new Vector3Int(
				Mathf.FloorToInt(worldPosition.x),
				Mathf.FloorToInt(worldPosition.y),
				Mathf.FloorToInt(worldPosition.z));
		}
	}
}
