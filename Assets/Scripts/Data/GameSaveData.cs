using System;
using UnityEngine;

[Serializable]
public sealed class GameSaveData
{
	[SerializeField] private int worldSeed = 1;
	[SerializeField] private Vector3Int playerWorldCell = Vector3Int.zero;

	public int WorldSeed => worldSeed;
	public Vector3Int PlayerWorldCell => playerWorldCell;
	public Vector3 PlayerWorldPosition => playerWorldCell;
}
