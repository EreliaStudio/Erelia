using UnityEngine;

namespace Erelia.Battle.Board
{
	public static class Constructor
	{
		public static Erelia.Battle.Board.Model ExportArea(
			Erelia.Core.Encounter.EncounterTable table,
			Erelia.Exploration.World.Model worldModel,
			Vector3 worldPosition)
		{
			if (table == null || worldModel == null)
			{
				return null;
			}

			int maxRadius = GetMaxRadius(table);
			int sizeX = (maxRadius * 2) + 1;
			int sizeZ = (maxRadius * 2) + 1;
			int sizeY = Erelia.Exploration.World.Chunk.Model.SizeY;

			var cells = Erelia.Battle.Voxel.Cell.CreatePack(sizeX, sizeY, sizeZ, new Erelia.Battle.Voxel.Cell(-2));

			Vector3Int centerCell = WorldToCell(worldPosition);
			int originX = centerCell.x - maxRadius;
			int originZ = centerCell.z - maxRadius;

			for (int x = 0; x < sizeX; x++)
			{
				int worldX = originX + x;
				int dx = worldX - centerCell.x;
				for (int z = 0; z < sizeZ; z++)
				{
					int worldZ = originZ + z;
					int dz = worldZ - centerCell.z;

					float radius = GetRadiusAt(worldX, worldZ, table);
					if ((dx * dx + dz * dz) > radius * radius)
					{
						continue;
					}

					if (!TryGetChunk(worldModel, worldX, worldZ, out Erelia.Exploration.World.Chunk.Model chunk, out int localX, out int localZ))
					{
						continue;
					}

					for (int y = 0; y < sizeY; y++)
					{
						Erelia.Core.VoxelKit.Cell source = chunk.Cells[localX, y, localZ];
						if (source == null)
						{
							cells[x, y, z] = new Erelia.Battle.Voxel.Cell(-1);
						}
						else
						{
							cells[x, y, z] = new Erelia.Battle.Voxel.Cell(source.Id, source.Orientation, source.FlipOrientation);
						}
					}
				}
			}

			Vector3Int origin = new Vector3Int(originX, 0, originZ);
			Vector3Int center = new Vector3Int(centerCell.x, 0, centerCell.z);
			return new Erelia.Battle.Board.Model(cells, origin, center);
		}

		public static int GetMaxRadius(Erelia.Core.Encounter.EncounterTable table)
		{
			if (table == null)
			{
				return 0;
			}

			return Mathf.Max(0, table.BaseRadius + Mathf.Max(0, table.NoiseAmplitude));
		}

		private static float GetRadiusAt(int worldX, int worldZ, Erelia.Core.Encounter.EncounterTable table)
		{
			if (table == null || table.BaseRadius <= 0)
			{
				return 0f;
			}

			float noise = Mathf.PerlinNoise(
				(worldX + table.NoiseSeed) * table.NoiseScale,
				(worldZ + table.NoiseSeed) * table.NoiseScale);

			return table.BaseRadius + (noise * Mathf.Max(0, table.NoiseAmplitude));
		}

		private static bool TryGetChunk(
			Erelia.Exploration.World.Model worldModel,
			int worldX,
			int worldZ,
			out Erelia.Exploration.World.Chunk.Model chunk,
			out int localX,
			out int localZ)
		{
			int sizeX = Erelia.Exploration.World.Chunk.Model.SizeX;
			int sizeZ = Erelia.Exploration.World.Chunk.Model.SizeZ;

			int chunkX = Mathf.FloorToInt(worldX / (float)sizeX);
			int chunkZ = Mathf.FloorToInt(worldZ / (float)sizeZ);
			var coords = new Erelia.Exploration.World.Chunk.Coordinates(chunkX, chunkZ);

			localX = worldX - (chunkX * sizeX);
			localZ = worldZ - (chunkZ * sizeZ);

			if (localX < 0 || localX >= sizeX || localZ < 0 || localZ >= sizeZ)
			{
				chunk = null;
				return false;
			}

			return worldModel.Chunks.TryGetValue(coords, out chunk);
		}

		private static Vector3Int WorldToCell(Vector3 worldPosition)
		{
			return new Vector3Int(
				Mathf.FloorToInt(worldPosition.x),
				Mathf.FloorToInt(worldPosition.y),
				Mathf.FloorToInt(worldPosition.z));
		}
	}
}
