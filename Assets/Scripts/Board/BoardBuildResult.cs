using System.Collections.Generic;
using UnityEngine;

public sealed class BoardBuildResult
{
	public BoardData Board { get; }
	public IReadOnlyList<Vector3Int> BorderWorldCells { get; }

	public BoardBuildResult(BoardData board, IReadOnlyList<Vector3Int> borderWorldCells)
	{
		Board = board;
		BorderWorldCells = borderWorldCells;
	}
}
