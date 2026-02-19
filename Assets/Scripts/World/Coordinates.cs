using System;
using UnityEngine;

namespace World
{
	[Serializable]
	public class ChunkCoordinates : IEquatable<ChunkCoordinates>
	{
		public int X;
		public int Z;

		public ChunkCoordinates()
		{
			X = 0;
			Z = 0;
		}

		public ChunkCoordinates(int x, int z)
		{
			X = x;
			Z = z;
		}

		public ChunkCoordinates(ChunkCoordinates other)
		{
			if (other == null)
			{
				X = 0;
				Z = 0;
				return;
			}

			X = other.X;
			Z = other.Z;
		}

		public static ChunkCoordinates FromWorld(Vector3 worldPosition)
		{
			int x = Mathf.FloorToInt(worldPosition.x / World.Chunk.SizeX);
			int z = Mathf.FloorToInt(worldPosition.z / World.Chunk.SizeZ);
			return new ChunkCoordinates(x, z);
		}

		public Vector2Int ToVector2Int()
		{
			return new Vector2Int(X, Z);
		}

		public static ChunkCoordinates FromVector2Int(Vector2Int value)
		{
			return new ChunkCoordinates(value.x, value.y);
		}

		public Vector3 WorldOrigin(float y = 0f)
		{
			return new Vector3(
				X * World.Chunk.SizeX,
				y,
				Z * World.Chunk.SizeZ
			);
		}

		public Vector3 WorldCenter(float y = 0f)
		{
			return WorldOrigin(y) + new Vector3(
				World.Chunk.SizeX * 0.5f,
				0f,
				World.Chunk.SizeZ * 0.5f
			);
		}

		public bool Equals(ChunkCoordinates other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return X == other.X && Z == other.Z;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ChunkCoordinates);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 31) + X;
				hash = (hash * 31) + Z;
				return hash;
			}
		}

		public static bool operator ==(ChunkCoordinates a, ChunkCoordinates b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}
			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
			{
				return false;
			}
			return a.X == b.X && a.Z == b.Z;
		}

		public static bool operator !=(ChunkCoordinates a, ChunkCoordinates b)
		{
			return !(a == b);
		}

		public ChunkCoordinates WithX(int x) => new ChunkCoordinates(x, Z);
		public ChunkCoordinates WithZ(int z) => new ChunkCoordinates(X, z);

		public ChunkCoordinates Offset(int dx, int dz)
		{
			return new ChunkCoordinates(X + dx, Z + dz);
		}

		public static ChunkCoordinates operator +(ChunkCoordinates a, ChunkCoordinates b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			if (b == null)
			{
				throw new ArgumentNullException(nameof(b));
			}
			return new ChunkCoordinates(a.X + b.X, a.Z + b.Z);
		}

		public static ChunkCoordinates operator -(ChunkCoordinates a, ChunkCoordinates b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			if (b == null)
			{
				throw new ArgumentNullException(nameof(b));
			}
			return new ChunkCoordinates(a.X - b.X, a.Z - b.Z);
		}

		public static ChunkCoordinates operator +(ChunkCoordinates a, Vector2Int b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new ChunkCoordinates(a.X + b.x, a.Z + b.y);
		}

		public static ChunkCoordinates operator -(ChunkCoordinates a, Vector2Int b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new ChunkCoordinates(a.X - b.x, a.Z - b.y);
		}

		public static ChunkCoordinates operator *(ChunkCoordinates a, int scalar)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new ChunkCoordinates(a.X * scalar, a.Z * scalar);
		}

		public override string ToString()
		{
			return "(" + X + ", " + Z + ")";
		}

		public static readonly ChunkCoordinates Zero = new ChunkCoordinates(0, 0);

		public ChunkCoordinates Copy()
		{
			return new ChunkCoordinates(X, Z);
		}

		public void Set(int x, int z)
		{
			X = x;
			Z = z;
		}
	}
}
