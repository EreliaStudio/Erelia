using NUnit.Framework;
using Tests.Effects;

namespace Tests.Effects.Statuses
{
	public sealed class ConsumeStatusTests : EffectTestBase
	{
		[Test]
		public void Apply_RemovesRequestedStacks()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("focus");
			target.Statuses.Add(status, p_stackCount: 3);

			new ConsumeStatus
			{
				Status = status,
				NbOfStackConsumed = 2
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Count, Is.EqualTo(1));
			Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(1));
		}

		[Test]
		public void Apply_WhenConsumedStacksEqualCurrentStacks_RemovesStatus()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("focus");
			target.Statuses.Add(status, p_stackCount: 3);

			new ConsumeStatus
			{
				Status = status,
				NbOfStackConsumed = 3
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Contains(status), Is.False);
			Assert.That(target.Statuses.Count, Is.EqualTo(0));
		}

		[Test]
		public void Apply_WhenConsumedStacksExceedCurrentStacks_RemovesStatus()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("focus");
			target.Statuses.Add(status, p_stackCount: 3);

			new ConsumeStatus
			{
				Status = status,
				NbOfStackConsumed = 10
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Contains(status), Is.False);
			Assert.That(target.Statuses.Count, Is.EqualTo(0));
		}

		[Test]
		public void Apply_WithZeroConsumedStacks_ConsumesOneStack()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("focus");
			target.Statuses.Add(status, p_stackCount: 3);

			new ConsumeStatus
			{
				Status = status,
				NbOfStackConsumed = 0
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Contains(status), Is.True);
			Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(2));
		}

		[Test]
		public void Apply_WithNegativeConsumedStacks_ConsumesOneStack()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("focus");
			target.Statuses.Add(status, p_stackCount: 3);

			new ConsumeStatus
			{
				Status = status,
				NbOfStackConsumed = -2
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Contains(status), Is.True);
			Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(2));
		}

		[Test]
		public void Apply_WhenTargetDoesNotHaveStatus_DoesNotRemoveOtherStatuses()
		{
			BattleUnit target = CreateUnit();
			Status consumedStatus = CreateStatus("focus");
			Status otherStatus = CreateStatus("guard");
			target.Statuses.Add(otherStatus, p_stackCount: 2);

			new ConsumeStatus
			{
				Status = consumedStatus,
				NbOfStackConsumed = 1
			}.Apply(CreateContext(p_target: target));

			Assert.That(target.Statuses.Count, Is.EqualTo(1));
			Assert.That(target.Statuses.Contains(otherStatus), Is.True);
			Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(2));
		}

		[Test]
		public void Apply_WithNullStatus_DoesNotThrowAndDoesNotRemoveStatuses()
		{
			BattleUnit target = CreateUnit();
			Status otherStatus = CreateStatus("guard");
			target.Statuses.Add(otherStatus, p_stackCount: 2);

			Assert.DoesNotThrow(() =>
			{
				new ConsumeStatus
				{
					Status = null,
					NbOfStackConsumed = 1
				}.Apply(CreateContext(p_target: target));
			});

			Assert.That(target.Statuses.Count, Is.EqualTo(1));
			Assert.That(target.Statuses.Contains(otherStatus), Is.True);
			Assert.That(target.Statuses[0].Value.Stack, Is.EqualTo(2));
		}

		[Test]
		public void Apply_WhenTargetHasNoStatuses_DoesNotThrow()
		{
			BattleUnit target = CreateUnit();
			Status status = CreateStatus("focus");

			Assert.DoesNotThrow(() =>
			{
				new ConsumeStatus
				{
					Status = status,
					NbOfStackConsumed = 1
				}.Apply(CreateContext(p_target: target));
			});

			Assert.That(target.Statuses.Count, Is.EqualTo(0));
		}

		[Test]
		public void Apply_WhenTargetIsNull_DoesNotThrow()
		{
			Status status = CreateStatus("focus");

			Assert.DoesNotThrow(() =>
			{
				new ConsumeStatus
				{
					Status = status,
					NbOfStackConsumed = 1
				}.Apply(CreateContext());
			});
		}
	}
}
