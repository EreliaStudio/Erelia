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
	public Vector3Int PlayerWorldCell => player != null ? player.WorldCell : Vector3Int.zero;
	public Vector3Int RespawnPoint => respawnPoint;

	public void SetPlayerWorldCell(Vector3Int cell)
	{
		player ??= new PlayerData();
		player.WorldCell = cell;
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
