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

	public static ChunkCoordinates FromWorldVoxelPosition(Vector3Int worldPosition)
	{
		return new ChunkCoordinates(
			Mathf.FloorToInt((float)worldPosition.x / ChunkData.FixedSizeX),
			Mathf.FloorToInt((float)worldPosition.z / ChunkData.FixedSizeZ));
	}

	public static ChunkCoordinates FromWorldPosition(float worldX, float worldZ)
	{
		return new ChunkCoordinates(
			Mathf.FloorToInt(worldX / ChunkData.FixedSizeX),
			Mathf.FloorToInt(worldZ / ChunkData.FixedSizeZ));
	}

	public static Vector3Int ToLocalVoxelPosition(Vector3Int worldPosition)
	{
		return new Vector3Int(
			PositiveModulo(worldPosition.x, ChunkData.FixedSizeX),
			worldPosition.y,
			PositiveModulo(worldPosition.z, ChunkData.FixedSizeZ));
	}

	public Vector3Int ToWorldOrigin()
	{
		return new Vector3Int(
			X * ChunkData.FixedSizeX,
			0,
			Z * ChunkData.FixedSizeZ);
	}

	public Vector3Int ToWorldVoxelPosition(Vector3Int localPosition)
	{
		return ToWorldOrigin() + localPosition;
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

	private static int PositiveModulo(int value, int modulo)
	{
		int result = value % modulo;
		return result < 0 ? result + modulo : result;
	}
}
