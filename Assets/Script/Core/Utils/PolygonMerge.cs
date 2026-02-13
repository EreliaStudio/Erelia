using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils
{
	public static class PolygonMerge
	{
		public readonly struct PlaneKey : IEquatable<PlaneKey>
		{
			private const float Scale = 10000f;
			private readonly int nx;
			private readonly int ny;
			private readonly int nz;
			private readonly int d;

			private PlaneKey(int nx, int ny, int nz, int d)
			{
				this.nx = nx;
				this.ny = ny;
				this.nz = nz;
				this.d = d;
			}

			public static PlaneKey From(Vector3 normal, float distance)
			{
				int nx = Mathf.RoundToInt(normal.x * Scale);
				int ny = Mathf.RoundToInt(normal.y * Scale);
				int nz = Mathf.RoundToInt(normal.z * Scale);
				int d = Mathf.RoundToInt(distance * Scale);
				return new PlaneKey(nx, ny, nz, d);
			}

			public bool Equals(PlaneKey other)
			{
				return nx == other.nx && ny == other.ny && nz == other.nz && d == other.d;
			}

			public override bool Equals(object obj)
			{
				return obj is PlaneKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = nx;
				unchecked
				{
					hash = (hash * 397) ^ ny;
					hash = (hash * 397) ^ nz;
					hash = (hash * 397) ^ d;
				}
				return hash;
			}
		}

		public sealed class PlaneGroup
		{
			public readonly Vector3 Normal;
			public readonly float Distance;
			public readonly List<List<Vector3>> Polygons = new List<List<Vector3>>();

			public PlaneGroup(Vector3 normal, float distance)
			{
				Normal = normal;
				Distance = distance;
			}
		}

		public readonly struct PointKey : IEquatable<PointKey>
		{
			private const float Scale = 10000f;
			private readonly int x;
			private readonly int y;

			private PointKey(int x, int y)
			{
				this.x = x;
				this.y = y;
			}

			public static PointKey From(Vector2 value)
			{
				int x = Mathf.RoundToInt(value.x * Scale);
				int y = Mathf.RoundToInt(value.y * Scale);
				return new PointKey(x, y);
			}

			public bool Equals(PointKey other)
			{
				return x == other.x && y == other.y;
			}

			public override bool Equals(object obj)
			{
				return obj is PointKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = x;
				unchecked
				{
					hash = (hash * 397) ^ y;
				}
				return hash;
			}
		}

		public readonly struct EdgeKey : IEquatable<EdgeKey>
		{
			private readonly PointKey a;
			private readonly PointKey b;

			public EdgeKey(PointKey a, PointKey b)
			{
				this.a = a;
				this.b = b;
			}

			public bool Equals(EdgeKey other)
			{
				return a.Equals(other.a) && b.Equals(other.b);
			}

			public override bool Equals(object obj)
			{
				return obj is EdgeKey other && Equals(other);
			}

			public override int GetHashCode()
			{
				int hash = a.GetHashCode();
				unchecked
				{
					hash = (hash * 397) ^ b.GetHashCode();
				}
				return hash;
			}
		}

		public readonly struct Edge
		{
			public readonly PointKey A;
			public readonly PointKey B;

			public Edge(PointKey a, PointKey b)
			{
				A = a;
				B = b;
			}
		}
	}
}
