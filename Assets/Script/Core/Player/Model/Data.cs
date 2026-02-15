using UnityEngine;

namespace Core.Player.Model
{
	public class Data
	{
		public Vector3Int CellPosition { get; private set; } = Vector3Int.zero;
		public Exploration.World.Chunk.Model.Coordinates ChunkCoordinates { get; private set; } = new Exploration.World.Chunk.Model.Coordinates(0, 0, 0);

		public Vector3 WorldPosition => new Vector3(CellPosition.x, CellPosition.y, CellPosition.z);

		public Core.Team.Data team = new Core.Team.Data();

		public void UpdateFromWorld(Vector3 worldPosition)
		{
			CellPosition = Vector3Int.FloorToInt(worldPosition);
			ChunkCoordinates = Exploration.World.Chunk.Model.Coordinates.FromWorld(worldPosition);
		}
	}
}
