using UnityEngine;

namespace Erelia.Core.Creature
{
	[System.Serializable]
	public sealed class Stats
	{
		[SerializeField] private int health;
		[SerializeField] private int strength;
		[SerializeField] private int ability;
		[SerializeField] private int armor;
		[SerializeField] private int resistance;
		[SerializeField] private float stamina = 5f;
		[SerializeField] private int actionPoints;
		[SerializeField] private int movementPoints;
		[SerializeField] private int range;

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

		public Stats(
			int health,
			int strength,
			int ability,
			int armor,
			int resistance,
			int actionPoints,
			int movementPoints,
			float stamina,
			int range)
		{
			this.health = health;
			this.strength = strength;
			this.ability = ability;
			this.armor = armor;
			this.resistance = resistance;
			this.actionPoints = actionPoints;
			this.movementPoints = movementPoints;
			this.stamina = stamina;
			this.range = range;
		}

		public int Health => Mathf.Max(0, health);
		public int Strength => Mathf.Max(0, strength);
		public int Ability => Mathf.Max(0, ability);
		public int Armor => Mathf.Max(0, armor);
		public int Resistance => Mathf.Max(0, resistance);
		public float Stamina => Mathf.Max(0f, stamina);
		public int ActionPoints => Mathf.Max(0, actionPoints);
		public int MovementPoints => Mathf.Max(0, movementPoints);
		public int Range => Mathf.Max(0, range);

		public Stats Clone()
		{
			return new Stats(
				Health,
				Strength,
				Ability,
				Armor,
				Resistance,
				ActionPoints,
				MovementPoints,
				Stamina,
				Range);
		}

		public static Stats Combine(Stats a, Stats b)
		{
			return new Stats(
				(a?.Health ?? 0) + (b?.Health ?? 0),
				(a?.Strength ?? 0) + (b?.Strength ?? 0),
				(a?.Ability ?? 0) + (b?.Ability ?? 0),
				(a?.Armor ?? 0) + (b?.Armor ?? 0),
				(a?.Resistance ?? 0) + (b?.Resistance ?? 0),
				(a?.ActionPoints ?? 0) + (b?.ActionPoints ?? 0),
				(a?.MovementPoints ?? 0) + (b?.MovementPoints ?? 0),
				(a?.Stamina ?? 0f) + (b?.Stamina ?? 0f),
				(a?.Range ?? 0) + (b?.Range ?? 0));
		}

		public static Stats Sum(params Stats[] layers)
		{
			var total = new Stats();
			if (layers == null)
			{
				return total;
			}

			for (int i = 0; i < layers.Length; i++)
			{
				total = Combine(total, layers[i]);
			}

			return total;
		}
	}
}
