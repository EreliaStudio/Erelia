using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
	[SerializeField] private Vector3Int cellPosition;
	[SerializeField] private ChunkCoordinates chunkCoordinates;

	public Vector3Int CellPosition
	{
		get => cellPosition;
		set => cellPosition = value;
	}

	public ChunkCoordinates ChunkCoordinates
	{
		get => chunkCoordinates;
		set => chunkCoordinates = value;
	}
}
