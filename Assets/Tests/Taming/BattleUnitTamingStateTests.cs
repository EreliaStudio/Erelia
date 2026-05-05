using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Taming.BattleState
{
	public sealed class BattleUnitTamingStateTests
	{
		[Test]
		public void NewBattleUnit_IsActiveInBattle()
		{
			BattleUnit unit = CreateBattleUnit(BattleSide.Enemy);

			Assert.That(unit.HasLeftBattle, Is.False);
			Assert.That(unit.IsDefeated, Is.False);
			Assert.That(unit.IsActiveInBattle, Is.True);
		}

		[Test]
		public void MarkLeftBattle_LeavesBattleWithoutDefeat()
		{
			BattleUnit unit = CreateBattleUnit(BattleSide.Enemy);

			unit.MarkLeftBattle();

			Assert.That(unit.HasLeftBattle, Is.True);
			Assert.That(unit.IsDefeated, Is.False);
			Assert.That(unit.IsActiveInBattle, Is.False);
		}

		[Test]
		public void ResetBattleRuntimeState_ClearsLeftBattleFlag()
		{
			BattleUnit unit = CreateBattleUnit(BattleSide.Enemy);

			unit.MarkLeftBattle();
			unit.ResetBattleRuntimeState();

			Assert.That(unit.HasLeftBattle, Is.False);
			Assert.That(unit.IsActiveInBattle, Is.True);
		}

		[Test]
		public void WildBattleUnit_IsNotInitiallyTamed()
		{
			WildBattleUnit wildUnit = CreateWildBattleUnit(CreateProfileWithCondition());

			Assert.That(wildUnit.IsTamed, Is.False);
			Assert.That(wildUnit.TamingProgress, Is.Not.Null);
			Assert.That(wildUnit.TamingProgress.IsImpressed, Is.False);
		}

		[Test]
		public void WildBattleUnit_EvaluateTamingEvent_WhenConditionMet_IsTamed()
		{
			WildBattleUnit wildUnit = CreateWildBattleUnit(CreateProfileWithCondition());

			wildUnit.EvaluateTamingEvent(new DealDamageRequirement.Event { Amount = 10 });

			Assert.That(wildUnit.IsTamed, Is.True);
			Assert.That(wildUnit.TamingProgress.IsImpressed, Is.True);
		}

		[Test]
		public void WildBattleUnit_EvaluateTamingEvent_NullEvent_DoesNotThrow()
		{
			WildBattleUnit wildUnit = CreateWildBattleUnit(CreateProfileWithCondition());

			Assert.DoesNotThrow(() => wildUnit.EvaluateTamingEvent(null));
			Assert.That(wildUnit.IsTamed, Is.False);
		}

		[Test]
		public void WildBattleUnit_MarkUntamable_PreventsImpression()
		{
			WildBattleUnit wildUnit = CreateWildBattleUnit(CreateProfileWithCondition());

			wildUnit.MarkUntamable();
			wildUnit.EvaluateTamingEvent(new DealDamageRequirement.Event { Amount = 10 });

			Assert.That(wildUnit.IsTamed, Is.False);
			Assert.That(wildUnit.TamingProgress.HasFailed, Is.True);
		}

		[Test]
		public void WildBattleUnit_ResetBattleRuntimeState_ResetsTamingProgress()
		{
			WildBattleUnit wildUnit = CreateWildBattleUnit(CreateProfileWithCondition());

			wildUnit.EvaluateTamingEvent(new DealDamageRequirement.Event { Amount = 10 });
			Assert.That(wildUnit.IsTamed, Is.True);

			wildUnit.ResetBattleRuntimeState();

			Assert.That(wildUnit.IsTamed, Is.False);
			Assert.That(wildUnit.TamingProgress.IsImpressed, Is.False);
			Assert.That(wildUnit.HasLeftBattle, Is.False);
		}

		[Test]
		public void WildBattleUnit_EvaluateTamingEvent_PartialProgress_DoesNotImpress()
		{
			WildBattleUnit wildUnit = CreateWildBattleUnit(CreateProfileWithCondition());

			wildUnit.EvaluateTamingEvent(new DealDamageRequirement.Event { Amount = 5 });

			Assert.That(wildUnit.IsTamed, Is.False);
			Assert.That(wildUnit.TamingProgress.ConditionAdvancements[0].Progress, Is.EqualTo(50f).Within(0.01f));
		}

		private static TamingProfile CreateProfileWithCondition()
		{
			var profile = new TamingProfile();
			profile.Conditions.Add(new DealDamageRequirement { RequiredAmount = 10 });
			return profile;
		}

		private static WildBattleUnit CreateWildBattleUnit(TamingProfile p_profile)
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();
			var creatureUnit = new CreatureUnit { Species = species };
			return new WildBattleUnit(creatureUnit, BattleSide.Enemy, p_profile);
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
