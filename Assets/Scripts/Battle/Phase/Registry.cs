using System;
using UnityEngine;

namespace Erelia.Battle.Phase
{
	[Serializable]
	public sealed class Registry
	{
		[SerializeField] private Erelia.Battle.Phase.Initialize.Root initialize = new Erelia.Battle.Phase.Initialize.Root();

		[SerializeField] private Erelia.Battle.Phase.Placement.Root placement = new Erelia.Battle.Phase.Placement.Root();

		[SerializeField] private Erelia.Battle.Phase.Idle.Root idle = new Erelia.Battle.Phase.Idle.Root();

		[SerializeField] private Erelia.Battle.Phase.PlayerTurn.Root playerTurn = new Erelia.Battle.Phase.PlayerTurn.Root();

		[SerializeField] private Erelia.Battle.Phase.EnemyTurn.Root enemyTurn = new Erelia.Battle.Phase.EnemyTurn.Root();

		[SerializeField] private Erelia.Battle.Phase.ResolveAction.Root resolveAction = new Erelia.Battle.Phase.ResolveAction.Root();

		[SerializeField] private Erelia.Battle.Phase.Result.Root result = new Erelia.Battle.Phase.Result.Root();

		[SerializeField] private Erelia.Battle.Phase.Cleanup.Root cleanup = new Erelia.Battle.Phase.Cleanup.Root();

		public bool TryGetPhase(Id id, out Root phase)
		{
			switch (id)
			{
				case Id.Initialize:
					phase = initialize;
					return phase != null;

				case Id.Placement:
					phase = placement;
					return phase != null;

				case Id.Idle:
					phase = idle;
					return phase != null;

				case Id.PlayerTurn:
					phase = playerTurn;
					return phase != null;

				case Id.EnemyTurn:
					phase = enemyTurn;
					return phase != null;

				case Id.ResolveAction:
					phase = resolveAction;
					return phase != null;

				case Id.Result:
					phase = result;
					return phase != null;

				case Id.Cleanup:
					phase = cleanup;
					return phase != null;

				default:
					phase = null;
					return false;
			}
		}
	}
}
