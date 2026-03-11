using UnityEngine;

namespace Erelia.Battle.Unit
{
	/// <summary>
	/// Runtime stat container built from species base stats and instance bonus stats.
	/// </summary>
	public sealed class LiveStats
	{
		private const float MinimumStamina = 0.1f;

		private readonly Erelia.Core.Creature.Stats baseStats;
		private readonly Erelia.Core.Creature.Stats bonusStats;
		private readonly Erelia.Core.Creature.Stats totalStats;

		public LiveStats(
			Erelia.Core.Creature.Stats baseStats,
			Erelia.Core.Creature.Stats bonusStats)
		{
			this.baseStats = (baseStats ?? new Erelia.Core.Creature.Stats()).Clone();
			this.bonusStats = (bonusStats ?? new Erelia.Core.Creature.Stats()).Clone();
			totalStats = Erelia.Core.Creature.Stats.Combine(this.baseStats, this.bonusStats);

			CurrentHealth = MaxHealth;
			CurrentStamina = BaseStamina;
		}

		public Erelia.Core.Creature.Stats BaseStats => baseStats;
		public Erelia.Core.Creature.Stats BonusStats => bonusStats;
		public Erelia.Core.Creature.Stats TotalStats => totalStats;
		public int MaxHealth => totalStats.Health;
		public int CurrentHealth { get; private set; }
		public float BaseStamina => Mathf.Max(MinimumStamina, totalStats.Stamina);
		public float CurrentStamina { get; private set; }
		public bool IsTakingTurn { get; private set; }
		public bool IsReadyForTurn => CurrentStamina <= 0f;
		public float StaminaProgress01 => BaseStamina <= 0f
			? 1f
			: Mathf.Clamp01(1f - (CurrentStamina / BaseStamina));

		public bool TickStamina(float deltaTime)
		{
			if (deltaTime <= 0f || IsTakingTurn)
			{
				return IsReadyForTurn;
			}

			CurrentStamina = Mathf.Max(0f, CurrentStamina - deltaTime);
			return IsReadyForTurn;
		}

		public void BeginTurn()
		{
			IsTakingTurn = true;
			CurrentStamina = 0f;
		}

		public void EndTurn()
		{
			IsTakingTurn = false;
			ResetStamina();
		}

		public void ResetStamina()
		{
			CurrentStamina = BaseStamina;
		}
	}
}
