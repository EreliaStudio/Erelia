using System;
using UnityEngine;

[Serializable]
public class PlayerData : ActorData
{
	[SerializeField] private Vector3Int worldCell = Vector3Int.zero;

	public Vector3Int WorldCell
	{
		get => worldCell;
		set => worldCell = value;
	}

	public Vector3 WorldPosition => worldCell;
}
