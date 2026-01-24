using System;
using UnityEngine;

[Serializable]
public class ChunkCoord : IEquatable<ChunkCoord>
{
    public int X;
    public int Y;
    public int Z;

    public ChunkCoord(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static ChunkCoord FromWorld(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / Chunk.SizeX);
        int y = Mathf.FloorToInt(worldPosition.y / Chunk.SizeY);
        int z = Mathf.FloorToInt(worldPosition.z / Chunk.SizeZ);
        return new ChunkCoord(x, y, z);
    }

    public bool Equals(ChunkCoord other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ChunkCoord);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + X;
            hash = (hash * 31) + Y;
            hash = (hash * 31) + Z;
            return hash;
        }
    }

    public override string ToString()
    {
        return "(" + X + ", " + Y + ", " + Z + ")";
    }
}
