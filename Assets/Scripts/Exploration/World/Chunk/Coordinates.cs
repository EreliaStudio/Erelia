using System;
using UnityEngine;

namespace Erelia.Exploration.World.Chunk
{
	/// <summary>
	/// Chunk coordinate pair (X,Z) for world chunk indexing.
	/// Provides conversion, math, and equality helpers for chunk indexing.
	/// </summary>
	[Serializable]
	public class Coordinates : IEquatable<Coordinates>
	{
		/// <summary>
		/// Chunk X coordinate.
		/// </summary>
		public int X;

		/// <summary>
		/// Chunk Z coordinate.
		/// </summary>
		public int Z;

		/// <summary>
		/// Creates a zero-initialized coordinate.
		/// </summary>
		public Coordinates()
		{
			X = 0;
			Z = 0;
		}

		/// <summary>
		/// Creates a coordinate from explicit values.
		/// </summary>
		/// <param name="x">Chunk X.</param>
		/// <param name="z">Chunk Z.</param>
		public Coordinates(int x, int z)
		{
			X = x;
			Z = z;
		}

		/// <summary>
		/// Creates a copy of another coordinate.
		/// </summary>
		/// <param name="other">Coordinate to copy.</param>
		public Coordinates(Coordinates other)
		{
			// Default to zero when input is null.
			if (other == null)
			{
				X = 0;
				Z = 0;
				return;
			}

			X = other.X;
			Z = other.Z;
		}

		/// <summary>
		/// Converts a world position to chunk coordinates.
		/// </summary>
		/// <param name="worldPosition">World-space position.</param>
		/// <returns>Chunk coordinates.</returns>
		public static Coordinates FromWorld(Vector3 worldPosition)
		{
			// Compute chunk index by dividing by chunk size.
			int x = Mathf.FloorToInt(worldPosition.x / Erelia.Exploration.World.Chunk.Model.SizeX);
			int z = Mathf.FloorToInt(worldPosition.z / Erelia.Exploration.World.Chunk.Model.SizeZ);
			return new Coordinates(x, z);
		}

		/// <summary>
		/// Converts to a <see cref="Vector2Int"/>.
		/// </summary>
		/// <returns>Vector2Int with (X,Z).</returns>
		public Vector2Int ToVector2Int()
		{
			return new Vector2Int(X, Z);
		}

		/// <summary>
		/// Creates coordinates from a <see cref="Vector2Int"/>.
		/// </summary>
		/// <param name="value">Source vector.</param>
		/// <returns>Chunk coordinates.</returns>
		public static Coordinates FromVector2Int(Vector2Int value)
		{
			return new Coordinates(value.x, value.y);
		}

		/// <summary>
		/// Returns the world-space origin of the chunk.
		/// </summary>
		/// <param name="y">Optional Y coordinate.</param>
		/// <returns>World position of the chunk origin.</returns>
		public Vector3 WorldOrigin(float y = 0f)
		{
			// Origin is the chunk corner in world units.
			return new Vector3(
				X * Erelia.Exploration.World.Chunk.Model.SizeX,
				y,
				Z * Erelia.Exploration.World.Chunk.Model.SizeZ
			);
		}

		/// <summary>
		/// Returns the world-space center of the chunk.
		/// </summary>
		/// <param name="y">Optional Y coordinate.</param>
		/// <returns>World position of the chunk center.</returns>
		public Vector3 WorldCenter(float y = 0f)
		{
			// Center is origin plus half chunk size.
			return WorldOrigin(y) + new Vector3(
				Erelia.Exploration.World.Chunk.Model.SizeX * 0.5f,
				0f,
				Erelia.Exploration.World.Chunk.Model.SizeZ * 0.5f
			);
		}

		/// <summary>
		/// Checks equality against another coordinate.
		/// </summary>
		/// <param name="other">Other coordinate.</param>
		/// <returns><c>true</c> if equal; otherwise <c>false</c>.</returns>
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

		/// <summary>
		/// Object equality override.
		/// </summary>
		/// <param name="obj">Other object.</param>
		/// <returns><c>true</c> if the object is equal; otherwise <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as Coordinates);
		}

		/// <summary>
		/// Computes a hash code for dictionary usage.
		/// </summary>
		/// <returns>Hash code.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				// Simple hash composition using X and Z.
				int hash = 17;
				hash = (hash * 31) + X;
				hash = (hash * 31) + Z;
				return hash;
			}
		}

		public static bool operator ==(Coordinates a, Coordinates b)
		{
			// Handle reference equality and nulls first.
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

		/// <summary>
		/// Inequality operator.
		/// </summary>
		public static bool operator !=(Coordinates a, Coordinates b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Returns a copy with a new X value.
		/// </summary>
		public Coordinates WithX(int x) => new Coordinates(x, Z);

		/// <summary>
		/// Returns a copy with a new Z value.
		/// </summary>
		public Coordinates WithZ(int z) => new Coordinates(X, z);

		/// <summary>
		/// Returns a new coordinate offset by the given deltas.
		/// </summary>
		/// <param name="dx">Delta X.</param>
		/// <param name="dz">Delta Z.</param>
		/// <returns>Offset coordinates.</returns>
		public Coordinates Offset(int dx, int dz)
		{
			// Return a new coordinate offset by the given deltas.
			return new Coordinates(X + dx, Z + dz);
		}

		/// <summary>
		/// Adds two coordinates.
		/// </summary>
		public static Coordinates operator +(Coordinates a, Coordinates b)
		{
			// Validate inputs before adding.
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

		/// <summary>
		/// Subtracts two coordinates.
		/// </summary>
		public static Coordinates operator -(Coordinates a, Coordinates b)
		{
			// Validate inputs before subtracting.
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

		/// <summary>
		/// Adds a vector to coordinates.
		/// </summary>
		public static Coordinates operator +(Coordinates a, Vector2Int b)
		{
			// Validate input before adding.
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new Coordinates(a.X + b.x, a.Z + b.y);
		}

		/// <summary>
		/// Subtracts a vector from coordinates.
		/// </summary>
		public static Coordinates operator -(Coordinates a, Vector2Int b)
		{
			// Validate input before subtracting.
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new Coordinates(a.X - b.x, a.Z - b.y);
		}

		/// <summary>
		/// Scales coordinates by an integer scalar.
		/// </summary>
		public static Coordinates operator *(Coordinates a, int scalar)
		{
			// Validate input before scaling.
			if (a == null)
			{
				throw new ArgumentNullException(nameof(a));
			}
			return new Coordinates(a.X * scalar, a.Z * scalar);
		}

		/// <summary>
		/// Returns a debug-friendly string representation.
		/// </summary>
		public override string ToString()
		{
			// Format as a simple tuple.
			return "(" + X + ", " + Z + ")";
		}

		/// <summary>
		/// Zero coordinate constant.
		/// </summary>
		public static readonly Coordinates Zero = new Coordinates(0, 0);

		/// <summary>
		/// Returns a copy of this coordinate.
		/// </summary>
		public Coordinates Copy()
		{
			// Return a shallow copy.
			return new Coordinates(X, Z);
		}

		/// <summary>
		/// Sets this coordinate to explicit values.
		/// </summary>
		/// <param name="x">Chunk X.</param>
		/// <param name="z">Chunk Z.</param>
		public void Set(int x, int z)
		{
			// Assign new coordinate values.
			X = x;
			Z = z;
		}
	}
}

