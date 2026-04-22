using NUnit.Framework;

public sealed class SetupPhaseTests
{
	[Test]
	public void Enter_TransitionsToPlacement_AndInitializesTurnBarsWithinBounds()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 2, enemyCount: 2);
		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();

		Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.Placement));

		for (int index = 0; index < fixture.BattleContext.AllUnits.Count; index++)
		{
			BattleUnit unit = fixture.BattleContext.AllUnits[index];
			Assert.That(unit.BattleAttributes.TurnBar.Current, Is.GreaterThanOrEqualTo(0f));
			Assert.That(unit.BattleAttributes.TurnBar.Current, Is.LessThanOrEqualTo(unit.BattleAttributes.TurnBar.Max));
		}
	}

	[Test]
	public void Enter_TransitionsToEnd_WhenOneSideHasNoLivingUnits()
	{
		using BattlePhaseTestFixture fixture = BattlePhaseTestFixture.Create(playerCount: 1, enemyCount: 1);
		fixture.EnemyUnits[0].BattleAttributes.Health.SetCurrent(0, true);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();

		Assert.That(orchestrator.Coordinator.CurrentPhaseType, Is.EqualTo(BattlePhaseType.End));
	}
}
