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

		public Stats()
		{
		}

		public Stats(int health, float stamina)
		{
			this.health = health;
			this.stamina = stamina;
		}

		public int Health => Mathf.Max(0, health);
		public float Stamina => Mathf.Max(0f, stamina);

		public Stats Clone()
		{
			return new Stats(Health, Stamina);
		}

		public static Stats Combine(Stats a, Stats b)
		{
			return new Stats(
				(a?.Health ?? 0) + (b?.Health ?? 0),
				(a?.Stamina ?? 0f) + (b?.Stamina ?? 0f));
		}
	}
}
