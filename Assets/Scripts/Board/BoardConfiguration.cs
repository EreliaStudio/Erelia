using System;
using UnityEngine;

[Serializable]
public sealed class BoardConfiguration
{
	public enum Shape
	{
		Square,
		Circle,
		RoundedSquare
	};

	[SerializeField] private Vector2Int size = new Vector2Int(9, 9);
	[SerializeField] public Shape shape = Shape.Square;

	public int SizeX => Mathf.Max(1, size.x);
	public int SizeY => ChunkData.FixedSizeY;
	public int SizeZ => Mathf.Max(1, size.y);
	public Vector3Int Size => new Vector3Int(SizeX, SizeY, SizeZ);
	public Vector3Int AnchorOffset => new Vector3Int(-SizeX / 2, 0, -SizeZ / 2);

	public Vector3Int GetSize()
	{
		return new Vector3Int(SizeX, SizeY, SizeZ);
	}

	public Vector3Int GetWorldOrigin(Vector3Int anchorWorldPosition)
	{
		return anchorWorldPosition + AnchorOffset;
	}
}
