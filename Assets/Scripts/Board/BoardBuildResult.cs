using System.Collections.Generic;
using UnityEngine;

public sealed class BoardBuildResult
{
	public BoardData Board { get; }
	public Vector3Int WorldOrigin { get; }
	public IReadOnlyList<Vector3Int> BorderWorldCells { get; }

	public BoardBuildResult(BoardData board, Vector3Int worldOrigin, IReadOnlyList<Vector3Int> borderWorldCells)
	{
		Board = board;
		WorldOrigin = worldOrigin;
		BorderWorldCells = borderWorldCells;
	}
}
