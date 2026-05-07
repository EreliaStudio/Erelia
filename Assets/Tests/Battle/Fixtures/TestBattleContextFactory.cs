using System;
using UnityEngine;

namespace Tests
{
	internal static class TestBattleContextFactory
	{
		public static BattleContext CreateEmpty()
		{
			BoardTerrainLayer terrain = new BoardTerrainLayer(1, 1, 1);
			BoardData board = new BoardData(terrain, new BoardNavigationLayer(), new BoardRuntimeRegistry());
			board.AssignBorderLocalCells(Array.Empty<Vector3Int>());

			return new BattleContext(
				Array.Empty<CreatureUnit>(),
				Array.Empty<EncounterUnit>(),
				board,
				PlacementStyle.HalfBoard,
				false);
		}
	}
}
