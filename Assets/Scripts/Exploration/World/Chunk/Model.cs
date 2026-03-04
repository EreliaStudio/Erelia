using System;
using System.IO;
using UnityEngine;

namespace Erelia.Exploration.World.Chunk
{
	/// <summary>
	/// Data model representing a single world chunk.
	/// Stores voxel cells and encounter ids, and supports validation plus binary save/load.
	/// </summary>
	public sealed class Model
	{

		/// <summary>
		/// Chunk size on X axis.
		/// </summary>
		public const int SizeX = 16;

		/// <summary>
		/// Chunk size on Y axis.
		/// </summary>
		public const int SizeY = 64;

		/// <summary>
		/// Chunk size on Z axis.
		/// </summary>
		public const int SizeZ = 16;

		/// <summary>
		/// Sentinel id used to mark "no encounter".
		/// </summary>
		public const int NoEncounterId = -1;

		/// <summary>
		/// Event raised when the chunk is validated.
		/// </summary>
		public event Action<Model> Validated;

		/// <summary>
		/// Voxel cells for this chunk.
		/// </summary>
		public Erelia.Core.VoxelKit.Cell[,,] Cells = Erelia.Core.VoxelKit.Cell.CreatePack(SizeX, SizeY, SizeZ);

		/// <summary>
		/// Encounter id grid for this chunk.
		/// </summary>
		public int[,,] EncounterIds = CreateEncounterIdPack(SizeX, SizeY, SizeZ);

		/// <summary>
		/// Triggers the <see cref="Validated"/> event.
		/// </summary>
		public void Validate()
		{
			// Notify listeners that the chunk is ready.
			Validated?.Invoke(this);
		}

		/// <summary>
		/// Sets a voxel cell at the given coordinates.
		/// </summary>
		/// <param name="x">Cell X.</param>
		/// <param name="y">Cell Y.</param>
		/// <param name="z">Cell Z.</param>
		/// <param name="cell">Cell value.</param>
		public void SetCell(int x, int y, int z, Erelia.Core.VoxelKit.Cell cell)
		{
			Cells[x, y, z] = cell;
		}

		/// <summary>
		/// Sets an encounter id at the given coordinates.
		/// </summary>
		/// <param name="x">Cell X.</param>
		/// <param name="y">Cell Y.</param>
		/// <param name="z">Cell Z.</param>
		/// <param name="encounterId">Encounter id.</param>
		public void SetEncounterId(int x, int y, int z, int encounterId)
		{
			EncounterIds[x, y, z] = encounterId;
		}

		/// <summary>
		/// Gets the encounter id at the given coordinates.
		/// </summary>
		/// <param name="x">Cell X.</param>
		/// <param name="y">Cell Y.</param>
		/// <param name="z">Cell Z.</param>
		/// <returns>Encounter id value.</returns>
		public int GetEncounterId(int x, int y, int z)
		{
			return EncounterIds[x, y, z];
		}

		/// <summary>
		/// Attempts to get a non-empty encounter id at the given coordinates.
		/// </summary>
		/// <param name="x">Cell X.</param>
		/// <param name="y">Cell Y.</param>
		/// <param name="z">Cell Z.</param>
		/// <param name="encounterId">Encounter id if present.</param>
		/// <returns><c>true</c> if an encounter id is present; otherwise <c>false</c>.</returns>
		public bool TryGetEncounterId(int x, int y, int z, out int encounterId)
		{
			encounterId = EncounterIds[x, y, z];
			return encounterId != NoEncounterId;
		}

		/// <summary>
		/// Clears all encounter ids in the chunk.
		/// </summary>
		public void ClearEncounterIds()
		{
			// Fill the encounter grid with the "no encounter" value.
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

		/// <summary>
		/// Creates a new encounter id grid initialized to <see cref="NoEncounterId"/>.
		/// </summary>
		/// <param name="sizeX">Grid size on X.</param>
		/// <param name="sizeY">Grid size on Y.</param>
		/// <param name="sizeZ">Grid size on Z.</param>
		/// <returns>Initialized encounter id grid.</returns>
		private static int[,,] CreateEncounterIdPack(int sizeX, int sizeY, int sizeZ)
		{
			// Allocate the grid and set defaults.
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

		/// <summary>
		/// Serializes this chunk to a binary file.
		/// </summary>
		/// <param name="path">Filesystem path to write to.</param>
		public void ToFile(string path)
		{
			// Validate output path.
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			// Ensure destination directory exists.
			string directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// Ensure arrays are allocated.
			Erelia.Core.VoxelKit.Cell[,,] cells = Cells ?? Erelia.Core.VoxelKit.Cell.CreatePack(SizeX, SizeY, SizeZ);
			int[,,] encounterIds = EncounterIds ?? CreateEncounterIdPack(SizeX, SizeY, SizeZ);

			// Write binary data.
			using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			using var writer = new BinaryWriter(stream);

			// Write voxel cells first.
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

			// Write encounter ids.
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

		/// <summary>
		/// Loads chunk data from a binary file.
		/// </summary>
		/// <param name="path">Filesystem path to read from.</param>
		/// <returns><c>true</c> if loaded; otherwise <c>false</c>.</returns>
		public bool FromFile(string path)
		{
			// Validate input path.
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			// Ensure file exists.
			if (!File.Exists(path))
			{
				Debug.LogWarning($"[Erelia.Exploration.World.Chunk.Model] Save file not found at '{path}'.");
				return false;
			}

			// Ensure cell array is allocated with correct dimensions.
			Erelia.Core.VoxelKit.Cell[,,] cells = Cells;
			if (cells == null || cells.GetLength(0) != SizeX || cells.GetLength(1) != SizeY || cells.GetLength(2) != SizeZ)
			{
				cells = Erelia.Core.VoxelKit.Cell.CreatePack(SizeX, SizeY, SizeZ);
				Cells = cells;
			}

			// Ensure encounter id array is allocated with correct dimensions.
			int[,,] encounterIds = EncounterIds;
			if (encounterIds == null || encounterIds.GetLength(0) != SizeX || encounterIds.GetLength(1) != SizeY || encounterIds.GetLength(2) != SizeZ)
			{
				encounterIds = CreateEncounterIdPack(SizeX, SizeY, SizeZ);
				EncounterIds = encounterIds;
			}

			try
			{
				// Read binary data.
				using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new BinaryReader(stream);

				// Read voxel cells.
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

				// Read encounter ids.
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
				// Handle truncated files gracefully.
				Debug.LogWarning($"[Erelia.Exploration.World.Chunk.Model] Save file is incomplete at '{path}'.");
				return false;
			}

			return true;
		}
	}
}


