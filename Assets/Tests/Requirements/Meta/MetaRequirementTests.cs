using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.Requirements.Meta.And
{
	public sealed class AndTests
	{
		private static readonly BattleUnit DummyUnit = new BattleUnit(
			new CreatureUnit { Attributes = new Attributes { Health = 100 }, Abilities = new List<Ability>(), PermanentPassives = new List<global::Status>() },
			BattleSide.Player);

		[Test]
		public void BothChildrenSatisfied_Completes()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 10 };
			var childB = new HealHealthRequirement { RequiredAmount = 5 };
			var req = new AndRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new DamageEvent { Amount = 10, Caster = DummyUnit },
				new HealEvent { Amount = 5, Caster = DummyUnit }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void OnlyOneChildSatisfied_DoesNotComplete()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 10 };
			var childB = new HealHealthRequirement { RequiredAmount = 5 };
			var req = new AndRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new DamageEvent { Amount = 10, Caster = DummyUnit }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void ProgressIsMinOfChildren()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 100 };
			var childB = new DealDamageRequirement { RequiredAmount = 100 };
			var req = new AndRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new DamageEvent { Amount = 50, Caster = DummyUnit }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(50f).Within(0.01f));
		}

		[Test]
		public void EmptyChildren_DoesNotComplete()
		{
			var req = new AndRequirement { Children = new List<FeatRequirement>() };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new DamageEvent { Amount = 100, Caster = DummyUnit }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}
	}
}

namespace Tests.Requirements.Meta.Or
{
	public sealed class OrTests
	{
		private static readonly BattleUnit DummyUnit = new BattleUnit(
			new CreatureUnit { Attributes = new Attributes { Health = 100 }, Abilities = new List<Ability>(), PermanentPassives = new List<global::Status>() },
			BattleSide.Player);

		[Test]
		public void FirstChildSatisfied_Completes()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 10 };
			var childB = new HealHealthRequirement { RequiredAmount = 5 };
			var req = new OrRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new DamageEvent { Amount = 10, Caster = DummyUnit }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void SecondChildSatisfied_Completes()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 10 };
			var childB = new HealHealthRequirement { RequiredAmount = 5 };
			var req = new OrRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new HealEvent { Amount = 5, Caster = DummyUnit }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void NeitherChildSatisfied_DoesNotComplete()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 100 };
			var childB = new HealHealthRequirement { RequiredAmount = 100 };
			var req = new OrRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new DamageEvent { Amount = 40, Caster = DummyUnit },
				new HealEvent { Amount = 30, Caster = DummyUnit }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void ProgressIsMaxOfChildren()
		{
			var childA = new DealDamageRequirement { RequiredAmount = 100 };
			var childB = new HealHealthRequirement { RequiredAmount = 100 };
			var req = new OrRequirement { Children = new List<FeatRequirement> { childA, childB } };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new DamageEvent { Amount = 40, Caster = DummyUnit },
				new HealEvent { Amount = 80, Caster = DummyUnit }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(80f).Within(0.01f));
		}
	}
}
