using System;
using UnityEngine;

namespace Erelia.World.Chunk
{
	[Serializable]
	public class Coordinates : IEquatable<Coordinates>
	{
		public int X;
		public int Z;

		public Coordinates()
		{
			X = 0;
			Z = 0;
		}

		public Coordinates(int x, int z)
		{
			X = x;
			Z = z;
		}

		public Coordinates(Coordinates other)
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

		public static Coordinates FromWorld(Vector3 worldPosition)
		{
			int x = Mathf.FloorToInt(worldPosition.x / Erelia.World.Chunk.Model.SizeX);
			int z = Mathf.FloorToInt(worldPosition.z / Erelia.World.Chunk.Model.SizeZ);
			return new Coordinates(x, z);
		}

		public Vector2Int ToVector2Int()
		{
			return new Vector2Int(X, Z);
		}

		public static Coordinates FromVector2Int(Vector2Int value)
		{
			return new Coordinates(value.x, value.y);
		}

		public Vector3 WorldOrigin(float y = 0f)
		{
			return new Vector3(
				X * Erelia.World.Chunk.Model.SizeX,
				y,
				Z * Erelia.World.Chunk.Model.SizeZ
			);
		}

		public Vector3 WorldCenter(float y = 0f)
		{
			return WorldOrigin(y) + new Vector3(
				Erelia.World.Chunk.Model.SizeX * 0.5f,
				0f,
				Erelia.World.Chunk.Model.SizeZ * 0.5f
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

			return X == other.X && Z == other.Z;
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
			return a.X == b.X && a.Z == b.Z;
		}

		public static bool operator !=(Coordinates a, Coordinates b)
		{
			return !(a == b);
		}

		public Coordinates WithX(int x) => new Coordinates(x, Z);
		public Coordinates WithZ(int z) => new Coordinates(X, z);

		public Coordinates Offset(int dx, int dz)
		{
			return new Coordinates(X + dx, Z + dz);
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
			return new Coordinates(a.X + b.X, a.Z + b.Z);
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
			return new Coordinates(a.X - b.X, a.Z - b.Z);
		}

		public static Coordinates operator +(Coordinates a, Vector2Int b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new Coordinates(a.X + b.x, a.Z + b.y);
		}

		public static Coordinates operator -(Coordinates a, Vector2Int b)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new Coordinates(a.X - b.x, a.Z - b.y);
		}

		public static Coordinates operator *(Coordinates a, int scalar)
		{
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new Coordinates(a.X * scalar, a.Z * scalar);
		}

		public override string ToString()
		{
			return "(" + X + ", " + Z + ")";
		}

		public static readonly Coordinates Zero = new Coordinates(0, 0);

		public Coordinates Copy()
		{
			return new Coordinates(X, Z);
		}

		public void Set(int x, int z)
		{
			X = x;
			Z = z;
		}
	}
}

