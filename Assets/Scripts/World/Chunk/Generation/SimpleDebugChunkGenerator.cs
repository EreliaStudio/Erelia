namespace Erelia.World.Chunk.Generation
{
	public sealed class SimpleDebugChunkGenerator : IGenerator
	{
		public void Generate(Erelia.World.Chunk.Model chunk, Erelia.World.Chunk.Coordinates coordinates, Erelia.World.Model worldModel)
		{
			if (chunk == null)
			{
				return;
			}

			int maxX = Erelia.World.Chunk.Model.SizeX - 1;
			int maxY = Erelia.World.Chunk.Model.SizeY - 1;
			int maxZ = Erelia.World.Chunk.Model.SizeZ - 1;

			if (maxX < 0 || maxY < 0 || maxZ < 0)
			{
				return;
			}

			for (int x = 0; x <= maxX; x++)
			{
				for (int z = 0; z <= maxZ; z++)
				{
					chunk.SetCell(x, 0, z, new VoxelKit.Cell(0));
				}
			}

			if (maxY < 1)
			{
				return;
			}

			for (int z = 0; z <= maxZ; z++)
			{
				chunk.SetCell(0, 1, z, new VoxelKit.Cell(1));
			}

			for (int z = 0; z <= maxZ; z++)
			{
				chunk.SetCell(maxX, 1, z, new VoxelKit.Cell(2));
			}

			for (int x = 0; x <= maxX; x++)
			{
				chunk.SetCell(x, 1, 0, new VoxelKit.Cell(3));
			}
			
			for (int x = 0; x <= maxX; x++)
			{
				chunk.SetCell(x, 1, maxZ, new VoxelKit.Cell(4));
			}
			
			for (int x = 0; x <= maxX; x++)
			{
				chunk.SetCell(x, 1, 5, new VoxelKit.Cell(4));
			}
		}
	}
}

