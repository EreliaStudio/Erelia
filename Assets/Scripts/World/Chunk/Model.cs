using System;

namespace Erelia.World.Chunk
{
	public sealed class Model
	{

		public const int SizeX = 16;
		public const int SizeY = 64;
		public const int SizeZ = 16;
		
		public event Action<Model> Validated;

		public Erelia.Voxel.Cell[,,] Cells = Erelia.Voxel.Cell.CreatePack(SizeX, SizeY, SizeZ);

		public void Validate()
		{
			Erelia.Logger.Log("[Erelia.World.Chunk.Model] Validate called.");
			Validated?.Invoke(this);
		}

		public void SetCell(int x, int y, int z, Erelia.Voxel.Cell cell)
		{
			Cells[x, y, z] = cell;
		}
	}
}

