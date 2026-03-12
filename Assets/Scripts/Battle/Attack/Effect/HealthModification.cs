using UnityEngine;

namespace Erelia.Battle.Attack.Effect
{
	[System.Serializable]
	public sealed class HealthModification : Erelia.Battle.Attack.Effect.Definition
	{
		[SerializeField] private int value;

		public override Erelia.Battle.Attack.Effect.Kind Kind =>
			Erelia.Battle.Attack.Effect.Kind.HealthModification;

		public int Value => value;

		public override void ApplyTo(
			Erelia.Battle.Unit.Presenter caster,
			Erelia.Battle.Unit.Presenter target,
			Vector3Int castCell)
		{
			if (!MatchesTargetType(caster, target) ||
				target == null ||
				!target.IsAlive ||
				!target.IsPlaced ||
				value == 0)
			{
				return;
			}

			if (value > 0)
			{
				target.RestoreHealth(value);
				return;
			}

			target.ApplyDamage(-value);
		}
	}
}
