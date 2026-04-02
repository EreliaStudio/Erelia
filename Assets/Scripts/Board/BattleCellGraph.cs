using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BattleCellGraph
{
	[Serializable]
	public sealed class Node
	{
		public enum Direction
		{
			PositiveX,
			NegativeX,
			PositiveZ,
			NegativeZ
		}

		[Serializable]
		public sealed class Neighbour
		{
			public Node Node;

			public bool HasValue => Node != null;
			public Vector3Int Position => Node != null ? Node.Position : default;
		}

		public Vector3Int Position;
		public BattleUnit Unit;
		public List<BattleInteractiveObject> InteractiveObjects = new List<BattleInteractiveObject>();
		public readonly Dictionary<Direction, Neighbour> Neighbours = CreateNeighbours();

		public Node()
		{
		}

		public Node(Vector3Int p_position)
		{
			Position = p_position;
		}

		public static Direction FromCardinalDirection(CardinalHeightSet.Direction p_direction)
		{
			switch (p_direction)
			{
				case CardinalHeightSet.Direction.PositiveX:
					return Direction.PositiveX;
				case CardinalHeightSet.Direction.NegativeX:
					return Direction.NegativeX;
				case CardinalHeightSet.Direction.PositiveZ:
					return Direction.PositiveZ;
				case CardinalHeightSet.Direction.NegativeZ:
					return Direction.NegativeZ;
				default:
					throw new ArgumentOutOfRangeException(nameof(p_direction), p_direction, "Unsupported graph direction.");
			}
		}

		public void SetNeighbour(Direction p_direction, Node p_node)
		{
			Neighbours[p_direction].Node = p_node;
		}

		public bool TryGetNeighbour(Direction p_direction, out Node p_node)
		{
			p_node = Neighbours[p_direction].Node;
			return p_node != null;
		}

		private static Dictionary<Direction, Neighbour> CreateNeighbours()
		{
			return new Dictionary<Direction, Neighbour>
			{
				[Direction.PositiveX] = new Neighbour(),
				[Direction.NegativeX] = new Neighbour(),
				[Direction.PositiveZ] = new Neighbour(),
				[Direction.NegativeZ] = new Neighbour()
			};
		}
	}

	public readonly Dictionary<Vector3Int, Node> Nodes = new Dictionary<Vector3Int, Node>();
	private readonly Dictionary<BattleObject, Node> nodesByObject = new Dictionary<BattleObject, Node>();

	public bool ContainsNode(Vector3Int p_position)
	{
		return Nodes.ContainsKey(p_position);
	}

	public bool TryGetNode(Vector3Int p_position, out Node p_node)
	{
		return Nodes.TryGetValue(p_position, out p_node);
	}

	public bool TryGetPosition(BattleObject p_object, out Vector3Int p_position)
	{
		if (p_object == null || !nodesByObject.TryGetValue(p_object, out Node node) || node == null)
		{
			p_position = default;
			return false;
		}

		p_position = node.Position;
		return true;
	}

	public bool CanPlaceUnit(Vector3Int p_position, BattleUnit p_unit = null)
	{
		return TryGetNode(p_position, out Node battleCell) && (battleCell.Unit == null || battleCell.Unit == p_unit);
	}

	public bool TryPlaceUnit(BattleUnit p_unit, Vector3Int p_position)
	{
		if (p_unit == null || !CanPlaceUnit(p_position, p_unit))
		{
			return false;
		}

		RemoveObject(p_unit);
		Node battleCell = Nodes[p_position];
		battleCell.Unit = p_unit;
		nodesByObject[p_unit] = battleCell;
		return true;
	}

	public bool TryAddInteractiveObject(BattleInteractiveObject p_interactiveObject, Vector3Int p_position)
	{
		if (p_interactiveObject == null || !TryGetNode(p_position, out Node battleCell))
		{
			return false;
		}

		RemoveObject(p_interactiveObject);
		battleCell.InteractiveObjects ??= new List<BattleInteractiveObject>();
		battleCell.InteractiveObjects.Add(p_interactiveObject);
		nodesByObject[p_interactiveObject] = battleCell;
		return true;
	}

	public bool SwapUnits(BattleUnit p_first, BattleUnit p_second)
	{
		if (p_first == null || p_second == null)
		{
			return false;
		}

		if (!TryGetPosition(p_first, out Vector3Int firstPosition) ||
			!TryGetPosition(p_second, out Vector3Int secondPosition) ||
			!TryGetNode(firstPosition, out Node firstNode) ||
			!TryGetNode(secondPosition, out Node secondNode))
		{
			return false;
		}

		firstNode.Unit = p_second;
		secondNode.Unit = p_first;
		nodesByObject[p_first] = secondNode;
		nodesByObject[p_second] = firstNode;
		return true;
	}

	public void RemoveObject(BattleObject p_object)
	{
		if (p_object == null || !nodesByObject.TryGetValue(p_object, out Node battleCell) || battleCell == null)
		{
			nodesByObject.Remove(p_object);
			return;
		}

		if (p_object is BattleUnit battleUnit)
		{
			if (battleCell.Unit == battleUnit)
			{
				battleCell.Unit = null;
			}
		}
		else if (p_object is BattleInteractiveObject interactiveObject)
		{
			battleCell.InteractiveObjects?.Remove(interactiveObject);
		}

		nodesByObject.Remove(p_object);
	}

	public List<BattleInteractiveObject> RemoveInteractiveObjectsByTags(Vector3Int p_position, IReadOnlyCollection<string> p_tags)
	{
		List<BattleInteractiveObject> removedObjects = new List<BattleInteractiveObject>();
		if (p_tags == null || p_tags.Count == 0)
		{
			return removedObjects;
		}

		if (!TryGetNode(p_position, out Node battleCell) ||
			battleCell.InteractiveObjects == null ||
			battleCell.InteractiveObjects.Count == 0)
		{
			return removedObjects;
		}

		HashSet<string> tags = new HashSet<string>(p_tags);
		for (int index = battleCell.InteractiveObjects.Count - 1; index >= 0; index--)
		{
			BattleInteractiveObject interactiveObject = battleCell.InteractiveObjects[index];
			if (interactiveObject == null)
			{
				battleCell.InteractiveObjects.RemoveAt(index);
				continue;
			}

			if (!interactiveObject.HasAnyTag(tags))
			{
				continue;
			}

			battleCell.InteractiveObjects.RemoveAt(index);
			nodesByObject.Remove(interactiveObject);
			removedObjects.Add(interactiveObject);
		}

		return removedObjects;
	}

	public List<BattleInteractiveObject> RemoveInteractiveObjectsByTags(IReadOnlyCollection<string> p_tags)
	{
		List<BattleInteractiveObject> removedObjects = new List<BattleInteractiveObject>();
		if (p_tags == null || p_tags.Count == 0)
		{
			return removedObjects;
		}

		foreach (KeyValuePair<Vector3Int, Node> entry in Nodes)
		{
			removedObjects.AddRange(RemoveInteractiveObjectsByTags(entry.Key, p_tags));
		}

		return removedObjects;
	}
}
