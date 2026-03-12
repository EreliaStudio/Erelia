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
	}
}
