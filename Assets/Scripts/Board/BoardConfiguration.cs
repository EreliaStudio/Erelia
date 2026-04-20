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

	public enum PlacementStyle
	{
		HalfBoard
	}

	[SerializeField] private Vector2Int size = new Vector2Int(9, 9);
	[SerializeField] public Shape shape = Shape.Square;
	[SerializeField] private PlacementStyle placementStyle = PlacementStyle.HalfBoard;

	public BoardConfiguration()
	{
	}

	public BoardConfiguration(Vector2Int size, Shape shape)
	{
		this.size = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
		this.shape = shape;
	}

	public int SizeX => Mathf.Max(1, size.x);
	public int SizeY => ChunkData.FixedSizeY;
	public int SizeZ => Mathf.Max(1, size.y);
	public Vector3Int AnchorOffset => new Vector3Int(-SizeX / 2, 0, -SizeZ / 2);
	public PlacementStyle UnitPlacementStyle => placementStyle;

	public Vector3Int GetSize()
	{
		return new Vector3Int(SizeX, SizeY, SizeZ);
	}

	public Vector3Int GetWorldOrigin(Vector3Int anchorWorldPosition)
	{
		return anchorWorldPosition + AnchorOffset;
	}


}
