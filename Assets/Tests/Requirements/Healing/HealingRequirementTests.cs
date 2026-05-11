using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Healing.HealSelf
{
	public sealed class HealSelfTests
	{
		private static BattleUnit CreateTestUnit() => new BattleUnit(
			new CreatureUnit { Attributes = new Attributes { Health = 100 }, Abilities = new List<Ability>(), PermanentPassives = new List<global::Status>() },
			BattleSide.Player);

		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new HealTargetRequirement { RequiredAmount = 50, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void AmountBelowRequired_PartialProgress()
		{
			var req = new HealTargetRequirement { RequiredAmount = 100, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };
			var unit = CreateTestUnit();

			progress.RegisterEvents(new List<BattleEvent>
			{
				new HealEvent { Amount = 30, Caster = unit, Target = unit }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(30f).Within(0.01f));
		}

		[Test]
		public void AmountAtRequired_Completes()
		{
			var req = new HealTargetRequirement { RequiredAmount = 50, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };
			var unit = CreateTestUnit();

			progress.RegisterEvents(new List<BattleEvent>
			{
				new HealEvent { Amount = 50, Caster = unit, Target = unit }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void AllyEvent_Ignored()
		{
			var req = new HealTargetRequirement { RequiredAmount = 50, Target = HealTargetRequirement.TargetFilter.Self };
			var progress = new FeatRequirementProgress { Requirement = req };
			var caster = CreateTestUnit();
			var target = CreateTestUnit();

			progress.RegisterEvents(new List<BattleEvent>
			{
				new HealEvent { Amount = 50, Caster = caster, Target = target }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}
	}
}

namespace Tests.Requirements.Healing.HealOther
{
	public sealed class HealOtherTests
	{
		private static BattleUnit CreateTestUnit() => new BattleUnit(
			new CreatureUnit { Attributes = new Attributes { Health = 100 }, Abilities = new List<Ability>(), PermanentPassives = new List<global::Status>() },
			BattleSide.Player);

		[Test]
		public void AmountBelowRequired_PartialProgress()
		{
			var req = new HealTargetRequirement { RequiredAmount = 100, Target = HealTargetRequirement.TargetFilter.Ally };
			var progress = new FeatRequirementProgress { Requirement = req };
			var caster = CreateTestUnit();
			var target = CreateTestUnit();

			progress.RegisterEvents(new List<BattleEvent>
			{
				new HealEvent { Amount = 75, Caster = caster, Target = target }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(75f).Within(0.01f));
		}

		[Test]
		public void AmountAtRequired_Completes()
		{
			var req = new HealTargetRequirement { RequiredAmount = 40, Target = HealTargetRequirement.TargetFilter.Ally };
			var progress = new FeatRequirementProgress { Requirement = req };
			var caster = CreateTestUnit();
			var target = CreateTestUnit();

			progress.RegisterEvents(new List<BattleEvent>
			{
				new HealEvent { Amount = 40, Caster = caster, Target = target }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void SelfEvent_Ignored()
		{
			var req = new HealTargetRequirement { RequiredAmount = 40, Target = HealTargetRequirement.TargetFilter.Ally };
			var progress = new FeatRequirementProgress { Requirement = req };
			var unit = CreateTestUnit();

			progress.RegisterEvents(new List<BattleEvent>
			{
				new HealEvent { Amount = 40, Caster = unit, Target = unit }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}
	}
}

namespace Tests.Requirements.Healing.WinAfterHealing
{
	public sealed class WinAfterHealingTests
	{
		private static BattleUnit CreateTestUnit() => new BattleUnit(
			new CreatureUnit { Attributes = new Attributes { Health = 100 }, Abilities = new List<Ability>(), PermanentPassives = new List<global::Status>() },
			BattleSide.Player);

		[Test]
		public void HealAmountAccumulatesProgress()
		{
			var req = new WinAfterHealingRequirement { RequiredAmount = 100 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new HealEvent { Amount = 50, Caster = CreateTestUnit() }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(50f).Within(0.01f));
		}

		[Test]
		public void ReachingThreshold_Completes()
		{
			var req = new WinAfterHealingRequirement { RequiredAmount = 30 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new HealEvent { Amount = 30, Caster = CreateTestUnit() }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
