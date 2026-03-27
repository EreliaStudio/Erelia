using UnityEngine;

namespace Erelia.Core.Creature
{
	[System.Serializable]
	public sealed class Stats
	{
		[SerializeField] private int health;
		[SerializeField] private float stamina = 5f;
		[SerializeField] private int actionPoints;
		[SerializeField] private int movementPoints;

		public Stats()
		{
		}

		public Stats(int health, float stamina)
			: this(health, stamina, 0, 0)
		{
		}

		public Stats(int health, float stamina, int movementPoints)
			: this(health, stamina, 0, movementPoints)
		{
		}

		public Stats(int health, float stamina, int actionPoints, int movementPoints)
		{
			this.health = health;
			this.stamina = stamina;
			this.actionPoints = actionPoints;
			this.movementPoints = movementPoints;
		}

		public int Health => Mathf.Max(0, health);
		public float Stamina => Mathf.Max(0f, stamina);
		public int ActionPoints => Mathf.Max(0, actionPoints);
		public int MovementPoints => Mathf.Max(0, movementPoints);

		public Stats Clone()
		{
			return new Stats(Health, Stamina, ActionPoints, MovementPoints);
		}

		public static Stats Combine(Stats a, Stats b)
		{
			return new Stats(
				(a?.Health ?? 0) + (b?.Health ?? 0),
				(a?.Stamina ?? 0f) + (b?.Stamina ?? 0f),
				(a?.ActionPoints ?? 0) + (b?.ActionPoints ?? 0),
				(a?.MovementPoints ?? 0) + (b?.MovementPoints ?? 0));
		}
	}
}
