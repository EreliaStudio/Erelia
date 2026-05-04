using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

internal static class PositionTestHelpers
{
	public static BattlePhaseTestFixture BuildFixture(int playerCount, int enemyCount)
	{
		return BattlePhaseTestFixture.Create(
			playerCount: playerCount,
			enemyCount: enemyCount,
			defaultHealth: 50,
			defaultActionPoints: 2,
			defaultMovement: 0);
	}

	public static void PlaceAt(BattlePhaseTestFixture fixture, BattleUnit unit, int x, int z)
	{
		fixture.BattleContext.TryPlaceUnit(unit, new Vector3Int(x, 0, z));
	}

	public static FeatRequirementProgress MakeProgress(FeatRequirement requirement)
	{
		return new FeatRequirementProgress { Requirement = requirement };
	}

	public static TEvent FindEvent<TEvent>(BattleUnit unit) where TEvent : FeatRequirement.EventBase
	{
		IReadOnlyList<FeatRequirement.EventBase> events = unit.PendingFeatEvents;
		for (int i = 0; i < events.Count; i++)
		{
			if (events[i] is TEvent typedEvent)
				return typedEvent;
		}
		return null;
	}
}

namespace Tests.Requirements.Position.TurnStartPosition
{
	public sealed class TurnStartPositionTests
	{
		// ── Event emission ────────────────────────────────────────────────────────

		[Test]
		public void BeginTurn_EmitsEvent()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
			PositionTestHelpers.PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

			BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnStartPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev, Is.Not.Null);
		}

		[Test]
		public void BeginTurn_ClosestEnemyDistance_MatchesManhattanDistance()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
			PositionTestHelpers.PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

			BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnStartPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev.ClosestEnemyDistance, Is.EqualTo(3));
		}

		[Test]
		public void BeginTurn_ClosestAllyDistance_UsesNearestAlly()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 2, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[1], 0, 2);
			PositionTestHelpers.PlaceAt(fixture, fixture.EnemyUnits[0], 0, 5);

			BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnStartPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev.ClosestAllyDistance, Is.EqualTo(2));
		}

		[Test]
		public void BeginTurn_ClosestAllyDistance_IsMaxValue_WhenNoAllyOnBoard()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
			PositionTestHelpers.PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

			BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnStartPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev.ClosestAllyDistance, Is.EqualTo(int.MaxValue));
		}

		[Test]
		public void BeginTurn_ClosestEnemyDistance_IsMaxValue_WhenNoEnemyOnBoard()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 2, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[1], 0, 2);

			BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnStartPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev.ClosestEnemyDistance, Is.EqualTo(int.MaxValue));
		}

		[Test]
		public void BeginTurn_NoEventEmitted_WhenUnitHasNoBoardPosition()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

			BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnStartPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev, Is.Null);
		}

		[Test]
		public void BeginTurn_NoEventEmitted_WhenNoOtherUnitIsOnBoard()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);

			BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnStartPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnStartPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev, Is.Null);
		}

		// ── Requirement evaluation ────────────────────────────────────────────────

		[Test]
		public void Within_Enemy_Completes_WhenCloseEnough()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Enemy,
				Condition = TurnStartPositionRequirement.DistanceKind.Within,
				Distance = 3
			});

			progress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 3, ClosestAllyDistance = int.MaxValue
			}});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void Within_Enemy_DoesNotComplete_WhenTooFar()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Enemy,
				Condition = TurnStartPositionRequirement.DistanceKind.Within,
				Distance = 3
			});

			progress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 4, ClosestAllyDistance = int.MaxValue
			}});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void AtLeast_Enemy_Completes_WhenFarEnough()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Enemy,
				Condition = TurnStartPositionRequirement.DistanceKind.AtLeast,
				Distance = 5
			});

			progress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 5, ClosestAllyDistance = int.MaxValue
			}});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void AtLeast_Enemy_DoesNotComplete_WhenTooClose()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Enemy,
				Condition = TurnStartPositionRequirement.DistanceKind.AtLeast,
				Distance = 5
			});

			progress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 4, ClosestAllyDistance = int.MaxValue
			}});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void Between_Enemy_Completes_WhenInsideInclusiveRange()
		{
			var req = new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Enemy,
				Condition = TurnStartPositionRequirement.DistanceKind.Between,
				Distance = 2,
				MaximumDistance = 5
			};

			var lowerProgress = PositionTestHelpers.MakeProgress(req);
			lowerProgress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 2, ClosestAllyDistance = int.MaxValue
			}});

			var upperProgress = PositionTestHelpers.MakeProgress(req);
			upperProgress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 5, ClosestAllyDistance = int.MaxValue
			}});

			Assert.That(lowerProgress.IsCompleted, Is.True);
			Assert.That(upperProgress.IsCompleted, Is.True);
		}

		[Test]
		public void Between_Enemy_DoesNotComplete_WhenOutsideRange()
		{
			var req = new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Enemy,
				Condition = TurnStartPositionRequirement.DistanceKind.Between,
				Distance = 2,
				MaximumDistance = 5
			};

			var tooCloseProgress = PositionTestHelpers.MakeProgress(req);
			tooCloseProgress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 1, ClosestAllyDistance = int.MaxValue
			}});

			var tooFarProgress = PositionTestHelpers.MakeProgress(req);
			tooFarProgress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 6, ClosestAllyDistance = int.MaxValue
			}});

			Assert.That(tooCloseProgress.IsCompleted, Is.False);
			Assert.That(tooFarProgress.IsCompleted, Is.False);
		}

		[Test]
		public void Within_Ally_UsesAllyDistance()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Ally,
				Condition = TurnStartPositionRequirement.DistanceKind.Within,
				Distance = 2
			});

			progress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestAllyDistance = 2, ClosestEnemyDistance = 10
			}});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void Within_AnyUnit_UsesSmallerOfAllyAndEnemy()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.AnyUnit,
				Condition = TurnStartPositionRequirement.DistanceKind.Within,
				Distance = 3
			});

			progress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestAllyDistance = 10, ClosestEnemyDistance = 2
			}});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void Within_Ally_DoesNotComplete_WhenNoAllyExists()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Ally,
				Condition = TurnStartPositionRequirement.DistanceKind.Within,
				Distance = 3
			});

			progress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestAllyDistance = int.MaxValue, ClosestEnemyDistance = 1
			}});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void AtLeast_Ally_Completes_WhenNoAllyExists()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Ally,
				Condition = TurnStartPositionRequirement.DistanceKind.AtLeast,
				Distance = 5
			});

			progress.RegisterEvents(new[] { new TurnStartPositionRequirement.Event
			{
				ClosestAllyDistance = int.MaxValue, ClosestEnemyDistance = 1
			}});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void AbilityScope_FailingEventsDoNotCombine()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Enemy,
				Condition = TurnStartPositionRequirement.DistanceKind.Within,
				Distance = 2
			});

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 5 },
				new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 5 }
			});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void AbilityScope_CompletesOnFirstPassingEvent()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Enemy,
				Condition = TurnStartPositionRequirement.DistanceKind.Within,
				Distance = 2
			});

			progress.RegisterEvents(new List<FeatRequirement.EventBase>
			{
				new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 5 },
				new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 1 }
			});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void StartEvent_IgnoredBy_EndRequirement()
		{
			var endProgress = PositionTestHelpers.MakeProgress(new TurnEndPositionRequirement
			{
				Target = TurnEndPositionRequirement.TargetKind.Enemy,
				Condition = TurnEndPositionRequirement.DistanceKind.Within,
				Distance = 10
			});

			endProgress.RegisterEvents(new[] { (FeatRequirement.EventBase)new TurnStartPositionRequirement.Event { ClosestEnemyDistance = 1 } });

			Assert.That(endProgress.IsCompleted, Is.False);
		}

		// ── Integration ───────────────────────────────────────────────────────────

		[Test]
		public void PlayerVictory_NodeCompleted_WhenConditionWasMet()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);

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
			fixture.PlayerUnits[0].RecordFeatEvent(new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 2, ClosestAllyDistance = int.MaxValue
			});

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], posNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

			orchestrator.Dispose();
		}

		[Test]
		public void PlayerVictory_NodeNotCompleted_WhenConditionWasNeverMet()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);

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
			fixture.PlayerUnits[0].RecordFeatEvent(new TurnStartPositionRequirement.Event
			{
				ClosestEnemyDistance = 5, ClosestAllyDistance = int.MaxValue
			});

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], posNode);
			bool completed = nodeProgress != null && nodeProgress.CompletionCount > 0;
			Assert.That(completed, Is.False);

			orchestrator.Dispose();
		}

		[Test]
		public void BeginTurnEvent_CompletesNode_ThroughFullBattleFlow()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);

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

			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
			PositionTestHelpers.PlaceAt(fixture, fixture.EnemyUnits[0], 0, 3);

			BattleTurnRules.BeginTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			fixture.EnemyUnits[0].BattleAttributes.Health.Decrease(1000);
			fixture.BattleContext.DefeatUnit(fixture.EnemyUnits[0]);

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], posNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

			orchestrator.Dispose();
		}
	}
}

namespace Tests.Requirements.Position.TurnEndPosition
{
	public sealed class TurnEndPositionTests
	{
		// ── Event emission ────────────────────────────────────────────────────────

		[Test]
		public void EndTurn_EmitsEvent()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);
			PositionTestHelpers.PlaceAt(fixture, fixture.EnemyUnits[0], 0, 4);

