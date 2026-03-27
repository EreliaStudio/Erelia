using UnityEngine;

namespace Erelia.Battle.Effects
{
	[System.Serializable]
	public abstract class AttackEffect
	{
		[SerializeField] private Erelia.Battle.TargetType targetType =
			Erelia.Battle.TargetType.Enemy;

		public abstract Erelia.Battle.Effects.Kind Kind { get; }
		public Erelia.Battle.TargetType TargetType => targetType;
		public abstract string BuildDescription();

		public abstract void ApplyTo(
			Erelia.Battle.Unit.Presenter caster,
			Erelia.Battle.Unit.Presenter target,
			Vector3Int castCell);

		protected bool MatchesTargetType(
			Erelia.Battle.Unit.Presenter caster,
			Erelia.Battle.Unit.Presenter target)
		{
			if (caster == null || target == null)
			{
				return false;
			}

			switch (targetType)
			{
				case Erelia.Battle.TargetType.Enemy:
					return target.Side != caster.Side;

				case Erelia.Battle.TargetType.Ally:
					return target.Side == caster.Side;

				case Erelia.Battle.TargetType.Both:
					return true;

				default:
					return false;
			}
		}

		protected string BuildTargetDescription()
		{
			switch (targetType)
			{
				case Erelia.Battle.TargetType.Enemy:
					return "enemies";

				case Erelia.Battle.TargetType.Ally:
					return "allies";

				case Erelia.Battle.TargetType.Both:
				default:
					return "targets";
			}
		}
	}
}

