using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Taming.Progress
{
	public sealed class TamingProgressTests
	{
		[Test]
		public void EvaluateEvents_NoMatchingProgress_DoesNotImpress()
		{
			BattleUnit targetUnit = CreateBattleUnit(BattleSide.Enemy);
			var requirement = new DealDamageRequirement { RequiredAmount = 10 };
			var progress = new TamingProgress(targetUnit, CreateProfile(requirement));

			progress.EvaluateEvents(new List<FeatRequirement.EventBase>
			{
				new MoveCountRequirement.Event()
			});

			Assert.That(progress.IsImpressed, Is.False);
			Assert.That(progress.ConditionAdvancements[0].Progress, Is.EqualTo(0f));
		}

		[Test]
		public void EvaluateEvents_PartialProgress_DoesNotImpress()
		{
			BattleUnit targetUnit = CreateBattleUnit(BattleSide.Enemy);
			var requirement = new DealDamageRequirement { RequiredAmount = 10 };
			var progress = new TamingProgress(targetUnit, CreateProfile(requirement));

			progress.EvaluateEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 5 }
			});

			Assert.That(progress.IsImpressed, Is.False);
			Assert.That(progress.ConditionAdvancements[0].Progress, Is.EqualTo(50f).Within(0.01f));
		}

		[Test]
		public void EvaluateEvents_SingleConditionCompleted_Impresses()
		{
			BattleUnit targetUnit = CreateBattleUnit(BattleSide.Enemy);
			var requirement = new DealDamageRequirement { RequiredAmount = 10 };
			var progress = new TamingProgress(targetUnit, CreateProfile(requirement));

			progress.EvaluateEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 10 }
			});

			Assert.That(progress.IsImpressed, Is.True);
		}

		[Test]
		public void EvaluateEvents_MultipleConditions_AllRequiredBeforeImpressed()
		{
			BattleUnit targetUnit = CreateBattleUnit(BattleSide.Enemy);

			var damageRequirement = new DealDamageRequirement { RequiredAmount = 10 };
			var moveRequirement = new MoveCountRequirement { RequiredCount = 1 };

			var progress = new TamingProgress(
				targetUnit,
				CreateProfile(damageRequirement, moveRequirement));

			progress.EvaluateEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 10 }
			});

			Assert.That(progress.IsImpressed, Is.False);

			progress.EvaluateEvents(new List<FeatRequirement.EventBase>
			{
				new MoveCountRequirement.Event()
			});

			Assert.That(progress.IsImpressed, Is.True);
		}

		[Test]
		public void EvaluateEvents_FightScope_AccumulatesAcrossCalls()
		{
			BattleUnit targetUnit = CreateBattleUnit(BattleSide.Enemy);
			var requirement = new DealDamageRequirement { RequiredAmount = 10 };
			var progress = new TamingProgress(targetUnit, CreateProfile(requirement));

			progress.EvaluateEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 4 }
			});

			Assert.That(progress.IsImpressed, Is.False);
			Assert.That(progress.ConditionAdvancements[0].Progress, Is.EqualTo(40f).Within(0.01f));

			progress.EvaluateEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 6 }
			});

			Assert.That(progress.IsImpressed, Is.True);
		}

		[Test]
		public void MarkFailed_BeforeCompletion_PreventsImpression()
		{
			BattleUnit targetUnit = CreateBattleUnit(BattleSide.Enemy);
			var requirement = new DealDamageRequirement { RequiredAmount = 10 };
			var progress = new TamingProgress(targetUnit, CreateProfile(requirement));

			progress.MarkFailed();

			progress.EvaluateEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 10 }
			});

			Assert.That(progress.HasFailed, Is.True);
			Assert.That(progress.IsImpressed, Is.False);
		}

		[Test]
		public void MarkFailed_AfterImpressed_DoesNotRemoveImpressedState()
		{
			BattleUnit targetUnit = CreateBattleUnit(BattleSide.Enemy);
			var requirement = new DealDamageRequirement { RequiredAmount = 10 };
			var progress = new TamingProgress(targetUnit, CreateProfile(requirement));

			progress.EvaluateEvents(new List<FeatRequirement.EventBase>
			{
				new DealDamageRequirement.Event { Amount = 10 }
			});

			progress.MarkFailed();

			Assert.That(progress.IsImpressed, Is.True);
			Assert.That(progress.HasFailed, Is.False);
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