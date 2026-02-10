using UnityEngine;
using System;
using System.Collections.Generic;

namespace Player
{
	[Serializable]
	public class Service
	{			
		public event Action<World.Chunk.Model.Coordinates> PlayerChunkCoordinateChanged;
		private Vector3Int lastBushGrid = new Vector3Int(0, 0, 0);

		public void Init()
		{
			
		}

		public void NotifyChunkCoordinateChanged(World.Chunk.Model.Coordinates coord)
		{
			PlayerChunkCoordinateChanged?.Invoke(coord);
		}

		public void NotifyPlayerWalkingInBush(Vector3 worldPosition)
		{
			Vector3Int grid = Vector3Int.FloorToInt(worldPosition);
			if (grid == lastBushGrid)
			{
				return;
			}

			Debug.Log($"Player moved in bush from {lastBushGrid} to {grid}.");

			lastBushGrid = grid;
		}
	}
}
