using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	/// <summary>
	/// Shared runtime state for the active battle.
	/// </summary>
	[System.Serializable]
	public sealed class Data
	{
		[SerializeField] private List<Vector3Int> acceptableCoordinates = new List<Vector3Int>();

		[System.NonSerialized] private readonly List<Erelia.Battle.Unit.Presenter> units =
			new List<Erelia.Battle.Unit.Presenter>();
		[System.NonSerialized] private readonly List<Erelia.Battle.Unit.Presenter> playerUnits =
			new List<Erelia.Battle.Unit.Presenter>();
		[System.NonSerialized] private readonly List<Erelia.Battle.Unit.Presenter> enemyUnits =
			new List<Erelia.Battle.Unit.Presenter>();
		[System.NonSerialized] private Erelia.Battle.Unit.Presenter activeUnit;
		[System.NonSerialized] private readonly Erelia.Battle.FeatProgressTracker featProgressTracker =
			new Erelia.Battle.FeatProgressTracker();

		/// <summary>
		/// Battle board model for the current encounter.
		/// </summary>
		public Erelia.Battle.Board.Model Board;

		/// <summary>
		/// Enemy team to fight in the current battle.
		/// </summary>
		public Erelia.Core.Creature.Team EnemyTeam;

		public IReadOnlyList<Vector3Int> AcceptableCoordinates => acceptableCoordinates;
		public IReadOnlyList<Erelia.Battle.Unit.Presenter> Units => units;
		public IReadOnlyList<Erelia.Battle.Unit.Presenter> PlayerUnits => playerUnits;
		public IReadOnlyList<Erelia.Battle.Unit.Presenter> EnemyUnits => enemyUnits;
		public Erelia.Battle.Unit.Presenter ActiveUnit => activeUnit;
		public Erelia.Battle.FeatProgressTracker FeatProgressTracker => featProgressTracker;

		public void Reset(Erelia.Core.Creature.Team enemyTeam, Erelia.Battle.Board.Model board)
		{
			ClearRuntime();
			EnemyTeam = enemyTeam;
			Board = board;
		}

		public void ClearRuntime()
		{
			ClearPlacementData();
			ClearUnits();
			activeUnit = null;
			featProgressTracker.Reset();
		}

		public void ClearPlacementData()
		{
			acceptableCoordinates.Clear();
		}

		public void AddAcceptableCoordinates(IEnumerable<Vector3Int> coordinates)
		{
			AddCoordinates(acceptableCoordinates, coordinates);
		}

		public void AddUnit(Erelia.Battle.Unit.Presenter unit)
		{
			if (unit == null)
			{
				return;
			}

			units.Add(unit);
			if (unit.Side == Erelia.Battle.Side.Player)
			{
				playerUnits.Add(unit);
				return;
			}

			enemyUnits.Add(unit);
		}

		public void ClearUnits()
		{
			activeUnit = null;

			for (int i = 0; i < units.Count; i++)
			{
				Erelia.Battle.Unit.Presenter unit = units[i];
				if (unit == null)
				{
					continue;
				}

				unit.Dispose();
			}

			units.Clear();
			playerUnits.Clear();
			enemyUnits.Clear();
		}

		public bool TryGetPlacedUnitAtCell(Vector3Int coordinate, out Erelia.Battle.Unit.Presenter unit)
		{
			for (int i = 0; i < units.Count; i++)
			{
				Erelia.Battle.Unit.Presenter candidate = units[i];
				if (candidate == null || !candidate.IsAlive || !candidate.IsPlaced || candidate.Cell != coordinate)
				{
					continue;
				}

				unit = candidate;
				return true;
			}

			unit = null;
			return false;
		}

		public void SetActiveUnit(Erelia.Battle.Unit.Presenter unit)
		{
			activeUnit = unit;
		}

		public void ClearActiveUnit()
		{
			activeUnit = null;
		}

		private static void AddCoordinates(List<Vector3Int> target, IEnumerable<Vector3Int> coordinates)
		{
			if (target == null || coordinates == null)
			{
				return;
			}

			foreach (Vector3Int coordinate in coordinates)
			{
				target.Add(coordinate);
			}
		}
	}
}
