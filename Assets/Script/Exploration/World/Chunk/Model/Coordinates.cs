using UnityEngine;

namespace World.Chunk.Model
{
	public class Coordinates
	{
		public int X;
		public int Y;
		public int Z;

		public Coordinates()
		{
			
		}
		
		public Coordinates(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public static Coordinates FromWorld(Vector3 worldPosition)
		{
			int x = Mathf.FloorToInt(worldPosition.x / World.Chunk.Model.Data.SizeX);
			int y = Mathf.FloorToInt(worldPosition.y / World.Chunk.Model.Data.SizeY);
			int z = Mathf.FloorToInt(worldPosition.z / World.Chunk.Model.Data.SizeZ);
			return new Coordinates(x, y, z);
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

		public override string ToString()
		{
			return "(" + X + ", " + Y + ", " + Z + ")";
		}
	}
}
