using System;
using UnityEngine;

[Serializable]
public struct ChunkCoordinates : IEquatable<ChunkCoordinates>
{
	public int X;
	public int Z;

	public ChunkCoordinates(int x, int z)
	{
		X = x;
		Z = z;
	}

	public static ChunkCoordinates FromWorldPosition(Vector3 worldPosition)
	{
		return FromWorldPosition(worldPosition.x, worldPosition.z);
	}

	public static ChunkCoordinates FromWorldPosition(float worldX, float worldZ)
	{
		return new ChunkCoordinates(
			Mathf.FloorToInt(worldX / ChunkData.FixedSizeX),
			Mathf.FloorToInt(worldZ / ChunkData.FixedSizeZ));
	}

	public bool Equals(ChunkCoordinates other)
	{
		return X == other.X && Z == other.Z;
	}

	public override bool Equals(object obj)
	{
		return obj is ChunkCoordinates other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return (X * 397) ^ Z;
		}
	}

	public override string ToString()
	{
		return $"({X}, {Z})";
	}
}
