using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class BoardData
{
	public BoardTerrainLayer Terrain { get; }
	public BoardNavigationLayer Navigation { get; }
	public BoardRuntimeRegistry Runtime { get; }
	public Vector3Int WorldAnchor { get; private set; }
	public IReadOnlyList<Vector3Int> BorderLocalCells { get; private set; } = Array.Empty<Vector3Int>();
	public BoardConfiguration.PlacementStyle PlacementStyle { get; private set; } = BoardConfiguration.PlacementStyle.HalfBoard;

	public BoardData() : this(new BoardTerrainLayer(), new BoardNavigationLayer(), new BoardRuntimeRegistry())
	{
	}

	public BoardData(
		BoardTerrainLayer p_terrainLayer,
		BoardNavigationLayer p_navigationLayer,
		BoardRuntimeRegistry p_runtimeRegistry,
		Vector3Int worldAnchor = default)
	{
		Terrain = p_terrainLayer ?? throw new ArgumentNullException(nameof(p_terrainLayer));
		Navigation = p_navigationLayer ?? throw new ArgumentNullException(nameof(p_navigationLayer));
		Runtime = p_runtimeRegistry ?? throw new ArgumentNullException(nameof(p_runtimeRegistry));
		WorldAnchor = worldAnchor;

		Runtime.AttachNavigationLayer(Navigation);
	}

	public void AssignBorderLocalCells(IReadOnlyList<Vector3Int> p_borderLocalCells)
	{
		BorderLocalCells = p_borderLocalCells ?? Array.Empty<Vector3Int>();
	}

	public void AssignPlacementStyle(BoardConfiguration.PlacementStyle p_placementStyle)
	{
		PlacementStyle = p_placementStyle;
	}

	public void ClearMask()
	{
		Terrain.MaskLayer.Clear();
		foreach (Vector3Int localCell in BorderLocalCells)
		{
			Terrain.MaskLayer.TryAddMask(localCell, VoxelMask.BattleAreaBorder);
		}
	}

	public void AssignVoxelRegistry(VoxelRegistry p_voxelRegistry)
	{
		Terrain.AssignVoxelRegistry(p_voxelRegistry);
	}

	public void RebuildNavigation()
	{
		Navigation.Rebuild(Terrain);
	}

	public bool IsInside(Vector3Int p_position)
	{
		return Terrain.IsInside(p_position);
	}

	public bool IsBorderCell(Vector3Int p_position)
	{
		for (int index = 0; index < BorderLocalCells.Count; index++)
		{
			if (BorderLocalCells[index] == p_position)
			{
				return true;
			}
		}

		return false;
	}

	public bool IsStandable(Vector3Int p_position)
	{
		return Navigation.IsStandable(p_position) && !IsBorderCell(p_position);
	}

	public bool HasUnitAt(Vector3Int p_position)
	{
		return Runtime.HasUnitAt(p_position);
	}

	public bool TryGetUnitAt(Vector3Int p_position, out BattleUnit p_unit)
	{
		return Runtime.TryGetUnitAt(p_position, out p_unit);
	}

	public bool CanPlace(BattleObject p_object, Vector3Int p_position)
	{
		return Runtime.CanRegister(p_object, p_position, Navigation);
	}

	public bool TryPlace(BattleObject p_object, Vector3Int p_position)
	{
		return Runtime.TryRegister(p_object, p_position, Navigation);
	}

	public bool TryMove(BattleObject p_object, Vector3Int p_position)
	{
		return Runtime.TryMove(p_object, p_position, Navigation);
	}

	public void Remove(BattleObject p_object)
	{
		Runtime.Remove(p_object);
	}

	public bool TryGetPosition(BattleObject p_object, out Vector3Int p_position)
	{
		return Runtime.TryGetPosition(p_object, out p_position);
	}
}
