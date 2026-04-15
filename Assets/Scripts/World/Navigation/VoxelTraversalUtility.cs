using System;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelTraversalUtility
{
	public const float MaximumVerticalTraversalGap = 0.5f;

	private readonly struct CardinalHeightKey : IEquatable<CardinalHeightKey>
	{
		private readonly CardinalHeightSet source;
		private readonly VoxelOrientation orientation;

		public CardinalHeightKey(CardinalHeightSet p_source, VoxelOrientation p_orientation)
		{
			source = p_source;
			orientation = p_orientation;
		}

		public bool Equals(CardinalHeightKey p_other)
		{
			return ReferenceEquals(source, p_other.source) && orientation == p_other.orientation;
		}

		public override bool Equals(object p_object)
		{
			return p_object is CardinalHeightKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			int hash = source != null ? source.GetHashCode() : 0;
			unchecked
			{
				hash = (hash * 397) ^ (int)orientation;
			}

			return hash;
		}
	}

	private static readonly Dictionary<CardinalHeightKey, CardinalHeightSet> CardinalHeightCache = new Dictionary<CardinalHeightKey, CardinalHeightSet>();

	public static bool IsReachableCell(VoxelGrid p_grid, Vector3Int p_position, VoxelRegistry p_voxelRegistry)
	{
		return IsSolid(p_grid.Cells[p_position.x, p_position.y, p_position.z], p_voxelRegistry) &&
			   IsPassableSpace(p_grid.Cells[p_position.x, p_position.y + 1, p_position.z], p_voxelRegistry) &&
			   IsPassableSpace(p_grid.Cells[p_position.x, p_position.y + 2, p_position.z], p_voxelRegistry);
	}

	public static bool IsPassableSpace(VoxelCell p_cell, VoxelRegistry p_voxelRegistry)
	{
		return p_cell == null || p_cell.IsEmpty || IsPassable(p_cell, p_voxelRegistry);
	}

	public static bool IsSolid(VoxelCell p_cell, VoxelRegistry p_voxelRegistry)
	{
		if (p_cell == null || p_cell.IsEmpty)
		{
			return false;
		}

		if (!p_voxelRegistry.TryGetVoxel(p_cell.Id, out VoxelDefinition voxelDefinition) || voxelDefinition == null)
		{
			return false;
		}

		return voxelDefinition.Data != null && voxelDefinition.Data.Traversal == VoxelTraversal.Obstacle;
	}

	public static bool IsPassable(VoxelCell p_cell, VoxelRegistry p_voxelRegistry)
	{
		if (p_cell == null || p_cell.IsEmpty)
		{
			return false;
		}

		if (!p_voxelRegistry.TryGetVoxel(p_cell.Id, out VoxelDefinition voxelDefinition) || voxelDefinition == null)
		{
			return false;
		}

		return voxelDefinition.Data != null && voxelDefinition.Data.Traversal == VoxelTraversal.Walkable;
	}

	public static bool TryGetWorldHeight(VoxelGrid p_grid, Vector3Int p_position, CardinalHeightSet.Direction p_direction, VoxelRegistry p_voxelRegistry, out float p_height)
	{
		p_height = 0f;

		if (p_grid == null || !p_grid.IsWithinBounds(p_position))
		{
			return false;
		}

		return TryGetWorldHeight(p_grid.Cells[p_position.x, p_position.y, p_position.z], p_position.y, p_direction, p_voxelRegistry, out p_height);
	}

	public static bool TryGetWorldHeight(WorldData p_worldData, Vector3Int p_position, CardinalHeightSet.Direction p_direction, VoxelRegistry p_voxelRegistry, out float p_height)
	{
		p_height = 0f;

		if (p_worldData == null || !p_worldData.TryGetCell(p_position, out VoxelCell cell))
		{
			return false;
		}

		return TryGetWorldHeight(cell, p_position.y, p_direction, p_voxelRegistry, out p_height);
	}

	public static bool TryGetStandingWorldPoint(WorldData p_worldData, Vector3Int p_position, VoxelRegistry p_voxelRegistry, out Vector3 p_worldPoint)
	{
		p_worldPoint = default;

		if (!TryGetWorldHeight(p_worldData, p_position, CardinalHeightSet.Direction.Stationary, p_voxelRegistry, out float height))
		{
			return false;
		}

		p_worldPoint = new Vector3(
			p_position.x + 0.5f,
			height,
			p_position.z + 0.5f);
		return true;
	}

	public static bool TryGetTraversalWorldPoint(WorldData p_worldData, Vector3Int p_position, CardinalHeightSet.Direction p_direction, VoxelRegistry p_voxelRegistry, out Vector3 p_worldPoint)
	{
		p_worldPoint = default;

		if (!TryGetWorldHeight(p_worldData, p_position, p_direction, p_voxelRegistry, out float height))
		{
			return false;
		}

		Vector3 horizontalOffset = p_direction switch
		{
			CardinalHeightSet.Direction.PositiveX => new Vector3(1f, 0f, 0.5f),
			CardinalHeightSet.Direction.NegativeX => new Vector3(0f, 0f, 0.5f),
			CardinalHeightSet.Direction.PositiveZ => new Vector3(0.5f, 0f, 1f),
			CardinalHeightSet.Direction.NegativeZ => new Vector3(0.5f, 0f, 0f),
			_ => new Vector3(0.5f, 0f, 0.5f)
		};

		p_worldPoint = new Vector3(
			p_position.x + horizontalOffset.x,
			height,
			p_position.z + horizontalOffset.z);
		return true;
	}

	public static CardinalHeightSet ResolveWorldHeights(CardinalHeightSet p_source, VoxelOrientation p_orientation)
	{
		if (p_source == null)
		{
			return CardinalHeightSet.CreateDefault();
		}

		CardinalHeightKey key = new CardinalHeightKey(p_source, p_orientation);
		if (CardinalHeightCache.TryGetValue(key, out CardinalHeightSet cached))
		{
			return cached;
		}

		CardinalHeightSet transformed = new CardinalHeightSet(
			positiveX: p_source.Get(ResolveLocalDirection(CardinalHeightSet.Direction.PositiveX, p_orientation)),
			negativeX: p_source.Get(ResolveLocalDirection(CardinalHeightSet.Direction.NegativeX, p_orientation)),
			positiveZ: p_source.Get(ResolveLocalDirection(CardinalHeightSet.Direction.PositiveZ, p_orientation)),
			negativeZ: p_source.Get(ResolveLocalDirection(CardinalHeightSet.Direction.NegativeZ, p_orientation)),
			stationary: p_source.Stationary);

		CardinalHeightCache[key] = transformed;
		return transformed;
	}

	public static CardinalHeightSet.Direction ResolveLocalDirection(CardinalHeightSet.Direction p_worldDirection, VoxelOrientation p_orientation)
	{
		switch (p_orientation)
		{
			case VoxelOrientation.PositiveX:
				return p_worldDirection;

			case VoxelOrientation.PositiveZ:
				return p_worldDirection switch
				{
					CardinalHeightSet.Direction.PositiveX => CardinalHeightSet.Direction.PositiveZ,
					CardinalHeightSet.Direction.NegativeX => CardinalHeightSet.Direction.NegativeZ,
					CardinalHeightSet.Direction.PositiveZ => CardinalHeightSet.Direction.NegativeX,
					CardinalHeightSet.Direction.NegativeZ => CardinalHeightSet.Direction.PositiveX,
					_ => CardinalHeightSet.Direction.Stationary
				};

			case VoxelOrientation.NegativeX:
				return p_worldDirection switch
				{
					CardinalHeightSet.Direction.PositiveX => CardinalHeightSet.Direction.NegativeX,
					CardinalHeightSet.Direction.NegativeX => CardinalHeightSet.Direction.PositiveX,
					CardinalHeightSet.Direction.PositiveZ => CardinalHeightSet.Direction.NegativeZ,
					CardinalHeightSet.Direction.NegativeZ => CardinalHeightSet.Direction.PositiveZ,
					_ => CardinalHeightSet.Direction.Stationary
				};

			case VoxelOrientation.NegativeZ:
				return p_worldDirection switch
				{
					CardinalHeightSet.Direction.PositiveX => CardinalHeightSet.Direction.NegativeZ,
					CardinalHeightSet.Direction.NegativeX => CardinalHeightSet.Direction.PositiveZ,
					CardinalHeightSet.Direction.PositiveZ => CardinalHeightSet.Direction.PositiveX,
					CardinalHeightSet.Direction.NegativeZ => CardinalHeightSet.Direction.NegativeX,
					_ => CardinalHeightSet.Direction.Stationary
				};

			default:
				return p_worldDirection;
		}
	}

	private static bool TryGetWorldHeight(VoxelCell p_cell, int p_baseY, CardinalHeightSet.Direction p_direction, VoxelRegistry p_voxelRegistry, out float p_height)
	{
		p_height = 0f;

		if (p_cell == null || p_cell.IsEmpty)
		{
			return false;
		}

		if (!p_voxelRegistry.TryGetVoxel(p_cell.Id, out VoxelDefinition voxelDefinition) || voxelDefinition?.Shape == null)
		{
			return false;
		}

		CardinalHeightSet localHeights = voxelDefinition.Shape.GetCardinalHeights(p_cell.FlipOrientation);
		CardinalHeightSet worldHeights = ResolveWorldHeights(localHeights, p_cell.Orientation);
		p_height = p_baseY + worldHeights.Get(p_direction);
		return true;
	}
}