			BattleTurnRules.EndTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnEndPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnEndPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev, Is.Not.Null);
		}

		[Test]
		public void EndTurn_ClosestEnemyDistance_MatchesManhattanDistance()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 1, 0);
			PositionTestHelpers.PlaceAt(fixture, fixture.EnemyUnits[0], 3, 2);

			BattleTurnRules.EndTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnEndPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnEndPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev.ClosestEnemyDistance, Is.EqualTo(4));
		}

		[Test]
		public void EndTurn_NoEventEmitted_WhenNoOtherUnitIsOnBoard()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);
			PositionTestHelpers.PlaceAt(fixture, fixture.PlayerUnits[0], 0, 0);

			BattleTurnRules.EndTurn(fixture.BattleContext, fixture.PlayerUnits[0]);

			TurnEndPositionRequirement.Event ev =
				PositionTestHelpers.FindEvent<TurnEndPositionRequirement.Event>(fixture.PlayerUnits[0]);
			Assert.That(ev, Is.Null);
		}

		// ── Requirement evaluation ────────────────────────────────────────────────

		[Test]
		public void Within_Enemy_Completes_WhenCloseEnough()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnEndPositionRequirement
			{
				Target = TurnEndPositionRequirement.TargetKind.Enemy,
				Condition = TurnEndPositionRequirement.DistanceKind.Within,
				Distance = 2
			});

			progress.RegisterEvents(new[] { new TurnEndPositionRequirement.Event
			{
				ClosestEnemyDistance = 2, ClosestAllyDistance = int.MaxValue
			}});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void AtLeast_Enemy_DoesNotComplete_WhenTooClose()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnEndPositionRequirement
			{
				Target = TurnEndPositionRequirement.TargetKind.Enemy,
				Condition = TurnEndPositionRequirement.DistanceKind.AtLeast,
				Distance = 4
			});

			progress.RegisterEvents(new[] { new TurnEndPositionRequirement.Event
			{
				ClosestEnemyDistance = 3, ClosestAllyDistance = int.MaxValue
			}});

			Assert.That(progress.IsCompleted, Is.False);
		}

		[Test]
		public void Between_Ally_Completes_WhenInsideInclusiveRange()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnEndPositionRequirement
			{
				Target = TurnEndPositionRequirement.TargetKind.Ally,
				Condition = TurnEndPositionRequirement.DistanceKind.Between,
				Distance = 2,
				MaximumDistance = 4
			});

			progress.RegisterEvents(new[] { new TurnEndPositionRequirement.Event
			{
				ClosestAllyDistance = 3, ClosestEnemyDistance = 10
			}});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void Between_AnyUnit_AcceptsReversedBounds()
		{
			var progress = PositionTestHelpers.MakeProgress(new TurnEndPositionRequirement
			{
				Target = TurnEndPositionRequirement.TargetKind.AnyUnit,
				Condition = TurnEndPositionRequirement.DistanceKind.Between,
				Distance = 5,
				MaximumDistance = 2
			});

			progress.RegisterEvents(new[] { new TurnEndPositionRequirement.Event
			{
				ClosestAllyDistance = 9, ClosestEnemyDistance = 4
			}});

			Assert.That(progress.IsCompleted, Is.True);
		}

		[Test]
		public void EndEvent_IgnoredBy_StartRequirement()
		{
			var startProgress = PositionTestHelpers.MakeProgress(new TurnStartPositionRequirement
			{
				Target = TurnStartPositionRequirement.TargetKind.Enemy,
				Condition = TurnStartPositionRequirement.DistanceKind.Within,
				Distance = 10
			});

			startProgress.RegisterEvents(new[] { (FeatRequirement.EventBase)new TurnEndPositionRequirement.Event { ClosestEnemyDistance = 1 } });

			Assert.That(startProgress.IsCompleted, Is.False);
		}

		// ── Integration ───────────────────────────────────────────────────────────

		[Test]
		public void PlayerVictory_NodeCompleted_WhenConditionWasMet()
		{
			using BattlePhaseTestFixture fixture = PositionTestHelpers.BuildFixture(playerCount: 1, enemyCount: 1);

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
			fixture.PlayerUnits[0].RecordFeatEvent(new TurnEndPositionRequirement.Event
			{
				ClosestEnemyDistance = 5, ClosestAllyDistance = int.MaxValue
			});

			orchestrator.TransitionTo(BattlePhaseType.End);

			FeatNodeProgress nodeProgress = FeatProgressionService.FindNodeProgress(fixture.PlayerSources[0], posNode);
			Assert.That(nodeProgress, Is.Not.Null);
			Assert.That(nodeProgress.CompletionCount, Is.GreaterThan(0));

			orchestrator.Dispose();
		}
	}
}
