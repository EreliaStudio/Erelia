using UnityEngine;

namespace Erelia.Battle.Attack.Effect
{
	[System.Serializable]
	public abstract class Definition
	{
		[SerializeField] private Erelia.Battle.Attack.TargetType targetType =
			Erelia.Battle.Attack.TargetType.Enemy;

		public abstract Erelia.Battle.Attack.Effect.Kind Kind { get; }
		public Erelia.Battle.Attack.TargetType TargetType => targetType;

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
				case Erelia.Battle.Attack.TargetType.Enemy:
					return target.Side != caster.Side;

				case Erelia.Battle.Attack.TargetType.Ally:
					return target.Side == caster.Side;

				case Erelia.Battle.Attack.TargetType.Both:
					return true;

				default:
					return false;
			}
		}
	}
}
