using System.IO;

namespace Erelia.Core.VoxelKit
{
	/// <summary>
	/// Represents a single voxel cell in a grid.
	/// </summary>
	/// <remarks>
	/// A <see cref="Cell"/> stores:
	/// <list type="bullet">
	/// <item><description>An integer <see cref="Id"/> used to resolve a voxel <c>Definition</c> from a registry.</description></item>
	/// <item><description>An <see cref="Orientation"/> describing the rotation of the voxel (in 90° steps around Y).</description></item>
	/// <item><description>A <see cref="FlipOrientation"/> describing whether the voxel is vertically flipped.</description></item>
	/// </list>
	/// This type also provides basic binary serialization helpers via <see cref="WriteTo"/> and <see cref="ReadFrom"/>.
	/// </remarks>
	public class Cell
	{
		/// <summary>
		/// Voxel identifier used to look up a <c>Definition</c> in a <c>Registry</c>.
		/// A negative value typically means "empty / no voxel".
		/// </summary>
		public int Id;

		/// <summary>
		/// Orientation of the voxel in the world (rotation around Y in 90° steps).
		/// </summary>
		public Erelia.Core.VoxelKit.Orientation Orientation;

		/// <summary>
		/// Flip orientation of the voxel (typically used to mirror the voxel vertically).
		/// </summary>
		public Erelia.Core.VoxelKit.FlipOrientation FlipOrientation;

		/// <summary>
		/// Creates a new cell with the provided id, using the default orientation (<see cref="Orientation.PositiveX"/>)
		/// and no vertical flip (<see cref="FlipOrientation.PositiveY"/>).
		/// </summary>
		/// <param name="id">Voxel id to store in the cell.</param>
		public Cell(int id)
			: this(id, Erelia.Core.VoxelKit.Orientation.PositiveX, Erelia.Core.VoxelKit.FlipOrientation.PositiveY)
		{
			// Delegates initialization to the full constructor.
		}

		/// <summary>
		/// Creates a new cell with the provided id and orientation, with no vertical flip (<see cref="FlipOrientation.PositiveY"/>).
		/// </summary>
		/// <param name="id">Voxel id to store in the cell.</param>
		/// <param name="orientation">Voxel orientation.</param>
		public Cell(int id, Erelia.Core.VoxelKit.Orientation orientation)
			: this(id, orientation, Erelia.Core.VoxelKit.FlipOrientation.PositiveY)
		{
			// Delegates initialization to the full constructor.
		}

		/// <summary>
		/// Creates a new cell with the provided id, orientation, and flip orientation.
		/// </summary>
		/// <param name="id">Voxel id to store in the cell.</param>
		/// <param name="orientation">Voxel orientation.</param>
		/// <param name="flipOrientation">Voxel flip orientation.</param>
		public Cell(int id, Erelia.Core.VoxelKit.Orientation orientation, Erelia.Core.VoxelKit.FlipOrientation flipOrientation)
		{
			// Store all cell properties.
			Id = id;
			Orientation = orientation;
			FlipOrientation = flipOrientation;
		}

		/// <summary>
		/// Writes this cell to a binary stream.
		/// </summary>
		/// <param name="writer">Binary writer used to output the cell data.</param>
		/// <remarks>
		/// Serialization format (in order):
		/// <list type="number">
		/// <item><description><see cref="Id"/> as <see cref="int"/>.</description></item>
		/// <item><description><see cref="Orientation"/> as a single <see cref="byte"/>.</description></item>
		/// <item><description><see cref="FlipOrientation"/> as a single <see cref="byte"/>.</description></item>
		/// </list>
		/// </remarks>
		public void WriteTo(BinaryWriter writer)
		{
			// Validate input to avoid null-reference writes.
			if (writer == null)
			{
				throw new System.ArgumentNullException(nameof(writer));
			}

			// Write the cell payload in a compact form.
			writer.Write(Id);
			writer.Write((byte)Orientation);
			writer.Write((byte)FlipOrientation);
		}

		/// <summary>
		/// Reads this cell's data from a binary stream and overwrites the current values.
		/// </summary>
		/// <param name="reader">Binary reader used to read the cell data.</param>
		/// <remarks>
		/// Expected serialization format is the one produced by <see cref="WriteTo"/>.
		/// </remarks>
		public void ReadFrom(BinaryReader reader)
		{
			// Validate input to avoid null-reference reads.
			if (reader == null)
			{
				throw new System.ArgumentNullException(nameof(reader));
			}

			// Read back the payload in the same order as WriteTo.
			Id = reader.ReadInt32();
			Orientation = (Erelia.Core.VoxelKit.Orientation)reader.ReadByte();
			FlipOrientation = (Erelia.Core.VoxelKit.FlipOrientation)reader.ReadByte();
		}

		/// <summary>
		/// Allocates a new <see cref="Cell"/> instance and fills it by reading from the provided reader.
		/// </summary>
		/// <param name="reader">Binary reader used to read the cell data.</param>
		/// <returns>A newly created <see cref="Cell"/> populated from the stream.</returns>
		public static Cell ReadNew(BinaryReader reader)
		{
			// Create a placeholder instance, then overwrite its fields from the stream.
			var cell = new Cell(-1);
			cell.ReadFrom(reader);
			return cell;
		}

		/// <summary>
		/// Creates a 3D grid (pack) of cells, initialized by cloning a default seed cell.
		/// </summary>
		/// <param name="sizeX">Pack size on X axis.</param>
		/// <param name="sizeY">Pack size on Y axis.</param>
		/// <param name="sizeZ">Pack size on Z axis.</param>
		/// <param name="defaultCell">
		/// Optional seed cell. If <c>null</c>, an "empty" seed (<c>Id = -1</c>, default orientation/flip) is used.
		/// </param>
		/// <returns>A newly allocated 3D array of <see cref="Cell"/> instances.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if any size is <= 0.</exception>
		public static Erelia.Core.VoxelKit.Cell[,,] CreatePack(int sizeX, int sizeY, int sizeZ, Erelia.Core.VoxelKit.Cell defaultCell = null)
		{
			// Validate pack dimensions.
			if (sizeX <= 0 || sizeY <= 0 || sizeZ <= 0)
			{
				throw new System.ArgumentOutOfRangeException("Pack sizes must be > 0.");
			}

			// Allocate the 3D array.
			var cells = new Erelia.Core.VoxelKit.Cell[sizeX, sizeY, sizeZ];

			// Choose a seed cell (either the provided default cell or an "empty" one).
			Erelia.Core.VoxelKit.Cell seed = defaultCell ?? new Erelia.Core.VoxelKit.Cell(-1);

			// Fill the pack by cloning the seed values into new Cell instances.
			for (int i = 0; i < sizeX; i++)
			{
				for (int j = 0; j < sizeY; j++)
				{
					for (int k = 0; k < sizeZ; k++)
					{
						cells[i, j, k] = new Erelia.Core.VoxelKit.Cell(seed.Id, seed.Orientation, seed.FlipOrientation);
					}
				}
			}

			return cells;
		}
	}
}