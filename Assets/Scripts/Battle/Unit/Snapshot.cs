using UnityEngine;

namespace Erelia.Battle.Unit
{
	public readonly struct Snapshot
	{
		public Snapshot(
			Sprite icon,
			string displayName,
			bool isPlaced,
			bool isAlive,
			int currentHealth,
			int maxHealth,
			float currentStaminaSeconds,
			float staminaProgress01,
			bool isTurnActive)
		{
			Icon = icon;
			DisplayName = displayName;
			IsPlaced = isPlaced;
			IsAlive = isAlive;
			CurrentHealth = currentHealth;
			MaxHealth = maxHealth;
			CurrentStaminaSeconds = currentStaminaSeconds;
			StaminaProgress01 = staminaProgress01;
			IsTurnActive = isTurnActive;
		}

		public Sprite Icon { get; }
		public string DisplayName { get; }
		public bool IsPlaced { get; }
		public bool IsAlive { get; }
		public int CurrentHealth { get; }
		public int MaxHealth { get; }
		public float CurrentStaminaSeconds { get; }
		public float StaminaProgress01 { get; }
		public bool IsTurnActive { get; }
	}
}
