using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Board : VoxelGrid
{
	[NonSerialized] private VoxelRegistry voxelRegistry;

	public readonly VoxelMaskCell[,,] MaskCells;
	public BattleCellGraph CellGraph { get; private set; } = new BattleCellGraph();
	public IReadOnlyCollection<Vector3Int> ReachableCells => (IReadOnlyCollection<Vector3Int>) CellGraph.Nodes.Keys;

	public Board() : base(0, 0, 0)
	{
		MaskCells = new VoxelMaskCell[0, 0, 0];
	}

	public Board(int sizeX, int sizeY, int sizeZ) : base(sizeX, sizeY, sizeZ)
	{
		MaskCells = new VoxelMaskCell[sizeX, sizeY, sizeZ];

		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				for (int z = 0; z < sizeZ; z++)
				{
					MaskCells[x, y, z] = new VoxelMaskCell();
				}
			}
		}
	}

	public bool IsInside(Vector3Int p_position)
	{
		if (!IsWithinBounds(p_position))
		{
			return false;
		}

		VoxelCell cell = Cells[p_position.x, p_position.y, p_position.z];
		return cell != null && !cell.IsEmpty;
	}

	public bool IsReachable(Vector3Int p_position)
	{
		return CellGraph != null && CellGraph.ContainsNode(p_position);
	}

	public void UpdateCellGraph()
	{
		BattleCellGraph previousGraph = CellGraph;
		BattleCellGraph nextGraph = voxelRegistry == null
			? new BattleCellGraph()
			: BattleCellGraphBuilder.Build(this, voxelRegistry);

		RestoreObjects(previousGraph, nextGraph);
		CellGraph = nextGraph;
	}

	public void UpdateCellGraph(VoxelRegistry p_voxelRegistry)
	{
		voxelRegistry = p_voxelRegistry;
		UpdateCellGraph();
	}

	public bool TryGetPosition(BattleObject p_object, out Vector3Int p_position)
	{
		if (CellGraph == null)
		{
			p_position = default;
			return false;
		}

		return CellGraph.TryGetPosition(p_object, out p_position);
	}

	public bool CanPlaceUnit(Vector3Int p_position, BattleUnit p_unit = null)
	{
		return IsInside(p_position) && IsReachable(p_position) && CellGraph.CanPlaceUnit(p_position, p_unit);
	}

	public bool TryPlaceUnit(BattleUnit p_unit, Vector3Int p_position)
	{
		return CanPlaceUnit(p_position, p_unit) && CellGraph.TryPlaceUnit(p_unit, p_position);
	}

	public bool TryAddInteractiveObject(BattleInteractiveObject p_interactiveObject, Vector3Int p_position)
	{
		return IsInside(p_position) && IsReachable(p_position) && CellGraph.TryAddInteractiveObject(p_interactiveObject, p_position);
	}

	public bool SwapUnits(BattleUnit p_first, BattleUnit p_second)
	{
		return CellGraph != null && CellGraph.SwapUnits(p_first, p_second);
	}

	public void RemoveObject(BattleObject p_object)
	{
		CellGraph?.RemoveObject(p_object);
	}

	public List<BattleInteractiveObject> RemoveInteractiveObjectsByTags(Vector3Int p_position, IReadOnlyCollection<string> p_tags)
	{
		return CellGraph?.RemoveInteractiveObjectsByTags(p_position, p_tags) ?? new List<BattleInteractiveObject>();
	}

	public List<BattleInteractiveObject> RemoveInteractiveObjectsByTags(IReadOnlyCollection<string> p_tags)
	{
		return CellGraph?.RemoveInteractiveObjectsByTags(p_tags) ?? new List<BattleInteractiveObject>();
	}

	private static void RestoreObjects(BattleCellGraph p_previousGraph, BattleCellGraph p_nextGraph)
	{
		if (p_previousGraph == null || p_nextGraph == null)
		{
			return;
		}

		foreach (KeyValuePair<Vector3Int, BattleCellGraph.Node> entry in p_previousGraph.Nodes)
		{
			Vector3Int position = entry.Key;
			BattleCellGraph.Node node = entry.Value;
			if (node == null)
			{
				continue;
			}

			if (node.Unit != null)
			{
				p_nextGraph.TryPlaceUnit(node.Unit, position);
			}

			if (node.InteractiveObjects == null || node.InteractiveObjects.Count == 0)
			{
				continue;
			}

			for (int index = 0; index < node.InteractiveObjects.Count; index++)
			{
				BattleInteractiveObject interactiveObject = node.InteractiveObjects[index];
				if (interactiveObject != null)
				{
					p_nextGraph.TryAddInteractiveObject(interactiveObject, position);
				}
			}
		}
	}
}
