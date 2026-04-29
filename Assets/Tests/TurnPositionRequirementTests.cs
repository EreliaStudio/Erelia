using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tests for position-based feat requirements:
///   - TurnStartPositionRequirement / TurnEndPositionRequirement
/// Organised into three groups:
///   1. Event emission  — BeginTurn / EndTurn write the correct distances into the unit
///   2. Requirement evaluation — ComputeProgress correctly accepts / rejects events
///   3. End-phase integration — nodes complete (or not) on player victory
/// </summary>
public sealed class TurnPositionRequirementTests
{
	// -------------------------------------------------------------------------
	// Group 1 — Event emission via BattleTurnRules
	// -------------------------------------------------------------------------

	[Test]
	public void BeginTurn_EmitsTurnStartPositionEvent()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);
		PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
		PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

		BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		BattleUnit player = fixture.PlayerUnits[0];
		TurnStartPositionRequirement.Event ev = FindEvent<TurnStartPositionRequirement.Event>(player);
		Assert.That(ev, Is.Not.Null, "BeginTurn must emit a TurnStartPositionRequirement.Event.");
	}

	[Test]
	public void BeginTurn_ClosestEnemyDistance_MatchesManhattanDistance()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);
		PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
		PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

		BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		TurnStartPositionRequirement.Event ev =
			FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
		Assert.That(ev.ClosestEnemyDistance, Is.EqualTo(3));
	}

	[Test]
	public void BeginTurn_ClosestAllyDistance_UsesNearestAlly()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 2, enemyCount: 1);
		PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
		PlaceAt(fixture, fixture.PlayerUnits[1], 0, 2);
		PlaceAt(fixture, fixture.EnemyUnits[0], 0, 5);

		BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		TurnStartPositionRequirement.Event ev =
			FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
		Assert.That(ev.ClosestAllyDistance, Is.EqualTo(2));
	}

	[Test]
	public void BeginTurn_ClosestAllyDistance_IsMaxValue_WhenNoAllyOnBoard()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);
		PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
		PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

		BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		TurnStartPositionRequirement.Event ev =
			FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
		Assert.That(ev.ClosestAllyDistance, Is.EqualTo(int.MaxValue));
	}

	[Test]
	public void BeginTurn_ClosestEnemyDistance_IsMaxValue_WhenNoEnemyOnBoard()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 2, enemyCount: 1);
		PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
		PlaceAt(fixture, fixture.PlayerUnits[1], 0, 2);
		// Enemy is not placed — HasBoardPosition is false, so excluded from distances

		BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		TurnStartPositionRequirement.Event ev =
			FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
		Assert.That(ev.ClosestEnemyDistance, Is.EqualTo(int.MaxValue));
	}

	[Test]
	public void EndTurn_EmitsTurnEndPositionEvent()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);
		PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
		PlaceAt(fixture, fixture.EnemyUnits[0], 0, 4);

		BattleTurnRules.EndTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		TurnEndPositionRequirement.Event ev =
			FindEvent<TurnEndPositionRequirement.Event>(fixture.PlayerUnits[0]);
		Assert.That(ev, Is.Not.Null, "EndTurn must emit a TurnEndPositionRequirement.Event.");
	}

	[Test]
	public void EndTurn_ClosestEnemyDistance_MatchesManhattanDistance()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);
		PlaceAt(fixture, fixture.PlayerUnits[0], 1, 0);
		PlaceAt(fixture, fixture.EnemyUnits[0], 3, 2);

		BattleTurnRules.EndTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		TurnEndPositionRequirement.Event ev =
			FindEvent<TurnEndPositionRequirement.Event>(fixture.PlayerUnits[0]);
		// Manhattan: |3-1| + |2-0| = 4
		Assert.That(ev.ClosestEnemyDistance, Is.EqualTo(4));
	}

	[Test]
	public void PositionEvent_NotEmitted_WhenUnitHasNoBoardPosition()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);
		// Player is not placed — HasBoardPosition is false
		PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

		BattleUnit player = fixture.PlayerUnits[0];
		BattleTurnRules.BeginTurn(fixture.BattleContext, player);

		TurnStartPositionRequirement.Event ev =
			FindEvent<TurnStartPositionRequirement.Event>(player);
		Assert.That(ev, Is.Null, "No position event should be emitted when the unit has no board position.");
	}

	[Test]
	public void BeginTurn_PositionEvent_NotEmitted_WhenNoOtherUnitIsOnBoard()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);
		// Only the player is placed; enemy has no board position
		PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);

		BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		TurnStartPositionRequirement.Event ev =
			FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
		Assert.That(ev, Is.Null,
			"No start-position event should be emitted when no ally or enemy is on the board.");
	}

	[Test]
	public void EndTurn_PositionEvent_NotEmitted_WhenNoOtherUnitIsOnBoard()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);
		PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);

		BattleTurnRules.EndTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		TurnEndPositionRequirement.Event ev =
			FindEvent<TurnEndPositionRequirement.Event>(fixture.PlayerUnits[0]);
		Assert.That(ev, Is.Null,
			"No end-position event should be emitted when no ally or enemy is on the board.");
	}

	// -------------------------------------------------------------------------
	// Group 2 — Requirement evaluation
	// -------------------------------------------------------------------------

	[Test]
	public void TurnStartRequirement_Within_Enemy_ReturnsFullProgress_WhenCloseEnough()
	{
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Enemy,
			Condition = TurnStartPositionRequirement.DistanceKind.Within,
			Distance = 3
		};

		float progress = req.Register(new TurnStartPositionRequirement.Event
		{
			ClosestEnemyDistance = 3,
			ClosestAllyDistance = int.MaxValue
		});

		Assert.That(progress, Is.EqualTo(100f));
	}

	[Test]
	public void TurnStartRequirement_Within_Enemy_ReturnsZero_WhenTooFar()
	{
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Enemy,
			Condition = TurnStartPositionRequirement.DistanceKind.Within,
			Distance = 3
		};

		float progress = req.Register(new TurnStartPositionRequirement.Event
		{
			ClosestEnemyDistance = 4,
			ClosestAllyDistance = int.MaxValue
		});

		Assert.That(progress, Is.EqualTo(0f));
	}

	[Test]
	public void TurnStartRequirement_AtLeast_Enemy_ReturnsFullProgress_WhenFarEnough()
	{
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Enemy,
			Condition = TurnStartPositionRequirement.DistanceKind.AtLeast,
			Distance = 5
		};

		float progress = req.Register(new TurnStartPositionRequirement.Event
		{
			ClosestEnemyDistance = 5,
			ClosestAllyDistance = int.MaxValue
		});

		Assert.That(progress, Is.EqualTo(100f));
	}

	[Test]
	public void TurnStartRequirement_AtLeast_Enemy_ReturnsZero_WhenTooClose()
	{
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Enemy,
			Condition = TurnStartPositionRequirement.DistanceKind.AtLeast,
			Distance = 5
		};

		float progress = req.Register(new TurnStartPositionRequirement.Event
		{
			ClosestEnemyDistance = 4,
			ClosestAllyDistance = int.MaxValue
		});

		Assert.That(progress, Is.EqualTo(0f));
	}

	[Test]
	public void TurnStartRequirement_Within_Ally_UsesAllyDistance()
	{
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Ally,
			Condition = TurnStartPositionRequirement.DistanceKind.Within,
			Distance = 2
		};

		// Ally at distance 2 (satisfies), enemy at distance 10 (irrelevant)
		float progress = req.Register(new TurnStartPositionRequirement.Event
		{
			ClosestAllyDistance = 2,
			ClosestEnemyDistance = 10
		});

		Assert.That(progress, Is.EqualTo(100f));
	}

	[Test]
	public void TurnStartRequirement_Within_AnyUnit_UsesSmallerOfAllyAndEnemy()
	{
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.AnyUnit,
			Condition = TurnStartPositionRequirement.DistanceKind.Within,
			Distance = 3
		};

		// Ally is far but enemy is close — AnyUnit should pick the minimum
		float progress = req.Register(new TurnStartPositionRequirement.Event
		{
			ClosestAllyDistance = 10,
			ClosestEnemyDistance = 2
		});

		Assert.That(progress, Is.EqualTo(100f));
	}

	[Test]
	public void TurnStartRequirement_Within_Ally_ReturnsZero_WhenNoAllyExists()
	{
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Ally,
			Condition = TurnStartPositionRequirement.DistanceKind.Within,
			Distance = 3
		};

		float progress = req.Register(new TurnStartPositionRequirement.Event
		{
			ClosestAllyDistance = int.MaxValue,
			ClosestEnemyDistance = 1
		});

		Assert.That(progress, Is.EqualTo(0f));
	}

	[Test]
	public void TurnStartRequirement_AtLeast_Ally_ReturnsFullProgress_WhenNoAllyExists()
	{
		// int.MaxValue >= Distance is always true — no allies means "infinitely far"
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Ally,
			Condition = TurnStartPositionRequirement.DistanceKind.AtLeast,
			Distance = 5
		};

		float progress = req.Register(new TurnStartPositionRequirement.Event
		{
			ClosestAllyDistance = int.MaxValue,
			ClosestEnemyDistance = 1
		});

		Assert.That(progress, Is.EqualTo(100f));
	}

	[Test]
	public void TurnEndRequirement_Within_Enemy_ReturnsFullProgress_WhenCloseEnough()
	{
		var req = new TurnEndPositionRequirement
		{
			Target = TurnEndPositionRequirement.TargetKind.Enemy,
			Condition = TurnEndPositionRequirement.DistanceKind.Within,
			Distance = 2
		};

		float progress = req.Register(new TurnEndPositionRequirement.Event
		{
			ClosestEnemyDistance = 2,
			ClosestAllyDistance = int.MaxValue
		});

		Assert.That(progress, Is.EqualTo(100f));
	}

	[Test]
	public void TurnEndRequirement_AtLeast_Enemy_ReturnsZero_WhenTooClose()
	{
		var req = new TurnEndPositionRequirement
		{
			Target = TurnEndPositionRequirement.TargetKind.Enemy,
			Condition = TurnEndPositionRequirement.DistanceKind.AtLeast,
			Distance = 4
		};

		float progress = req.Register(new TurnEndPositionRequirement.Event
		{
			ClosestEnemyDistance = 3,
			ClosestAllyDistance = int.MaxValue
		});

		Assert.That(progress, Is.EqualTo(0f));
	}

	[Test]
	public void PositionRequirement_MaximumMode_DoesNotAccumulateAcrossEvents()
	{
		// Two failing events must not combine to 100%
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Enemy,
			Condition = TurnStartPositionRequirement.DistanceKind.Within,
			Distance = 2
		};

		var progress = new FeatRequirementProgress { Requirement = req, CurrentProgress = 0f };
		progress.Register(new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 5 }); // fail
		progress.Register(new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 5 }); // fail

		Assert.That(progress.CurrentProgress, Is.EqualTo(0f));
	}

	[Test]
	public void PositionRequirement_MaximumMode_CompletesOnFirstPassingEvent()
	{
		var req = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Enemy,
			Condition = TurnStartPositionRequirement.DistanceKind.Within,
			Distance = 2
		};

		var progress = new FeatRequirementProgress { Requirement = req, CurrentProgress = 0f };
		progress.Register(new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 5 }); // fail
		progress.Register(new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 1 }); // pass

		Assert.That(progress.CurrentProgress, Is.EqualTo(100f));
	}

	[Test]
	public void TurnStartEvent_IgnoredBy_TurnEndRequirement_AndViceVersa()
	{
		var startReq = new TurnStartPositionRequirement
		{
			Target = TurnStartPositionRequirement.TargetKind.Enemy,
			Condition = TurnStartPositionRequirement.DistanceKind.Within,
			Distance = 10
		};
		var endReq = new TurnEndPositionRequirement
		{
			Target = TurnEndPositionRequirement.TargetKind.Enemy,
			Condition = TurnEndPositionRequirement.DistanceKind.Within,
			Distance = 10
		};

		var startEvent = new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 1 };
		var endEvent = new TurnEndPositionRequirement.Event { ClosestEnemyDistance = 1 };

		// Start req ignores end event
		Assert.That(startReq.Register(endEvent), Is.EqualTo(0f));
		// End req ignores start event
		Assert.That(endReq.Register(startEvent), Is.EqualTo(0f));
	}

	// -------------------------------------------------------------------------
	// Group 3 — End-phase integration
	// -------------------------------------------------------------------------

	[Test]
	public void PlayerVictory_TurnStartPositionNode_Completed_WhenConditionWasMet()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);

		FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
		FeatNode posNode = new FeatNode
		{
			Id = "pos_node",
			DisplayName = "Close Quarters Start",
			Requirements = new List<FeatRequirement>
			{
				new TurnStartPositionRequirement
				{
					Target = TurnStartPositionRequirement.TargetKind.Enemy,
					Condition = TurnStartPositionRequirement.DistanceKind.Within,
					Distance = 3
				}
			},
			Rewards = new List<FeatReward>
			{
				new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
			},
			NeighbourNodeIds = new List<string> { rootNode.Id }
		};
		rootNode.NeighbourNodeIds.Add(posNode.Id);

		fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
		{
			Nodes = new List<FeatNode> { rootNode, posNode },
			RootNodeId = rootNode.Id
		};
		FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
		fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

		// Inject a passing position event (distance 2 <= threshold 3)
		fixture.PlayerUnits[0].RecordFeatEvent(new TurnStartPositionRequirement.Event
		{
			ClosestEnemyDistance = 2,
			ClosestAllyDistance = int.MaxValue
		});

		orchestrator.TransitionTo(BattlePhaseType.End);

		FeatNodeProgress nodeProgress =
			FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], posNode);
		Assert.That(nodeProgress, Is.Not.Null);
		Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

		orchestrator.Dispose();
	}

	[Test]
	public void PlayerVictory_TurnStartPositionNode_NotCompleted_WhenConditionWasNeverMet()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);

		FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
		FeatNode posNode = new FeatNode
		{
			Id = "pos_node",
			DisplayName = "Close Quarters Start",
			Requirements = new List<FeatRequirement>
			{
				new TurnStartPositionRequirement
				{
					Target = TurnStartPositionRequirement.TargetKind.Enemy,
					Condition = TurnStartPositionRequirement.DistanceKind.Within,
					Distance = 3
				}
			},
			Rewards = new List<FeatReward>
			{
				new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
			},
			NeighbourNodeIds = new List<string> { rootNode.Id }
		};
		rootNode.NeighbourNodeIds.Add(posNode.Id);

		fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
		{
			Nodes = new List<FeatNode> { rootNode, posNode },
			RootNodeId = rootNode.Id
		};
		FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
		fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

		// Distance 5 never satisfied the within-3 condition
		fixture.PlayerUnits[0].RecordFeatEvent(new TurnStartPositionRequirement.Event
		{
			ClosestEnemyDistance = 5,
			ClosestAllyDistance = int.MaxValue
		});

		orchestrator.TransitionTo(BattlePhaseType.End);

		FeatNodeProgress nodeProgress =
			FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], posNode);
		bool completed = nodeProgress != null && nodeProgress.CompletionCount > 0;
		Assert.That(completed, Is.False);

		orchestrator.Dispose();
	}

	[Test]
	public void PlayerVictory_TurnEndPositionNode_Completed_WhenConditionWasMet()
	{
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);

		FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
		FeatNode posNode = new FeatNode
		{
			Id = "pos_node",
			DisplayName = "Safe Distance End",
			Requirements = new List<FeatRequirement>
			{
				new TurnEndPositionRequirement
				{
					Target = TurnEndPositionRequirement.TargetKind.Enemy,
					Condition = TurnEndPositionRequirement.DistanceKind.AtLeast,
					Distance = 4
				}
			},
			Rewards = new List<FeatReward>
			{
				new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
			},
			NeighbourNodeIds = new List<string> { rootNode.Id }
		};
		rootNode.NeighbourNodeIds.Add(posNode.Id);

		fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
		{
			Nodes = new List<FeatNode> { rootNode, posNode },
			RootNodeId = rootNode.Id
		};
		FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
		fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

		// Distance 5 >= threshold 4 → condition met
		fixture.PlayerUnits[0].RecordFeatEvent(new TurnEndPositionRequirement.Event
		{
			ClosestEnemyDistance = 5,
			ClosestAllyDistance = int.MaxValue
		});

		orchestrator.TransitionTo(BattlePhaseType.End);

		FeatNodeProgress nodeProgress =
			FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], posNode);
		Assert.That(nodeProgress, Is.Not.Null);
		Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

		orchestrator.Dispose();
	}

	[Test]
	public void BeginTurn_EmittedEvent_CompletesPositionNode_ThroughFullBattleFlow()
	{
		// Verify the event emitted by BeginTurn propagates correctly to feat nodes
		// when applied at end of battle.
		using BattlePhaseTestFixture fixture = BuildFixture(playerCount: 1, enemyCount: 1);

		FeatNode rootNode = new FeatNode { Id = "root", DisplayName = "Root" };
		FeatNode posNode = new FeatNode
		{
			Id = "pos_node",
			DisplayName = "Close Start",
			Requirements = new List<FeatRequirement>
			{
				new TurnStartPositionRequirement
				{
					Target = TurnStartPositionRequirement.TargetKind.Enemy,
					Condition = TurnStartPositionRequirement.DistanceKind.Within,
					Distance = 4
				}
			},
			Rewards = new List<FeatReward>
			{
				new BonusStatsReward { Attribute = BonusStatsReward.AttributeType.Health, Value = 5 }
			},
			NeighbourNodeIds = new List<string> { rootNode.Id }
		};
		rootNode.NeighbourNodeIds.Add(posNode.Id);

		fixture.PlayerSources[0].Species.FeatBoard = new FeatBoard
		{
			Nodes = new List<FeatNode> { rootNode, posNode },
			RootNodeId = rootNode.Id
		};
		FeatProgressionService.InitializeCreatureUnit(fixture.PlayerSources[0]);

		BattleOrchestrator orchestrator = fixture.CreateInitializedOrchestrator();
		fixture.CompletePlacement(
			orchestrator,
			playerTurnBars: new[] { fixture.PlayerUnits[0].BattleAttributes.TurnBar.Max },
			enemyTurnBars: new[] { 0f });

		// Place units 3 apart (within the 4-distance threshold)
		PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
		PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

		// Call BeginTurn directly — this is what emits the event with real board distances
		BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

		// Now defeat enemy for a player victory
		fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
		fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

		orchestrator.TransitionTo(BattlePhaseType.End);

		FeatNodeProgress nodeProgress =
			FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], posNode);
		Assert.That(nodeProgress, Is.Not.Null);
		Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0),
			"BeginTurn-emitted event should satisfy the TurnStartPositionRequirement node.");

		orchestrator.Dispose();
	}

	// -------------------------------------------------------------------------
	// Helpers
	// -------------------------------------------------------------------------

	private static BattlePhaseTestFixture BuildFixture(int playerCount, int enemyCount)
	{
		return BattlePhaseTestFixture.Create(
			playerCount: playerCount,
			enemyCount: enemyCount,
			defaultHealth: 50,
			defaultActionPoints: 2,
			defaultMovement: 0);
	}

	private static void PlaceAt(BattlePhaseTestFixture fixture, BattleUnit unit, int x, int z)
	{
		fixture.BattleContext.TryPlaceUnit(unit, new Vector3Int(x, 0, z));
	}

	private static TEvent FindEvent<TEvent>(BattleUnit unit) where TEvent : FeatRequirement.EventBase
	{
		IReadOnlyList<FeatRequirement.EventBase> events = unit.PendingFeatEvents;
		for (int i = 0; i < events.Count; i++)
		{
			if (events[i] is TEvent typedEvent)
			{
				return typedEvent;
			}
		}

		return null;
	}
}
