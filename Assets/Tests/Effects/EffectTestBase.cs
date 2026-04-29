using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Effects
{
	public abstract class EffectTestBase
	{
		private readonly List<UnityEngine.Object> ownedAssets = new();

		[TearDown]
		public void TearDown()
		{
			for (int index = 0; index < ownedAssets.Count; index++)
			{
				if (ownedAssets[index] != null)
				{
					UnityEngine.Object.DestroyImmediate(ownedAssets[index]);
				}
			}

			ownedAssets.Clear();
		}

		protected static BattleUnit CreateUnit(int p_health = 100, float p_lifeSteal = 0f)
		{
			var creatureUnit = new CreatureUnit
			{
				Attributes = new Attributes
				{
					Health = p_health,
					LifeSteal = p_lifeSteal
				},
				Abilities = new List<Ability>(),
				PermanentPassives = new List<Status>()
			};

			return new BattleUnit(creatureUnit, BattleSide.Player);
		}

		protected Status CreateStatus(params string[] p_tags)
		{
			Status status = ScriptableObject.CreateInstance<Status>();
			status.Tags = new List<string>(p_tags ?? new string[0]);
			ownedAssets.Add(status);
			return status;
		}

		protected InteractionObject CreateInteractionObject(params string[] p_tags)
		{
			InteractionObject interactionObject = ScriptableObject.CreateInstance<InteractionObject>();
			interactionObject.Tags = new List<string>(p_tags ?? new string[0]);
			ownedAssets.Add(interactionObject);
			return interactionObject;
		}

		protected static DamageTargetEffect CreateDamageEffect(int p_baseDamage)
		{
			return new DamageTargetEffect
			{
				Input = new MathFormula.DamageInput
				{
					BaseDamage = p_baseDamage,
					DamageKind = MathFormula.DamageInput.Kind.Physical,
					AttackRatio = 0f,
					MagicRatio = 0f
				}
			};
		}

		protected static BattleAbilityExecutionContext CreateContext(
			BattleUnit p_source = null,
			BattleUnit p_target = null,
			BattleContext p_battleContext = null,
			Vector3Int p_anchorCell = default,
			Vector3Int p_affectedCell = default)
		{
			return new BattleAbilityExecutionContext
			{
				BattleContext = p_battleContext,
				SourceObject = p_source,
				TargetObject = p_target,
				AnchorCell = p_anchorCell,
				AffectedCell = p_affectedCell
			};
		}

		protected static void PlaceUnit(BattleContext p_battleContext, BattleUnit p_unit, Vector3Int p_cell)
		{
			Assert.That(p_battleContext.Board.IsStandable(p_cell), Is.True, $"Cell should be standable before placement: {p_cell}");
			Assert.That(p_battleContext.TryPlaceUnit(p_unit, p_cell), Is.True, $"Unit should be placeable at: {p_cell}");
		}

		protected static TEvent FindEvent<TEvent>(BattleUnit p_unit) where TEvent : FeatRequirement.EventBase
		{
			if (p_unit == null)
			{
				return null;
			}

			IReadOnlyList<FeatRequirement.EventBase> events = p_unit.PendingFeatEvents;
			for (int index = 0; index < events.Count; index++)
			{
				if (events[index] is TEvent typedEvent)
				{
					return typedEvent;
				}
			}

			return null;
		}
	}
}
