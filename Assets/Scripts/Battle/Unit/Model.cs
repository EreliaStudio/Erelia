using UnityEngine;

namespace Erelia.Battle.Unit
{
	public sealed class Model
	{
		private static readonly Vector3Int UnplacedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
		private const float DefaultBaseStaminaSeconds = 5f;

		public Model(Erelia.Core.Creature.Instance.Model creature, Erelia.Battle.Side side)
		{
			Creature = creature;
			Side = side;
			Cell = UnplacedCell;
			IsPlaced = false;
			BaseStaminaSeconds = ResolveBaseStaminaSeconds(creature);
			CurrentStaminaSeconds = BaseStaminaSeconds;
		}

		public Erelia.Core.Creature.Instance.Model Creature { get; }
		public Erelia.Battle.Side Side { get; }
		public Vector3Int Cell { get; private set; }
		public bool IsPlaced { get; private set; }
		public string DisplayName => Creature != null ? Creature.DisplayName : string.Empty;
		public float BaseStaminaSeconds { get; }
		public float CurrentStaminaSeconds { get; private set; }
		public bool IsTakingTurn { get; private set; }
		public bool IsReadyForTurn => CurrentStaminaSeconds <= 0f;
		public float StaminaProgress01 => BaseStaminaSeconds <= 0f
			? 1f
			: Mathf.Clamp01(1f - (CurrentStaminaSeconds / BaseStaminaSeconds));

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
			if (deltaTime <= 0f || IsTakingTurn)
			{
				return IsReadyForTurn;
			}

			CurrentStaminaSeconds = Mathf.Max(0f, CurrentStaminaSeconds - deltaTime);
			return IsReadyForTurn;
		}

		public void BeginTurn()
		{
			IsTakingTurn = true;
			CurrentStaminaSeconds = 0f;
		}

		public void EndTurn()
		{
			IsTakingTurn = false;
			ResetStamina();
		}

		public void ResetStamina()
		{
			CurrentStaminaSeconds = BaseStaminaSeconds;
		}

		private static float ResolveBaseStaminaSeconds(Erelia.Core.Creature.Instance.Model creature)
		{
			if (creature == null || creature.IsEmpty)
			{
				return DefaultBaseStaminaSeconds;
			}

			Erelia.Core.Creature.SpeciesRegistry registry = Erelia.Core.Creature.SpeciesRegistry.Instance;
			if (registry == null ||
				!registry.TryGet(creature.SpeciesId, out Erelia.Core.Creature.Species species) ||
				species == null)
			{
				return DefaultBaseStaminaSeconds;
			}

			return species.BaseStaminaSeconds;
		}
	}
}
