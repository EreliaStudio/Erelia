using UnityEngine;

namespace Erelia.Battle.Effects
{
	[System.Serializable]
	public sealed class HealthModification : Erelia.Battle.Effects.AttackEffect
	{
		[SerializeField] private int value;

		public override Erelia.Battle.Effects.Kind Kind =>
			Erelia.Battle.Effects.Kind.HealthModification;

		public int Value => value;

		public override string BuildDescription()
		{
			if (value > 0)
			{
				return $"Restore {value} health to {BuildTargetDescription()}";
			}

			if (value < 0)
			{
				return $"Deal {-value} damage to {BuildTargetDescription()}";
			}

			return string.Empty;
		}

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

