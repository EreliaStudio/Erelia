using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class VoxelTraversalGraph
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

		public Vector3Int Position;

		public Node PositiveX;
		public Node NegativeX;
		public Node PositiveZ;
		public Node NegativeZ;

		public Node(Vector3Int p_position)
		{
			Position = p_position;
		}

		public Node GetNeighbour(Direction p_direction)
		{
			return p_direction switch
			{
				Direction.PositiveX => PositiveX,
				Direction.NegativeX => NegativeX,
				Direction.PositiveZ => PositiveZ,
				Direction.NegativeZ => NegativeZ,
				_ => null
			};
		}

		public void SetNeighbour(Direction p_direction, Node p_node)
		{
			switch (p_direction)
			{
				case Direction.PositiveX:
					PositiveX = p_node;
					break;
				case Direction.NegativeX:
					NegativeX = p_node;
					break;
				case Direction.PositiveZ:
					PositiveZ = p_node;
					break;
				case Direction.NegativeZ:
					NegativeZ = p_node;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(p_direction), p_direction, null);
			}
		}
	}

	private readonly Node[,,] nodes;
	private readonly List<Node> allNodes = new List<Node>();

	public int SizeX => nodes.GetLength(0);
	public int SizeY => nodes.GetLength(1);
	public int SizeZ => nodes.GetLength(2);

	public IReadOnlyList<Node> AllNodes => allNodes;

	public VoxelTraversalGraph(int p_sizeX, int p_sizeY, int p_sizeZ)
	{
		nodes = new Node[p_sizeX, p_sizeY, p_sizeZ];
	}

	public bool IsInside(Vector3Int p_position)
	{
		return p_position.x >= 0 && p_position.x < SizeX &&
			   p_position.y >= 0 && p_position.y < SizeY &&
			   p_position.z >= 0 && p_position.z < SizeZ;
	}

	public bool ContainsNode(Vector3Int p_position)
	{
		return IsInside(p_position) && nodes[p_position.x, p_position.y, p_position.z] != null;
	}

	public bool TryGetNode(Vector3Int p_position, out Node p_node)
	{
		if (!IsInside(p_position))
		{
			p_node = null;
			return false;
		}

		p_node = nodes[p_position.x, p_position.y, p_position.z];
		return p_node != null;
	}

	public Node GetNode(Vector3Int p_position)
	{
		if (!TryGetNode(p_position, out Node node))
		{
			throw new InvalidOperationException($"No traversal node at position {p_position}.");
		}

		return node;
	}

	public Node CreateNode(Vector3Int p_position)
	{
		if (!IsInside(p_position))
		{
			throw new ArgumentOutOfRangeException(nameof(p_position), $"Position {p_position} is outside the graph.");
		}

		Node existingNode = nodes[p_position.x, p_position.y, p_position.z];
		if (existingNode != null)
		{
			return existingNode;
		}

		Node newNode = new Node(p_position);
		nodes[p_position.x, p_position.y, p_position.z] = newNode;
		allNodes.Add(newNode);
		return newNode;
	}
}
