using System;
using System.IO;
using UnityEngine;

namespace Erelia.Exploration.World.Chunk
{
	public sealed class Model
	{

		public const int SizeX = 16;
		public const int SizeY = 64;
		public const int SizeZ = 16;
		public const int NoEncounterId = -1;
		public event Action<Model> Validated;

		public Erelia.Core.VoxelKit.Cell[,,] Cells = Erelia.Core.VoxelKit.Cell.CreatePack(SizeX, SizeY, SizeZ);
		public int[,,] EncounterIds = CreateEncounterIdPack(SizeX, SizeY, SizeZ);

		public void Validate()
		{
			Validated?.Invoke(this);
		}

		public void SetCell(int x, int y, int z, Erelia.Core.VoxelKit.Cell cell)
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

		public void Save(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			string directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			Erelia.Core.VoxelKit.Cell[,,] cells = Cells ?? Erelia.Core.VoxelKit.Cell.CreatePack(SizeX, SizeY, SizeZ);
			int[,,] encounterIds = EncounterIds ?? CreateEncounterIdPack(SizeX, SizeY, SizeZ);

			using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			using var writer = new BinaryWriter(stream);

			for (int x = 0; x < SizeX; x++)
			{
				for (int y = 0; y < SizeY; y++)
				{
					for (int z = 0; z < SizeZ; z++)
					{
						Erelia.Core.VoxelKit.Cell cell = cells[x, y, z] ?? new Erelia.Core.VoxelKit.Cell(-1);
						cell.WriteTo(writer);
					}
				}
			}

			for (int x = 0; x < SizeX; x++)
			{
				for (int y = 0; y < SizeY; y++)
				{
					for (int z = 0; z < SizeZ; z++)
					{
						writer.Write(encounterIds[x, y, z]);
					}
				}
			}
		}

		public bool Load(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			if (!File.Exists(path))
			{
				Debug.LogWarning($"[Erelia.Exploration.World.Chunk.Model] Save file not found at '{path}'.");
				return false;
			}

			Erelia.Core.VoxelKit.Cell[,,] cells = Cells;
			if (cells == null || cells.GetLength(0) != SizeX || cells.GetLength(1) != SizeY || cells.GetLength(2) != SizeZ)
			{
				cells = Erelia.Core.VoxelKit.Cell.CreatePack(SizeX, SizeY, SizeZ);
				Cells = cells;
			}

			int[,,] encounterIds = EncounterIds;
			if (encounterIds == null || encounterIds.GetLength(0) != SizeX || encounterIds.GetLength(1) != SizeY || encounterIds.GetLength(2) != SizeZ)
			{
				encounterIds = CreateEncounterIdPack(SizeX, SizeY, SizeZ);
				EncounterIds = encounterIds;
			}

			try
			{
				using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new BinaryReader(stream);

				for (int x = 0; x < SizeX; x++)
				{
					for (int y = 0; y < SizeY; y++)
					{
						for (int z = 0; z < SizeZ; z++)
						{
							Erelia.Core.VoxelKit.Cell cell = cells[x, y, z];
							if (cell == null)
							{
								cells[x, y, z] = Erelia.Core.VoxelKit.Cell.ReadNew(reader);
							}
							else
							{
								cell.ReadFrom(reader);
							}
						}
					}
				}

				for (int x = 0; x < SizeX; x++)
				{
					for (int y = 0; y < SizeY; y++)
					{
						for (int z = 0; z < SizeZ; z++)
						{
							encounterIds[x, y, z] = reader.ReadInt32();
						}
					}
				}
			}
			catch (EndOfStreamException)
			{
				Debug.LogWarning($"[Erelia.Exploration.World.Chunk.Model] Save file is incomplete at '{path}'.");
				return false;
			}

			return true;
		}
	}
}


