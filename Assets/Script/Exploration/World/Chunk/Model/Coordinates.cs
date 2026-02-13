using System;
using UnityEngine;

namespace Exploration.World.Chunk.Model
{
	[Serializable]
	public class Coordinates : IEquatable<Coordinates>
	{
		public int X;
		public int Y;
		public int Z;

		public Coordinates()
		{
			X = 0;
			Y = 0;
			Z = 0;
		}

		public Coordinates(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Coordinates(Coordinates other)
		{
			if (other == null)
			{
				X = 0;
				Y = 0;
				Z = 0;
				return;
			}
			X = other.X;
			Y = other.Y;
			Z = other.Z;
		}

		public static Coordinates FromWorld(Vector3 worldPosition)
		{
			int x = Mathf.FloorToInt(worldPosition.x / Exploration.World.Chunk.Model.Data.SizeX);
			int y = Mathf.FloorToInt(worldPosition.y / Exploration.World.Chunk.Model.Data.SizeY);
			int z = Mathf.FloorToInt(worldPosition.z / Exploration.World.Chunk.Model.Data.SizeZ);
			return new Coordinates(x, y, z);
		}

		public Vector3Int ToVector3Int()
		{
			return new Vector3Int(X, Y, Z);
		}

		public static Coordinates FromVector3Int(Vector3Int value)
		{
			return new Coordinates(value.x, value.y, value.z);
		}

		public Vector3 WorldOrigin()
		{
			return new Vector3(
				X * Exploration.World.Chunk.Model.Data.SizeX,
				Y * Exploration.World.Chunk.Model.Data.SizeY,
				Z * Exploration.World.Chunk.Model.Data.SizeZ
			);
		}

		public Vector3 WorldCenter()
		{
			return WorldOrigin() + new Vector3(
				Exploration.World.Chunk.Model.Data.SizeX * 0.5f,
				Exploration.World.Chunk.Model.Data.SizeY * 0.5f,
				Exploration.World.Chunk.Model.Data.SizeZ * 0.5f
			);
		}

		public bool Equals(Coordinates other)
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
			return Equals(obj as Coordinates);
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

		public static bool operator ==(Coordinates a, Coordinates b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}
			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
			{
				return false;
			}
			return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
		}

		public static bool operator !=(Coordinates a, Coordinates b)
		{
			return !(a == b);
		}

		public Coordinates WithX(int x) => new Coordinates(x, Y, Z);
		public Coordinates WithY(int y) => new Coordinates(X, y, Z);
		public Coordinates WithZ(int z) => new Coordinates(X, Y, z);

		public Coordinates Offset(int dx, int dy, int dz)
		{
			return new Coordinates(X + dx, Y + dy, Z + dz);
		}

		public static Coordinates operator +(Coordinates a, Coordinates b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			if (b == null)
			{
				throw new ArgumentNullException(nameof(b));
			}
			return new Coordinates(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		public static Coordinates operator -(Coordinates a, Coordinates b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			if (b == null)
			{
				throw new ArgumentNullException(nameof(b));
			}
			return new Coordinates(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		public static Coordinates operator +(Coordinates a, Vector3Int b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new Coordinates(a.X + b.x, a.Y + b.y, a.Z + b.z);
		}

		public static Coordinates operator -(Coordinates a, Vector3Int b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new Coordinates(a.X - b.x, a.Y - b.y, a.Z - b.z);
		}

		public static Coordinates operator *(Coordinates a, int scalar)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new Coordinates(a.X * scalar, a.Y * scalar, a.Z * scalar);
		}

		public override string ToString()
		{
			return "(" + X + ", " + Y + ", " + Z + ")";
		}

		public static readonly Coordinates Zero = new Coordinates(0, 0, 0);

		public Coordinates Copy()
		{
			return new Coordinates(X, Y, Z);
		}

		public void Set(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}
