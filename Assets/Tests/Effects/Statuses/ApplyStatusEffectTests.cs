using NUnit.Framework;
using Tests.Effects;

namespace Tests.Effects.Statuses
{
	public sealed class ApplyStatusEffectTests : EffectTestBase
	{
		[Test]
		public void Apply_AddsStatusWithStackAndDuration()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("guard");

			new ApplyStatusEffect
			{
				Status = status,
				StackCount = 2,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 3 }
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Count, Is.EqualTo(1));
			Assert.That(target.Statuses[0].Value.Status, Is.SameAs(status));
			Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(2));
			Assert.That(target.Statuses[0].Value.RemainingDuration.Type, Is.EqualTo(Duration.Kind.TurnBased));
			Assert.That(target.Statuses[0].Value.RemainingDuration.Turns, Is.EqualTo(3));
		}

		[Test]
		public void Apply_AddsStatusWithOneStackByDefault()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("poison");

			new ApplyStatusEffect
			{
				Status = status,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 2 }
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Count, Is.EqualTo(1));
			Assert.That(target.Statuses[0].Value.Status, Is.SameAs(status));
			Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(1));
			Assert.That(target.Statuses[0].Value.RemainingDuration.Turns, Is.EqualTo(2));
		}

		[Test]
		public void Apply_AddingSameStatusTwice_CreatesSeparateEntries()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("burn");

			new ApplyStatusEffect
			{
				Status = status,
				StackCount = 2,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 3 }
			}.Apply(CreateContext(p_target: target));

			new ApplyStatusEffect
			{
				Status = status,
				StackCount = 3,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 3 }
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Count, Is.EqualTo(2));
			Assert.That(target.Statuses[0].Value.Status, Is.SameAs(status));
			Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(2));
			Assert.That(target.Statuses[1].Value.Status, Is.SameAs(status));
			Assert.That(target.Statuses[1].Value.Stack, Is.EqualTo(3));
		}

		[Test]
		public void Apply_AddingDifferentStatuses_CreatesSeparateEntries()
		{
			BattleUnit target = CreateUnit();
			Status poison = CreateStatus("poison");
			Status guard = CreateStatus("guard");

			new ApplyStatusEffect
			{
				Status = poison,
				StackCount = 1,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 2 }
			}.Apply(CreateContext(p_target: target));

			new ApplyStatusEffect
			{
				Status = guard,
				StackCount = 1,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 3 }
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Count, Is.EqualTo(2));
			Assert.That(target.Statuses.Contains(poison), Is.True);
			Assert.That(target.Statuses.Contains(guard), Is.True);
		}

		[Test]
		public void Apply_WithZeroStack_DoesNotAddStatus()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("burn");

			new ApplyStatusEffect
			{
				Status = status,
				StackCount = 0,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 3 }
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Count, Is.EqualTo(0));
		}

		[Test]
		public void Apply_WithNegativeStack_DoesNotAddStatus()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("burn");

			new ApplyStatusEffect
			{
				Status = status,
				StackCount = -2,
				Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 3 }
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Count, Is.EqualTo(0));
		}

		[Test]
		public void Apply_WithNullStatus_DoesNotThrowAndDoesNotAddStatus()
		{
			BattleUnit target = CreateUnit();

			Assert.DoesNotThrow(() =>
			{
				new ApplyStatusEffect
				{
					Status = null,
					StackCount = 1,
					Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 3 }
				}.Apply(CreateContext(p_target: target));
			});

			Assert.That(target.Statuses.Count, Is.EqualTo(0));
		}

		[Test]
		public void Apply_WhenTargetIsNull_DoesNotThrow()
		{
			Status status = CreateStatus("guard");

			Assert.DoesNotThrow(() =>
			{
				new ApplyStatusEffect
				{
					Status = status,
					StackCount = 1,
					Duration = new Duration { Type = Duration.Kind.TurnBased, Turns = 3 }
				}.Apply(CreateContext());
			});
		}
	}
}
