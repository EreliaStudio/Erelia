using NUnit.Framework;
using UnityEngine;

namespace Tests.Taming.Rules
{
	public sealed class TamingRulesTests
	{
		[Test]
		public void CanTrackTaming_EnemyWithConfiguredProfile_ReturnsTrue()
		{
			BattleUnit enemyUnit = CreateBattleUnit(BattleSide.Enemy);
			TamingProfile profile = CreateProfile(new DealDamageRequirement { RequiredAmount = 10 });

			Assert.That(TamingRules.CanTrackTaming(enemyUnit, profile), Is.True);
		}

		[Test]
		public void CanTrackTaming_PlayerUnit_ReturnsFalse()
		{
			BattleUnit playerUnit = CreateBattleUnit(BattleSide.Player);
			TamingProfile profile = CreateProfile(new DealDamageRequirement { RequiredAmount = 10 });

			Assert.That(TamingRules.CanTrackTaming(playerUnit, profile), Is.False);
		}

		[Test]
		public void CanTrackTaming_EmptyProfile_ReturnsFalse()
		{
			BattleUnit enemyUnit = CreateBattleUnit(BattleSide.Enemy);
			var profile = new TamingProfile();

			Assert.That(TamingRules.CanTrackTaming(enemyUnit, profile), Is.False);
		}

		[Test]
		public void AreAllConditionsComplete_NoConditions_ReturnsFalse()
		{
			var profile = new TamingProfile();

			Assert.That(
				TamingRules.AreAllConditionsComplete(profile, new FeatRequirement.Advancement[0]),
				Is.False);
		}

		[Test]
		public void AreAllConditionsComplete_AllCompleted_ReturnsTrue()
		{
			var damageRequirement = new DealDamageRequirement { RequiredAmount = 10 };
			var moveRequirement = new MoveCountRequirement { RequiredCount = 1 };
			var profile = CreateProfile(damageRequirement, moveRequirement);

			var advancements = new[]
			{
				new FeatRequirement.Advancement(0f, 1),
				new FeatRequirement.Advancement(0f, 1)
			};

			Assert.That(TamingRules.AreAllConditionsComplete(profile, advancements), Is.True);
		}

		[Test]
		public void AreAllConditionsComplete_OneIncomplete_ReturnsFalse()
		{
			var damageRequirement = new DealDamageRequirement { RequiredAmount = 10 };
			var moveRequirement = new MoveCountRequirement { RequiredCount = 1 };
			var profile = CreateProfile(damageRequirement, moveRequirement);

			var advancements = new[]
			{
				new FeatRequirement.Advancement(0f, 1),
				new FeatRequirement.Advancement(50f, 0)
			};

			Assert.That(TamingRules.AreAllConditionsComplete(profile, advancements), Is.False);
		}

		[Test]
		public void CreateRecruitFromImpressedUnit_CopiesSpeciesAndForm()
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();

			var sourceCreature = new CreatureUnit
			{
				Species = species,
				CurrentFormID = "WildForm"
			};

			var impressedUnit = new BattleUnit(sourceCreature, BattleSide.Enemy);

			CreatureUnit recruit = TamingRules.CreateRecruitFromImpressedUnit(impressedUnit);

			Assert.That(recruit, Is.Not.Null);
			Assert.That(recruit, Is.Not.SameAs(sourceCreature));
			Assert.That(recruit.Species, Is.SameAs(species));
			Assert.That(recruit.CurrentFormID, Is.EqualTo("WildForm"));
			Assert.That(recruit.FeatBoardProgress, Is.Not.Null);

			Object.DestroyImmediate(species);
		}

		[Test]
		public void CreateRecruitFromImpressedUnit_NullUnit_ReturnsNull()
		{
			Assert.That(TamingRules.CreateRecruitFromImpressedUnit(null), Is.Null);
		}

		private static TamingProfile CreateProfile(params FeatRequirement[] p_requirements)
		{
			var profile = new TamingProfile();

			for (int index = 0; index < p_requirements.Length; index++)
			{
				profile.Conditions.Add(p_requirements[index]);
			}

			return profile;
		}

		private static BattleUnit CreateBattleUnit(BattleSide p_side)
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();

			var creatureUnit = new CreatureUnit
			{
				Species = species
			};

			return new BattleUnit(creatureUnit, p_side);
		}
	}
}