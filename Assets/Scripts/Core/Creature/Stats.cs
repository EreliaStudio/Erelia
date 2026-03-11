using UnityEngine;

namespace Erelia.Core.Creature
{
	/// <summary>
	/// Serializable stat container shared by species and creature instances.
	/// </summary>
	[System.Serializable]
	public sealed class Stats
	{
		[SerializeField] private int health;
		[SerializeField] private float stamina = 5f;
		[SerializeField] private int movementPoints;

		public Stats()
		{
		}

		public Stats(int health, float stamina)
			: this(health, stamina, 0)
		{
		}

		public Stats(int health, float stamina, int movementPoints)
		{
			this.health = health;
			this.stamina = stamina;
			this.movementPoints = movementPoints;
		}

		public int Health => Mathf.Max(0, health);
		public float Stamina => Mathf.Max(0f, stamina);
		public int MovementPoints => Mathf.Max(0, movementPoints);

		public Stats Clone()
		{
			return new Stats(Health, Stamina, MovementPoints);
		}

		public static Stats Combine(Stats a, Stats b)
		{
			return new Stats(
				(a?.Health ?? 0) + (b?.Health ?? 0),
				(a?.Stamina ?? 0f) + (b?.Stamina ?? 0f),
				(a?.MovementPoints ?? 0) + (b?.MovementPoints ?? 0));
		}
	}
}
