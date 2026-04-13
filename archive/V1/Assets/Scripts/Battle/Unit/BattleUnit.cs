using UnityEngine;

namespace Erelia.Battle.Unit
{
	public sealed class BattleUnit
	{
		private static readonly Vector3Int UnplacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
		private static readonly Erelia.Core.Creature.Stats DefaultResolvedStats =
			new Erelia.Core.Creature.Stats(0, 5f, 6, 3);
		private static readonly Erelia.Battle.Attack[] EmptyAttacks =
			System.Array.Empty<Erelia.Battle.Attack>();

		private readonly Erelia.Core.Creature.Instance.CreatureProjection projection;

		public BattleUnit(Erelia.Core.Creature.Instance.CreatureInstance creature, Erelia.Battle.Side side)
		{
			Erelia.Core.Creature.FeatProgression.EnsureInitialized(creature);
			Creature = creature;
			projection = creature != null ? creature.CreateProjection() : null;
			Side = side;
			Cell = UnplacedCell;
			IsPlaced = false;
			LiveStats = new Erelia.Battle.Unit.LiveStats(projection != null ? projection.Stats : DefaultResolvedStats);
		}

		public Erelia.Core.Creature.Instance.CreatureInstance Creature { get; }
		public Erelia.Core.Creature.Instance.CreatureProjection Projection => projection;
		public Erelia.Battle.Side Side { get; }
		public Vector3Int Cell { get; private set; }
		public bool IsPlaced { get; private set; }
		public string UnitId => projection != null ? projection.UnitId : string.Empty;
		public string DisplayName => projection != null ? projection.DisplayName : string.Empty;
		public Sprite Icon => projection != null ? projection.Icon : null;
		public GameObject UnitPrefab => projection != null ? projection.UnitPrefab : null;
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
		public System.Collections.Generic.IReadOnlyList<Erelia.Battle.Attack> Attacks =>
			projection != null ? projection.EquippedActions : EmptyAttacks;

		public bool TryGetSpecies(out Erelia.Core.Creature.Species species)
		{
			species = projection != null ? projection.Species : null;
			return species != null;
		}

		public bool TryGetForm(out Erelia.Core.Creature.Form form)
		{
			form = projection != null ? projection.Form : null;
			return form != null;
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
	}
}



