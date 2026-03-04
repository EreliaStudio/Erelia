using UnityEngine;

namespace Erelia.Battle.Board
{
	/// <summary>
	/// Builds battle board models from exploration world data.
	/// Samples voxels around the encounter area and exports a battle cell grid.
	/// </summary>
	public static class Constructor
	{
		/// <summary>
		/// Exports a battle board around a world position using encounter data.
		/// </summary>
		public static Erelia.Battle.Board.Model ExportArea(
			Erelia.Core.Encounter.EncounterTable table,
			Erelia.Exploration.World.Model worldModel,
			Vector3 worldPosition)
		{
			// Validate required inputs.
			if (table == null || worldModel == null)
			{
				return null;
			}

			// Determine the sampling radius and resulting grid dimensions.
			int maxRadius = GetMaxRadius(table);
			int sizeX = (maxRadius * 2) + 1; // Diameter on X.
			int sizeZ = (maxRadius * 2) + 1; // Diameter on Z.
			int sizeY = Erelia.Exploration.World.Chunk.Model.SizeY; // Vertical size matches world chunk height.

			// Allocate the battle voxel grid and fill it with a default "unknown/uninitialized" cell (-2).
			var cells = Erelia.Battle.Voxel.Cell.CreatePack(sizeX, sizeY, sizeZ, new Erelia.Battle.Voxel.Cell(-2));

			// Convert the world position to integer cell coordinates.
			Vector3Int centerCell = WorldToCell(worldPosition);

			// Compute the world-space origin (bottom-left) of the sampled square.
			int originX = centerCell.x - maxRadius;
			int originZ = centerCell.z - maxRadius;

			// Iterate over the sampled square area and copy world voxels into the battle grid.
			for (int x = 0; x < sizeX; x++)
			{
				// Resolve current world X and delta to center (used for radial masking).
				int worldX = originX + x;
				int dx = worldX - centerCell.x;

				for (int z = 0; z < sizeZ; z++)
				{
					// Resolve current world Z and delta to center (used for radial masking).
					int worldZ = originZ + z;
					int dz = worldZ - centerCell.z;

					// Compute the encounter radius at this world position (includes noise).
					float radius = GetRadiusAt(worldX, worldZ, table);

					// Skip cells that are outside the (potentially noisy) circular encounter area.
					if ((dx * dx + dz * dz) > radius * radius)
					{
						continue;
					}

					// Try to locate the chunk containing (worldX, worldZ) and compute local indices.
					if (!TryGetChunk(worldModel, worldX, worldZ, out Erelia.Exploration.World.Chunk.Model chunk, out int localX, out int localZ))
					{
						// Chunk not loaded / not found / invalid local indices.
						continue;
					}

					// Copy the vertical column from the world chunk into the battle grid.
					for (int y = 0; y < sizeY; y++)
					{
						Erelia.Core.VoxelKit.Cell source = chunk.Cells[localX, y, localZ];

						if (source == null)
						{
							// Null means "empty" in the source world: map it to an explicit battle empty cell (-1).
							cells[x, y, z] = new Erelia.Battle.Voxel.Cell(-1);
						}
						else
						{
							// Copy voxel id + orientation data into the battle cell.
							cells[x, y, z] = new Erelia.Battle.Voxel.Cell(source.Id, source.Orientation, source.FlipOrientation);
						}
					}
				}
			}

			// Store the battle-grid origin and center in world cell coordinates (Y forced to 0 for board plane).
			Vector3Int origin = new Vector3Int(originX, 0, originZ);
			Vector3Int center = new Vector3Int(centerCell.x, 0, centerCell.z);

			// Build and return the board model.
			return new Erelia.Battle.Board.Model(cells, origin, center);
		}

		/// <summary>
		/// Computes the maximum encounter radius for the given table.
		/// </summary>
		public static int GetMaxRadius(Erelia.Core.Encounter.EncounterTable table)
		{
			// Null-safe default.
			if (table == null)
			{
				return 0;
			}

			// Ensure non-negative radius and non-negative noise contribution.
			return Mathf.Max(0, table.BaseRadius + Mathf.Max(0, table.NoiseAmplitude));
		}

		/// <summary>
		/// Computes the encounter radius at a world X/Z position.
		/// </summary>
		private static float GetRadiusAt(int worldX, int worldZ, Erelia.Core.Encounter.EncounterTable table)
		{
			// If table is invalid or radius is not meaningful, treat as no area.
			if (table == null || table.BaseRadius <= 0)
			{
				return 0f;
			}

			// Perlin noise sample (0..1) using world coordinates, seed, and scale.
			float noise = Mathf.PerlinNoise(
				(worldX + table.NoiseSeed) * table.NoiseScale,
				(worldZ + table.NoiseSeed) * table.NoiseScale);

			// Base radius plus a noise-based extra radius (scaled by NoiseAmplitude).
			return table.BaseRadius + (noise * Mathf.Max(0, table.NoiseAmplitude));
		}

		/// <summary>
		/// Tries to resolve the chunk and local coordinates for a world cell.
		/// </summary>
		private static bool TryGetChunk(
			Erelia.Exploration.World.Model worldModel,
			int worldX,
			int worldZ,
			out Erelia.Exploration.World.Chunk.Model chunk,
			out int localX,
			out int localZ)
		{
			// Read chunk dimensions (used to convert world cell coordinates to chunk coordinates).
			int sizeX = Erelia.Exploration.World.Chunk.Model.SizeX;
			int sizeZ = Erelia.Exploration.World.Chunk.Model.SizeZ;

			// Convert world cell coordinates to chunk coordinates (floor handles negatives correctly).
			int chunkX = Mathf.FloorToInt(worldX / (float)sizeX);
			int chunkZ = Mathf.FloorToInt(worldZ / (float)sizeZ);

			// Build chunk key.
			var coords = new Erelia.Exploration.World.Chunk.Coordinates(chunkX, chunkZ);

			// Convert world cell coordinates to local indices inside the resolved chunk.
			localX = worldX - (chunkX * sizeX);
			localZ = worldZ - (chunkZ * sizeZ);

			// Reject if local indices are outside expected chunk bounds.
			if (localX < 0 || localX >= sizeX || localZ < 0 || localZ >= sizeZ)
			{
				chunk = null;
				return false;
			}

			// Resolve the chunk from the world model.
			return worldModel.Chunks.TryGetValue(coords, out chunk);
		}

		/// <summary>
		/// Converts a world position to integer cell coordinates.
		/// </summary>
		private static Vector3Int WorldToCell(Vector3 worldPosition)
		{
			// Convert continuous world space to discrete cell space (cell = floor(world)).
			return new Vector3Int(
				Mathf.FloorToInt(worldPosition.x),
				Mathf.FloorToInt(worldPosition.y),
				Mathf.FloorToInt(worldPosition.z));
		}
	}
}