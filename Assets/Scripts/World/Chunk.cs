namespace World
{
	class Chunk
	{
		public const int SizeX = 16;
		public const int SizeY = 64;
		public const int SizeZ = 16;

		public Voxel.Cell[,,] Cells = Voxel.Cell.CreatePack(SizeX, SizeY, SizeZ);

		public Chunk()
		{
			
		}
	}
}
