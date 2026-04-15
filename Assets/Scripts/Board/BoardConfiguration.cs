using System;
using UnityEngine;

[Serializable]
public sealed class BoardConfiguration
{
	[SerializeField] private Vector3Int size = new Vector3Int(9, ChunkData.FixedSizeY, 9);
	[SerializeField] private Vector3Int anchorOffset = new Vector3Int(-4, 0, -4);

	public int SizeX => Mathf.Max(1, size.x);
	public int SizeY => ChunkData.FixedSizeY;
	public int SizeZ => Mathf.Max(1, size.z);
	public Vector3Int Size => size;
	public Vector3Int AnchorOffset => anchorOffset;

	public Vector3Int GetSize()
	{
		return new Vector3Int(SizeX, SizeY, SizeZ);
	}

	public Vector3Int GetWorldOrigin(Vector3Int anchorWorldPosition)
	{
		return anchorWorldPosition + anchorOffset;
	}
}
