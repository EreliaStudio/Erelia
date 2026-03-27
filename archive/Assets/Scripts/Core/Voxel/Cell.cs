using System.IO;

namespace Erelia.Core.Voxel
{
	public class Cell
	{
		public int Id;

		public Erelia.Core.Voxel.Orientation Orientation;

		public Erelia.Core.Voxel.FlipOrientation FlipOrientation;

		public Cell(int id)
			: this(id, Erelia.Core.Voxel.Orientation.PositiveX, Erelia.Core.Voxel.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Erelia.Core.Voxel.Orientation orientation)
			: this(id, orientation, Erelia.Core.Voxel.FlipOrientation.PositiveY)
		{
		}

		public Cell(int id, Erelia.Core.Voxel.Orientation orientation, Erelia.Core.Voxel.FlipOrientation flipOrientation)
		{
			Id = id;
			Orientation = orientation;
			FlipOrientation = flipOrientation;
		}

		public void WriteTo(BinaryWriter writer)
		{
			if (writer == null)
			{
				throw new System.ArgumentNullException(nameof(writer));
			}

			writer.Write(Id);
			writer.Write((byte)Orientation);
			writer.Write((byte)FlipOrientation);
		}

		public void ReadFrom(BinaryReader reader)
		{
			if (reader == null)
			{
				throw new System.ArgumentNullException(nameof(reader));
			}

			Id = reader.ReadInt32();
			Orientation = (Erelia.Core.Voxel.Orientation)reader.ReadByte();
			FlipOrientation = (Erelia.Core.Voxel.FlipOrientation)reader.ReadByte();
		}

		public static Cell ReadNew(BinaryReader reader)
		{
			var cell = new Cell(-1);
			cell.ReadFrom(reader);
			return cell;
		}

		public static Erelia.Core.Voxel.Cell[,,] CreatePack(int sizeX, int sizeY, int sizeZ, Erelia.Core.Voxel.Cell defaultCell = null)
		{
			if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
			{
				throw new System.ArgumentOutOfRangeException("Pack sizes must be > 0.");
			}

			var cells = new Erelia.Core.Voxel.Cell[sizeX, sizeY, sizeZ];

			Erelia.Core.Voxel.Cell seed = defaultCell ?? new Erelia.Core.Voxel.Cell(-1);

			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					for (int k = 0; k < sizeZ; k++)
					{
						cells[i, j, k] = new Erelia.Core.Voxel.Cell(seed.Id, seed.Orientation, seed.FlipOrientation);
					}
				}
			}

			return cells;
		}
	}
}

