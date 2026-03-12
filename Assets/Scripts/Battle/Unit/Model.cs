using UnityEngine;

namespace Erelia.Battle.Unit
{
	public sealed class Model
	{
		private static readonly Vector3Int UnplacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
		private static readonly Erelia.Core.Creature.Stats DefaultSpeciesStats =
			new Erelia.Core.Creature.Stats(0, 5f, 6, 3);
		private static readonly Erelia.Battle.Attack.Definition[] EmptyAttacks =
			System.Array.Empty<Erelia.Battle.Attack.Definition>();

		public Model(Erelia.Core.Creature.Instance.Model creature, Erelia.Battle.Side side)
		{
			Creature = creature;
			Side = side;
			Cell = UnplacedCell;
			IsPlaced = false;
			LiveStats = new Erelia.Battle.Unit.LiveStats(
				ResolveSpeciesStats(creature),
				creature?.Stats);
		}

		public Erelia.Core.Creature.Instance.Model Creature { get; }
		public Erelia.Battle.Side Side { get; }
		public Vector3Int Cell { get; private set; }
		public bool IsPlaced { get; private set; }
		public string DisplayName => Creature != null ? Creature.DisplayName : string.Empty;
		public Erelia.Battle.Unit.LiveStats LiveStats { get; }
		public Erelia.Core.Creature.Stats Stats => LiveStats.TotalStats;
		public int MaxHealth => LiveStats.MaxHealth;
		public int CurrentHealth => LiveStats.CurrentHealth;
		public bool IsAlive => LiveStats.IsAlive;
		public int ActionPoints => LiveStats.ActionPoints;
		public int RemainingActionPoints => LiveStats.RemainingActionPoints;
		public int MovementPoints => LiveStats.MovementPoints;
		public int RemainingMovementPoints => LiveStats.RemainingMovementPoints;
		public float CurrentStaminaSeconds => LiveStats.CurrentStamina;
		public bool IsTakingTurn => LiveStats.IsTakingTurn;
		public bool IsReadyForTurn => LiveStats.IsReadyForTurn;
		public float StaminaProgress01 => LiveStats.StaminaProgress01;
		public System.Collections.Generic.IReadOnlyList<Erelia.Battle.Attack.Definition> Attacks =>
			Creature != null ? Creature.Attacks : EmptyAttacks;

		public bool TryGetSpecies(out Erelia.Core.Creature.Species species)
		{
			species = null;

			if (Creature == null || Creature.IsEmpty)
			{
				return false;
			}

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			return registry != null &&
				registry.TryGet(Creature.SpeciesId, out species) &&
				species != null;
		}

		public void Place(Vector3Int cell)
		{
			Cell = cell;
			IsPlaced = true;
		}

		public void Unplace()
		{
			Cell = UnplacedCell;
			IsPlaced = false;
		}

		public bool TickStamina(float deltaTime)
		{
			return LiveStats.TickStamina(deltaTime);
		}

		public void BeginTurn()
		{
			LiveStats.BeginTurn();
		}

		public void EndTurn()
		{
			LiveStats.EndTurn();
		}

		public void ResetStamina()
		{
			LiveStats.ResetStamina();
		}

		public void ResetMovementPoints()
		{
			LiveStats.ResetMovementPoints();
		}

		public void ResetActionPoints()
		{
			LiveStats.ResetActionPoints();
		}

		public bool TryConsumeMovementPoints(int amount)
		{
			return LiveStats.TryConsumeMovementPoints(amount);
		}

		public bool TryConsumeActionPoints(int amount)
		{
			return LiveStats.TryConsumeActionPoints(amount);
		}

		public bool ChangeRemainingActionPoints(int delta)
		{
			return LiveStats.ChangeRemainingActionPoints(delta);
		}

		public bool SetCurrentHealth(int value)
		{
			return LiveStats.SetCurrentHealth(value);
		}

		public bool ChangeHealth(int delta)
		{
			return LiveStats.ChangeHealth(delta);
		}

		public bool ApplyDamage(int amount)
		{
			return LiveStats.ApplyDamage(amount);
		}

		public bool RestoreHealth(int amount)
		{
			return LiveStats.RestoreHealth(amount);
		}

		public bool ChangeRemainingMovementPoints(int delta)
		{
			return LiveStats.ChangeRemainingMovementPoints(delta);
		}

		private static Erelia.Core.Creature.Stats ResolveSpeciesStats(Erelia.Core.Creature.Instance.Model creature)
		{
			if (creature == null || creature.IsEmpty)
			{
				return DefaultSpeciesStats;
			}

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			if (registry == null ||
				!registry.TryGet(creature.SpeciesId, out Erelia.Core.Creature.Species species) ||
				species == null)
			{
				return DefaultSpeciesStats;
			}

			return species.Stats;
		}
	}
}
