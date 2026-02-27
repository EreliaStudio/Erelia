using System;

namespace Erelia.Exploration.World.Chunk
{
	public sealed class Model
	{

		public const int SizeX = 16;
		public const int SizeY = 64;
		public const int SizeZ = 16;
		public const int NoEncounterId = -1;
		
		public event Action<Model> Validated;

		public VoxelKit.Cell[,,] Cells = VoxelKit.Cell.CreatePack(SizeX, SizeY, SizeZ);
		public int[,,] EncounterIds = CreateEncounterIdPack(SizeX, SizeY, SizeZ);

		public void Validate()
		{
			Validated?.Invoke(this);
		}

		public void SetCell(int x, int y, int z, VoxelKit.Cell cell)
		{
			Cells[x, y, z] = cell;
		}

		public void SetEncounterId(int x, int y, int z, int encounterId)
		{
			EncounterIds[x, y, z] = encounterId;
		}

		public int GetEncounterId(int x, int y, int z)
		{
			return EncounterIds[x, y, z];
		}

		public bool TryGetEncounterId(int x, int y, int z, out int encounterId)
		{
			encounterId = EncounterIds[x, y, z];
			return encounterId != NoEncounterId;
		}

		public void ClearEncounterIds()
		{
			for (int x = 0; x < SizeX; x++)
			{
				for (int y = 0; y < SizeY; y++)
				{
					for (int z = 0; z < SizeZ; z++)
					{
						EncounterIds[x, y, z] = NoEncounterId;
					}
				}
			}
		}

		private static int[,,] CreateEncounterIdPack(int sizeX, int sizeY, int sizeZ)
		{
			var ids = new int[sizeX, sizeY, sizeZ];
			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					for (int z = 0; z < sizeZ; z++)
					{
						ids[x, y, z] = NoEncounterId;
					}
				}
			}

			return ids;
		}
	}
}


