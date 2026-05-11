using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Requirements.Shield.ApplyShieldCount
{
	public sealed class ApplyShieldCountTests
	{
		private static BattleUnit CreateBattleUnit()
		{
			CreatureSpecies species = ScriptableObject.CreateInstance<CreatureSpecies>();
			return new BattleUnit(new CreatureUnit { Species = species }, BattleSide.Player);
		}

		[Test]
		public void NoEvents_ZeroProgress()
		{
			var req = new ApplyShieldCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>());

			Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
		}

		[Test]
		public void OneEvent_PartialProgress()
		{
			var req = new ApplyShieldCountRequirement { RequiredCount = 3 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new ShieldAppliedEvent { Caster = CreateBattleUnit() }
			});

			Assert.That(progress.CurrentProgress, Is.EqualTo(100f / 3f).Within(0.01f));
		}

		[Test]
		public void ExactCountRequired_Completes()
		{
			var req = new ApplyShieldCountRequirement { RequiredCount = 2 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new ShieldAppliedEvent { Caster = CreateBattleUnit() },
				new ShieldAppliedEvent { Caster = CreateBattleUnit() }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void MoreThanRequired_StillCompletes()
		{
			var req = new ApplyShieldCountRequirement { RequiredCount = 2 };
			var progress = new FeatRequirementProgress { Requirement = req };

			progress.RegisterEvents(new List<BattleEvent>
			{
				new ShieldAppliedEvent { Caster = CreateBattleUnit() },
				new ShieldAppliedEvent { Caster = CreateBattleUnit() },
				new ShieldAppliedEvent { Caster = CreateBattleUnit() }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}
	}
}
