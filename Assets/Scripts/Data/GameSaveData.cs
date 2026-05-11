using System;
using UnityEngine;

[Serializable]
public sealed class GameSaveData
{
	[SerializeField] private int worldSeed = 1;
	[SerializeField] private PlayerData player = new PlayerData();
	[SerializeField] private Vector3Int respawnPoint = Vector3Int.zero;

	public int WorldSeed => worldSeed;
	public PlayerData Player => player;
	public Vector3Int PlayerWorldCell => player != null ? Vector3Int.FloorToInt(player.Position.Value) : Vector3Int.zero;
	public Vector3Int RespawnPoint => respawnPoint;

	public void SetWorldSeed(int seed)
	{
		worldSeed = seed;
	}

	public void SetPlayerWorldCell(Vector3Int cell)
	{
		player ??= new PlayerData();
		player.SetPosition(new Vector3(cell.x + 0.5f, cell.y, cell.z + 0.5f), true);
	}

	public void SetRespawnPoint(Vector3Int cell)
	{
		respawnPoint = cell;
	}

	public void CopyPlayerFrom(PlayerData source)
	{
		player ??= new PlayerData();
		player.CopyFrom(source);
	}
}
