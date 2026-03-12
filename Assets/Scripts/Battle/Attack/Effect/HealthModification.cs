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
	}
}
