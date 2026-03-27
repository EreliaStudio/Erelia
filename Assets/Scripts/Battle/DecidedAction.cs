using System;
using System.Collections.Generic;
using UnityEngine;

namespace Erelia.Battle
{
	public sealed class DecidedAction
	{
		private static readonly Vector3Int[] EmptyPath = Array.Empty<Vector3Int>();

		private DecidedAction(
			Kind actionKind,
			Erelia.Battle.Unit.Presenter actor,
			IReadOnlyList<Vector3Int> movementPath,
			Erelia.Battle.Attack.Definition attack,
			Vector3Int targetCell)
		{
			ActionKind = actionKind;
			Actor = actor;
			MovementPath = CopyPath(movementPath);
			Attack = attack;
			TargetCell = targetCell;
		}

		public enum Kind
		{
			Move = 0,
			Attack = 1,
			EndTurn = 2
		}

		public Kind ActionKind { get; }
		public Erelia.Battle.Unit.Presenter Actor { get; }
		public IReadOnlyList<Vector3Int> MovementPath { get; }
		public int MovementCost => MovementPath.Count;
		public Erelia.Battle.Attack.Definition Attack { get; }
		public Vector3Int TargetCell { get; }

		public static DecidedAction CreateMove(
			Erelia.Battle.Unit.Presenter actor,
			IReadOnlyList<Vector3Int> movementPath)
		{
			if (actor == null)
			{
				throw new ArgumentNullException(nameof(actor));
			}

			if (movementPath == null || movementPath.Count == 0)
			{
				throw new ArgumentException("A movement action requires a non-empty path.", nameof(movementPath));
			}

			return new DecidedAction(Kind.Move, actor, movementPath, null, default);
		}

		public static DecidedAction CreateAttack(
			Erelia.Battle.Unit.Presenter actor,
			Erelia.Battle.Attack.Definition attack,
			Vector3Int targetCell)
		{
			if (actor == null)
			{
				throw new ArgumentNullException(nameof(actor));
			}

			if (attack == null)
			{
				throw new ArgumentNullException(nameof(attack));
			}

			return new DecidedAction(Kind.Attack, actor, null, attack, targetCell);
		}

		public static DecidedAction CreateEndTurn(Erelia.Battle.Unit.Presenter actor)
		{
			if (actor == null)
			{
				throw new ArgumentNullException(nameof(actor));
			}

			return new DecidedAction(Kind.EndTurn, actor, null, null, default);
		}

		private static IReadOnlyList<Vector3Int> CopyPath(IReadOnlyList<Vector3Int> movementPath)
		{
			if (movementPath == null || movementPath.Count == 0)
			{
				return EmptyPath;
			}

			var copy = new Vector3Int[movementPath.Count];
			for (int i = 0; i < movementPath.Count; i++)
			{
				copy[i] = movementPath[i];
			}

			return copy;
		}
	}
}
