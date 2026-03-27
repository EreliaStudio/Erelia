using UnityEngine;

namespace Erelia.Battle.Unit
{
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
			RemainingActionPoints = ActionPoints;
			RemainingMovementPoints = MovementPoints;
		}

		public Erelia.Core.Creature.Stats BaseStats => baseStats;
		public Erelia.Core.Creature.Stats BonusStats => bonusStats;
		public Erelia.Core.Creature.Stats TotalStats => totalStats;
		public int MaxHealth => totalStats.Health;
		public int CurrentHealth { get; private set; }
		public bool IsAlive => CurrentHealth > 0;
		public float BaseStamina => Mathf.Max(MinimumStamina, totalStats.Stamina);
		public int ActionPoints => totalStats.ActionPoints;
		public int RemainingActionPoints { get; private set; }
		public int MovementPoints => totalStats.MovementPoints;
		public int RemainingMovementPoints { get; private set; }
		public float CurrentStamina { get; private set; }
		public bool IsTakingTurn { get; private set; }
		public bool IsReadyForTurn => CurrentStamina <= 0f;
		public float StaminaProgress01 => BaseStamina <= 0f
			? 1f
			: Mathf.Clamp01(1f - (CurrentStamina / BaseStamina));

		public bool TickStamina(float deltaTime)
		{
			if (!IsAlive)
			{
				return false;
			}

			if (deltaTime <= 0f || IsTakingTurn)
			{
				return IsReadyForTurn;
			}

			CurrentStamina = Mathf.Max(0f, CurrentStamina - deltaTime);
			return IsReadyForTurn;
		}

		public void BeginTurn()
		{
			if (!IsAlive)
			{
				return;
			}

			IsTakingTurn = true;
			CurrentStamina = 0f;
		}

		public void EndTurn()
		{
			IsTakingTurn = false;
			if (!IsAlive)
			{
				return;
			}

			ResetActionPoints();
			ResetMovementPoints();
			ResetStamina();
		}

		public void ResetStamina()
		{
			CurrentStamina = BaseStamina;
		}

		public void ResetMovementPoints()
		{
			RemainingMovementPoints = IsAlive ? MovementPoints : 0;
		}

		public void ResetActionPoints()
		{
			RemainingActionPoints = IsAlive ? ActionPoints : 0;
		}

		public bool SetRemainingActionPoints(int value)
		{
			int clampedValue = Mathf.Clamp(value, 0, ActionPoints);
			if (clampedValue == RemainingActionPoints)
			{
				return false;
			}

			RemainingActionPoints = clampedValue;
			return true;
		}

		public bool ChangeRemainingActionPoints(int delta)
		{
			if (delta == 0)
			{
				return false;
			}

			return SetRemainingActionPoints(RemainingActionPoints + delta);
		}

		public bool TryConsumeActionPoints(int amount)
		{
			if (!IsAlive)
			{
				return false;
			}

			if (amount <= 0)
			{
				return true;
			}

			if (amount > RemainingActionPoints)
			{
				return false;
			}

			return ChangeRemainingActionPoints(-amount);
		}

		public bool TryConsumeMovementPoints(int amount)
		{
			if (!IsAlive || amount <= 0 || amount > RemainingMovementPoints)
			{
				return false;
			}

			return ChangeRemainingMovementPoints(-amount);
		}

		public bool SetRemainingMovementPoints(int value)
		{
			int clampedValue = Mathf.Clamp(value, 0, MovementPoints);
			if (clampedValue == RemainingMovementPoints)
			{
				return false;
			}

			RemainingMovementPoints = clampedValue;
			return true;
		}

		public bool ChangeRemainingMovementPoints(int delta)
		{
			if (delta == 0)
			{
				return false;
			}

			return SetRemainingMovementPoints(RemainingMovementPoints + delta);
		}

		public bool SetCurrentHealth(int value)
		{
			int clampedValue = Mathf.Clamp(value, 0, MaxHealth);
			if (clampedValue == CurrentHealth)
			{
				return false;
			}

			CurrentHealth = clampedValue;
			if (!IsAlive)
			{
				IsTakingTurn = false;
				CurrentStamina = BaseStamina;
				RemainingActionPoints = 0;
				RemainingMovementPoints = 0;
			}

			return true;
		}

		public bool ChangeHealth(int delta)
		{
			if (delta == 0)
			{
				return false;
			}

			return SetCurrentHealth(CurrentHealth + delta);
		}

		public bool ApplyDamage(int amount)
		{
			if (amount <= 0)
			{
				return false;
			}

			return SetCurrentHealth(CurrentHealth - amount);
		}

		public bool RestoreHealth(int amount)
		{
			if (amount <= 0)
			{
				return false;
			}

			return SetCurrentHealth(CurrentHealth + amount);
		}
	}
}
