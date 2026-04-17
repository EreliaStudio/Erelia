using System;
using UnityEngine;

[Serializable]
public sealed class GameSaveData
{
	[SerializeField] private int worldSeed = 1;
	[SerializeField] private Vector3Int playerWorldCell = Vector3Int.zero;
	[SerializeField] private bool playerSpawnResolved = false;

	public int WorldSeed => worldSeed;
	public Vector3Int PlayerWorldCell => playerWorldCell;
	public bool PlayerSpawnResolved => playerSpawnResolved;

	public void SetResolvedSpawn(Vector3Int cell)
	{
		playerWorldCell = cell;
		playerSpawnResolved = true;
	}
}
