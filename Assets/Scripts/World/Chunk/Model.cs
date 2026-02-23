using System;

namespace Erelia.World.Chunk
{
	public sealed class Model
	{

		public const int SizeX = 16;
		public const int SizeY = 64;
		public const int SizeZ = 16;
		
		public event Action<Model> Validated;

		public VoxelKit.Cell[,,] Cells = VoxelKit.Cell.CreatePack(SizeX, SizeY, SizeZ);

		public void Validate()
		{
			Validated?.Invoke(this);
		}

		public void SetCell(int x, int y, int z, VoxelKit.Cell cell)
		{
			Cells[x, y, z] = cell;
		}
	}
}


